namespace Game.Component.Element;

using Game.Autoload;
using Game.Extension;

/// <summary>
/// 불(Fire) 원소 컴포넌트 - 게임 내 불 번짐, 피해, 시각 효과를 관리합니다.
/// Main.tscn의 플레이어, 좀비 등에 추가되어 불 붙는 기능을 제공합니다.
/// 다른 엔티티와 접촉 시 불을 전파하고, 시간에 따라 에너지가 감소하며, 불이 붙은 대상에게 체력 피해를 입힙니다.
/// </summary>
public partial class FireComponent : ElementComponent
{
	/// <summary>
	/// 부모 노드의 체력 컴포넌트 참조 (불에 의한 피해 적용 시 사용)
	/// </summary>
	private HealthComponent _healthComponent;

	/// <summary>
	/// 이 컴포넌트가 부착된 부모 Node2D (플레이어, 좀비 등)
	/// </summary>
	private Node2D _parent;

	/// <summary>
	/// 불 전파 시 경합 상태(race condition) 방지를 위한 대기 플래그
	/// 불 컴포넌트가 지연 추가되는 동안 중복 추가를 방지합니다.
	/// </summary>
	private bool _wait;

	/// <summary>
	/// 불이 번질 수 있는 영역을 정의하는 Area2D
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// 다른 엔티티가 이 영역에 들어오면 불이 전파됩니다.
	/// </summary>
	[Node]
	public Area2D CatchFireZone;

	/// <summary>
	/// CatchFireZone의 충돌 영역 모양을 정의하는 CollisionShape2D
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// </summary>
	[Node]
	public CollisionShape2D CatchFireZoneCollision;

	/// <summary>
	/// 불 에너지의 감소 속도 (초당 감소율)
	/// Inspector에서 설정 가능 - 음수 값으로 시간이 지나면서 불이 점차 약해집니다.
	/// 기본값: -0.03 (초당 3% 감소)
	/// </summary>
	[Export]
	public float DepletionRate = -0.03f;

	/// <summary>
	/// 불 에너지 감소를 관리하는 타이머
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// 타이머가 만료될 때마다 DepletionRate만큼 에너지가 감소합니다.
	/// </summary>
	[Node]
	public Timer DepletionTimer;

	/// <summary>
	/// 다른 엔티티에 불을 전파할 때 자신이 잃는 에너지 비율
	/// Inspector에서 설정 가능 - 불을 전파하면 자신의 불도 약해집니다.
	/// 기본값: -10.0 (10% 손실)
	/// </summary>
	[Export]
	public float EnergyLossPercent = -10.0f;

	/// <summary>
	/// 다른 엔티티에 불을 전파할 때 전달하는 에너지 비율
	/// Inspector에서 설정 가능 - 자신의 에너지 중 얼마를 상대에게 전달할지 결정합니다.
	/// 기본값: 10.0 (10% 전달)
	/// </summary>
	[Export]
	public float EnergyTransferPercent = 10.0f;

	/// <summary>
	/// 불 시각 효과를 표현하는 CPU 파티클 시스템
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// 불 에너지에 따라 파티클의 수명과 발산 여부가 조절됩니다.
	/// </summary>
	[Node]
	public CpuParticles2D FireParticles;

	/// <summary>
	/// 체력 피해를 주기적으로 적용하는 타이머
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// 타이머가 만료될 때마다 불 에너지에 비례한 피해를 입힙니다.
	/// </summary>
	[Node]
	public Timer HealthDamageTimer;

	/// <summary>
	/// 현재 불 에너지를 화면에 표시하는 레이블 (디버깅용)
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// </summary>
	[Node]
	public Label Label;

	/// <summary>
	/// 불로 인한 최대 체력 피해량 (에너지가 1.0일 때의 피해량)
	/// Inspector에서 설정 가능 - 실제 피해는 현재 에너지 비율에 따라 계산됩니다.
	/// 기본값: 0.05 (5% 피해)
	/// </summary>
	[Export]
	public float MaxHealthDamage = 0.05f;

