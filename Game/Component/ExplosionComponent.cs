namespace Game.Component;

using Game.Autoload;
using Game.Entity.Enemy;
using Game.Manager;
using Game.Extension;

/// <summary>
/// 좀비가 죽을 때 폭발 애니메이션과 화면 흔들림 효과를 표시하는 컴포넌트
/// Zombie.cs의 _CheckHealth에서 호출되며 좀비가 죽을 때 추가된다.
/// ExplosionComponent 리소스가 필요하며, 애니메이션 완료 후 자동으로 제거된다.
/// </summary>
public partial class ExplosionComponent : BaseComponent
{
	/// <summary>폭발 애니메이션을 표시하는 AnimatedSprite2D 노드</summary>
	[Node]
	public AnimatedSprite2D AnimatedSprite2D;

	/// <summary>폭발 비주얼을 포함하는 Node2D (회전 처리됨)</summary>
	[Node]
	public Node2D Visuals;

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
	/// 노드가 준비되었을 때(씬 로드 완료) 호출되는 라이프사이클 메서드
	/// 폭발 애니메이션을 시작하고 카메라 흔들림 효과를 적용한다.
	/// </summary>
	public override void _Ready()
	{
		// 애니메이션 완료 시 이 노드를 큐에서 제거하도록 설정
		AnimatedSprite2D.AnimationFinished += QueueFree;

		// 폭발 비주얼을 무작위로 회전 (0도 ~ 2π라디안 범위)
		// Randfn: 정규 분포 난수 생성 (평균: Mathf.Tau, 표준편차: Mathf.Pi)
		Visuals.Rotation = (float)GD.Randfn(Mathf.Tau, Mathf.Pi);

		// 부모 노드가 Zombie인 경우 처리
		if (GetParent() is Zombie node)
		{
			// 좀비를 화면 위에 표시 (ZIndex = 2로 다른 오브젝트보다 앞)
			node.ZIndex = 2;

			// 좀비의 색상을 강화된 다크마젠타로 변경 (죽음 표현)
			node.Modulate = this.IntensifyColor(Colors.DarkMagenta, 2.3f);
		}

		// 카메라로부터의 거리에 따른 화면 흔들림 강도 계산
		var shakeIntensity = _CalcShakeIntensity();

		// 흔들림 강도가 거의 0이면 흔들림 효과 생략
		if (shakeIntensity <= float.Epsilon) return;

		// 화면 흔들림 효과 적용
		// FxManager.ShakeScreen(지속시간, 강도)
		FxManager.ShakeScreen(1f, 5 * shakeIntensity);
	}

	/// <summary>
	/// 폭발 위치와 카메라 거리에 따른 화면 흔들림 강도를 계산하는 메서드
	/// 거리가 가까울수록 강도가 높고, 500픽셀 이상이면 강도가 0이 된다.
	/// </summary>
	/// <returns>0.0 ~ 1.0 범위의 흔들림 강도</returns>
	private float _CalcShakeIntensity()
	{
		// 폭발 위치와 카메라 위치 사이의 거리를 계산
		// Mathf.Remap: (입력값, 입력 최소, 입력 최대, 출력 최소, 출력 최대)
		// 거리 0 → 강도 1.0, 거리 500 → 강도 0.0
		var shakeIntensity = Mathf.Remap(
			GlobalPosition.DistanceTo(Global.Instance.Camera2D.GlobalPosition),
			0,      // 입력 최소 (거리 0)
			500f,   // 입력 최대 (거리 500)
			1.0f,   // 출력 최대 (강도 1.0)
			0.0f);  // 출력 최소 (강도 0.0)

		return shakeIntensity;
	}
}
