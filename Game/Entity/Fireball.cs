namespace Game.Entity;

using Game.Autoload;
using Game.Component;
using Game.Component.Element;
using Game.Extension;
using Godot.Collections;

/// <summary>
/// 게임의 플레이어 객체인 화염구(Fireball) 클래스
/// Main.tscn의 FireBall 노드로 표현되며, 마우스를 따라가며 주변의 좀비를 태운다.
/// 핫존(HotZone) 범위 내의 좀비에게 불 데미지를 지속적으로 입힌다.
/// </summary>
public partial class Fireball : StaticBody2D
{
	/// <summary>
	/// 좀비에게 입힐 열 에너지 비율
	/// Inspector에서 조정 가능 (기본값: 0.075)
	/// 매 타이머 틱마다 핫존 내의 좀비에게 이 값만큼의 불 데미지를 입힘
	/// </summary>
	[Export]
	public float HeatUpEnergyRate = 0.075f;

	/// <summary>
	/// 핫존 내 접촉 중인 좀비에게 열을 입힐지 여부
	/// Inspector에서 비활성화 가능 (기본값: true)
	/// false면 핫존 내에 있어도 좀비가 데미지를 받지 않음
	/// </summary>
	[Export]
	public bool HeatUpTouching = true;

	/// <summary>마우스를 따라가는 컴포넌트 - Fireball이 플레이어의 마우스 위치로 이동하게 함</summary>
	[Node]
	public FollowMouseComponent FollowMouseComponent;

	/// <summary>이동 속도 및 방향을 관리하는 컴포넌트</summary>
	[Node]
	public VelocityComponent VelocityComponent;

	/// <summary>
	/// 주변 오브젝트에 열을 입히는 영역(Area2D)
	/// 이 영역에 들어온 좀비에게 불 데미지를 입힌다.
	/// </summary>
	[Node]
	public Area2D HotZone;

	/// <summary>Fireball을 시각적으로 나타내는 두개골 스프라이트</summary>
	[Node]
	public Sprite2D Skull;

	/// <summary>
	/// 핫존을 일정 주기로 활성화/업데이트하는 타이머
	/// Timeout 신호를 통해 _HeatUpHotZone 메서드를 호출
	/// </summary>
	[Node]
	public Timer Timer;

	/// <summary>Fireball 뒤를 따라오는 파티클 이펙트 (흔적 표현)</summary>
	[Node]
	public CpuParticles2D TrailingParticles;

	/// <summary>Fireball 주위를 소용돌이치는 파티클 이펙트</summary>
	[Node]
	public CpuParticles2D SwirlingParticles;

	/// <summary>Fireball이 좀비에게 공격받을 때 표시되는 충격 파티클 이펙트</summary>
	[Node]
	public CpuParticles2D HitParticles;

	/// <summary>
	/// 현재 핫존 내에 있는 좀비들과 그들의 FireComponent를 저장하는 딕셔너리
	/// 키: 좀비 노드의 이름, 값: 좀비의 FireComponent
	/// 핫존에 좀비가 들어오면 추가, 나가면 제거됨
	/// </summary>
	private Dictionary<string, FireComponent> _nodesInsideHotZone = new();

	/// <summary>게임 화면의 크기(가로, 세로 픽셀) - 마우스 위치 계산에 사용</summary>
	private Vector2 _screenSize;

	/// <summary>Skull 스프라이트의 원래 크기 - 피격 애니메이션 후 복원에 사용</summary>
	private Vector2 _skullScale;

	/// <summary>
	/// 노드가 씬 트리에 들어올 때 호출되는 라이프사이클 메서드
	/// [Node] 속성으로 표시된 필드들을 자동으로 탐색하고 연결한다.
	/// </summary>
	public override void _EnterTree()
	{
		// [Node] 속성으로 마킹된 필드들을 자동으로 탐색하고 연결
		this.WireNodes();
	}

	/// <summary>
	/// 노드가 준비되었을 때(씬 로드 완료) 호출되는 비동기 라이프사이클 메서드
	/// 게임 초기 설정 및 신호 연결을 수행한다.
	/// </summary>
	public override async void _Ready()
	{
		// 게임 화면의 크기 가져오기
		_screenSize = GetViewportRect().Size;

		// HotZone(Area2D)의 신호 연결 - 좀비가 영역에 들어올 때와 나갈 때
		HotZone.BodyEntered += _EnteredHotZone;  // 좀비가 핫존에 진입
		HotZone.BodyExited += _ExitedHotZone;    // 좀비가 핫존에서 퇴출

		// 타이머의 Timeout 신호 연결 - 일정 주기로 _HeatUpHotZone 호출
		Timer.Timeout += _HeatUpHotZone;

		// GameEvents의 PlayerHit 신호 연결 - Fireball이 피격되었을 때
		GameEvents.Instance.PlayerHit += _hit;

		// Skull의 원래 크기 저장 (피격 애니메이션에서 사용)
		_skullScale = Skull.Scale;

		// 한 프레임 대기 (모든 노드가 준비될 때까지 대기)
		await ToSignal(GetTree(), "process_frame");

		// 마우스를 화면 중앙으로 이동
		Input.WarpMouse(_screenSize / 2.0f);
	}