	/// <summary>
	/// 불 시각 효과(색상, 파티클 등)의 부모 노드
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// 불 에너지에 따라 색상이 동적으로 변경됩니다.
	/// </summary>
	[Node]
	public Node2D Visuals;

	/// <summary>
	/// 노드가 씬 트리에 진입할 때 호출됩니다.
	/// [Node] 속성이 붙은 모든 필드를 자동으로 연결합니다.
	/// </summary>
	public override void _EnterTree()
	{
		this.WireNodes();
	}

	/// <summary>
	/// 노드가 준비되고 모든 자식 노드가 씬에 진입했을 때 호출됩니다.
	/// 타이머 이벤트, 충돌 이벤트 등을 설정합니다.
	/// </summary>
	public override void _Ready()
	{
		// 에너지 감소 타이머 - 타임아웃마다 불 에너지가 DepletionRate만큼 감소
		DepletionTimer.Timeout += () => { AddEnergy(DepletionRate); };
		// 체력 피해 타이머 - 타임아웃마다 불로 인한 피해 적용
		HealthDamageTimer.Timeout += _InflictDamage;

		// 다른 엔티티가 불 전파 영역에 들어올 때 이벤트 연결
		CatchFireZone.BodyEntered += _EnteredCatchFireZone;

		_parent = GetParent() as Node2D;
	}

	/// <summary>
	/// 다른 엔티티가 불 전파 영역(CatchFireZone)에 진입했을 때 호출됩니다.
	/// </summary>
	/// <param name="body">진입한 Node2D 엔티티</param>
	private void _EnteredCatchFireZone(Node2D body)
	{
		_ApplyFire(body);
	}

	/// <summary>
	/// 대상 엔티티에 불을 전파합니다.
	/// 대상에 불 컴포넌트가 없으면 새로 추가하고, 있으면 에너지를 증가시킵니다.
	/// </summary>
	/// <param name="body">불을 전파할 대상 노드</param>
	private void _ApplyFire(Node body)
	{
		// _wait 플래그로 경합 상태 방지 - 지연 호출로 불 컴포넌트를 추가하는 동안
		// GetFirstNodeOfType가 null을 반환하는 상황 방지
		if (_wait || !Enabled) return;

		var fireComponent = body.GetFirstNodeOfType<FireComponent>();

		if (fireComponent == null)
		{
			// 대상에 불 컴포넌트가 없으면 새로 추가
			_wait = true;
			body.AddResourceDeferredWithAction<FireComponent>(fc =>
			{
				// 현재 에너지의 일부를 새 불 컴포넌트에 전달
				fc.SetEnergy(Energy * EnergyTransferPercent / 100f);
				_wait = false;
			});
		}
		else
		{
			// 대상에 이미 불 컴포넌트가 있으면 에너지 추가
			fireComponent.AddEnergy(EnergyTransferPercent / 100f);
			// 자신의 불 에너지는 감소
			AddEnergy(EnergyLossPercent / 100f);
		}
	}

	/// <summary>
	/// 불 에너지를 증가 또는 감소시킵니다.
	/// 부모 클래스의 메서드를 오버라이드하여 컴포넌트 활성화 여부를 확인합니다.
	/// </summary>
	/// <param name="factor">에너지 변화량 (양수: 증가, 음수: 감소)</param>
	/// <param name="emitSignals">true면 에너지 변화 시 시그널 발생</param>
	/// <returns>변경 전 에너지 값</returns>
	public override float AddEnergy(float factor, bool emitSignals = true)
	{
		if (!Enabled) return Energy;

		var oldEnergy = SetEnergy(Energy + factor, emitSignals);
		return oldEnergy;
	}

	/// <summary>
	/// 불 에너지를 특정 값으로 설정합니다.
	/// 시각 효과 업데이트와 상태 변화 알림을 처리합니다.
	/// </summary>
	/// <param name="energy">설정할 에너지 값</param>
	/// <param name="emitSignals">true면 에너지 변화 시 시그널 발생</param>
	/// <returns>변경 전 에너지 값</returns>
	public override float SetEnergy(float energy, bool emitSignals = true)
	{
		if (!Enabled) return Energy;

		var result = base.SetEnergy(energy, emitSignals);

		// 불의 시각적 표현 업데이트 (색상, 레이블 등)
		_UpdateVisuals();

		// 에너지 상태 확인 및 관련 이벤트 발생
		_CheckAndNotify(emitSignals);

		return result;
	}