	/// <summary>
	/// 매 물리 프레임마다 호출되는 라이프사이클 메서드
	/// Fireball의 이동을 처리한다.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		// VelocityComponent를 사용하여 Fireball을 마우스 방향으로 이동
		// MoveAndCollide: 충돌을 감지하면서 이동
		VelocityComponent.MoveAndCollide(this, delta);
	}

	/// <summary>
	/// HotZone에 좀비가 진입했을 때 호출되는 신호 핸들러
	/// 좀비에게 FireComponent를 추가하고 추적 딕셔너리에 등록한다.
	/// </summary>
	/// <param name="body">핫존에 진입한 물체(좀비)</param>
	private void _EnteredHotZone(Node2D body)
	{
		// 좀비에서 FireComponent를 찾거나, 없으면 새로 추가
		// ?? (null-coalescing) 연산자 사용: 왼쪽이 null이면 오른쪽 실행
		var fireComponent = body.GetFirstNodeOfType<FireComponent>() ?? body.AddResourceDeferred<FireComponent>();

		// 추적 딕셔너리에 좀비의 이름과 FireComponent 등록
		_nodesInsideHotZone.Add(body.Name, fireComponent);
	}

	/// <summary>
	/// HotZone에서 좀비가 퇴출했을 때 호출되는 신호 핸들러
	/// 추적 딕셔너리에서 좀비를 제거한다.
	/// </summary>
	/// <param name="body">핫존에서 퇴출한 물체(좀비)</param>
	private void _ExitedHotZone(Node2D body)
	{
		// 추적 딕셔너리에서 좀비 제거
		_nodesInsideHotZone.Remove(body.Name);
	}

	/// <summary>
	/// 타이머의 Timeout 신호에 의해 호출되는 메서드
	/// 핫존 내의 모든 좀비에게 열 에너지를 추가하여 불 데미지를 입힌다.
	/// </summary>
	private void _HeatUpHotZone()
	{
		// HeatUpTouching이 false이면 아무것도 하지 않음
		if (!HeatUpTouching) return;

		// 핫존 내의 모든 좀비의 FireComponent에 열 에너지 추가
		foreach (var fireComponent in _nodesInsideHotZone.Values)
		{
			fireComponent.AddEnergy(HeatUpEnergyRate);
		}
	}

	/// <summary>
	/// Fireball이 좀비에게 피격되었을 때 호출되는 신호 핸들러
	/// 시각적 피격 효과(애니메이션 및 파티클)를 표시한다.
	/// </summary>
	/// <param name="player">피격된 플레이어(Fireball 자신)</param>
	/// <param name="angle">공격 방향의 각도</param>
	/// <param name="direction">공격 방향 단위 벡터</param>
	private void _hit(Node2D player, float angle, Vector2 direction)
	{
		// Tween을 생성하여 애니메이션 시퀀스 정의
		var tween = CreateTween();

		// 파티클 이펙트 설정
		if (!HitParticles.Emitting)
		{
			HitParticles.Emitting = true;
			// 파티클의 중력 방향을 공격 방향의 반대로 설정 (밀려나는 효과)
			HitParticles.Gravity = Vector2.One * direction * -1000.0f;
		}

		// Skull의 색상 애니메이션: Magenta(보라색) → White(흰색)
		// 0.25초 동안 선형으로 변환
		tween.TweenProperty(
				Skull,
				CanvasItem.PropertyName.Modulate.ToString(),
				Colors.White,
				0.25f)
			.From(this.IntensifyColor(Colors.Magenta, 2.3f))  // 시작 색상: 강화된 Magenta
			.SetTrans(Tween.TransitionType.Linear)             // 선형 트랜지션
			.SetEase(Tween.EaseType.In);                       // 이징: In

		// 다음 애니메이션들을 병렬로 실행
		tween.SetParallel();

		// Skull의 크기 애니메이션: 1.3배 → 원래 크기
		tween.TweenProperty(
				Skull,
				Node2D.PropertyName.Scale.ToString(),
				_skullScale,
				0.25f)
			.From(_skullScale * 1.3f)
			.SetTrans(Tween.TransitionType.Linear)
			.SetEase(Tween.EaseType.In);

		// HitParticles의 방향 업데이트
		tween.TweenProperty(
			HitParticles,
			CpuParticles2D.PropertyName.Direction.ToString(),
			direction,
			0.25f);

		// 병렬 모드 종료 (이후 애니메이션은 순차 실행)
		tween.SetParallel(false);

		// 애니메이션 완료 후 파티클 이펙트 중지
		tween.TweenCallback(Callable.From(() => HitParticles.Emitting = false));
	}
}