	/// <summary>
	/// 불 에너지에 따라 시각 효과를 업데이트합니다.
	/// 레이블에 에너지 값을 표시하고, 불의 색상을 에너지 레벨에 맞춰 조정합니다.
	/// </summary>
	private void _UpdateVisuals()
	{
		// 디버그 레이블이 보이면 현재 에너지 표시
		if (Label.Visible)
			Label.Text = $"{Energy}";
		// 에너지가 높을수록 더 밝은 빨간색으로 표시
		Visuals.Modulate = new Color(3.0f * Energy, 2.0f * Energy, 2.0f * Energy);
	}

	/// <summary>
	/// 에너지 상태를 확인하고 필요한 경우 관련 시그널을 발생시킵니다.
	/// 불이 붙거나 꺼질 때, 최대 강도에 도달할 때 이벤트를 발생시킵니다.
	/// </summary>
	/// <param name="emitSignals">true면 시그널 발생 허용</param>
	private void _CheckAndNotify(bool emitSignals)
	{
		if (Energy > 0.0f)
		{
			// 에너지가 0보다 크면 불이 타고 있는 상태
			if (DepletionTimer.IsInsideTree())
			{
				DepletionTimer.Start(); // 에너지 감소 시작
				FireParticles.Emitting = true; // 파티클 효과 활성화
				FireParticles.Lifetime = Energy; // 파티클 수명을 에너지에 맞춤
			}
		}
		else
		{
			// 에너지가 0이면 불이 꺼진 상태
			if (DepletionTimer.IsInsideTree())
			{
				DepletionTimer.Stop(); // 에너지 감소 중지
				FireParticles.Emitting = false; // 파티클 효과 비활성화
				if (emitSignals)
				{
					// 불이 꺼졌음을 알림
					EmitSignal(ElementComponent.SignalName.IntensityDepleted, this);
					GameEvents.EmitElementIntensityDepleted(this);
				}
			}
		}

		// 에너지가 최대치에 도달했을 때
		if (!(Energy >= 1.0f)) return;

		if (!emitSignals) return;
		// 최대 강도 도달을 알림
		EmitSignal(ElementComponent.SignalName.IntensityMaxed, this);
		GameEvents.EmitElementIntensityMaxed(this);
	}

	/// <summary>
	/// 컴포넌트의 활성화 상태가 변경된 후 호출됩니다.
	/// 활성화 상태에 따라 에너지 감소 타이머를 시작하거나 중지합니다.
	/// </summary>
	protected override void _EnabledPostProcess()
	{
		base._EnabledPostProcess();
		// 활성화되면 타이머 시작, 비활성화되면 중지
		if (Enabled) DepletionTimer.Start();
		else DepletionTimer.Stop();
	}

	/// <summary>
	/// 불로 인한 체력 피해를 적용합니다.
	/// 현재 불 에너지에 비례하여 부모 노드의 체력을 감소시킵니다.
	/// </summary>
	private void _InflictDamage()
	{
		if (Energy > 0.0f)
		{
			// 체력 컴포넌트 참조를 캐시 (null 병합 할당 연산자 사용)
			_healthComponent ??= _parent.GetFirstNodeOfType<HealthComponent>();
			// 현재 에너지 비율에 따른 피해 적용
			_healthComponent?.DecreaseHealth(MaxHealthDamage * Energy);
		}
	}

	/// <summary>
	/// 노드가 씬 트리에서 제거될 때 호출됩니다.
	/// 불이 활성화 상태였다면 소진 이벤트를 발생시킵니다.
	/// </summary>
	public override void _ExitTree()
	{
		if (Enabled)
		{
			// 노드 제거 시 불이 꺼졌음을 알림
			EmitSignal(ElementComponent.SignalName.IntensityDepleted, null);
			GameEvents.EmitElementIntensityDepleted(null);
		}
	}
}
