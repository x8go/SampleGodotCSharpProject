namespace Game.Component;

using Game.Autoload;
using Game.Component;
using Game.Extension;
using Vector2 = Godot.Vector2;

/// <summary>
/// 좀비의 시체가 플레이어(Fireball)에게 끌려가는 스코어 어트랙터 컴포넌트
/// Zombie.cs의 _CheckHealth에서 호출되며 좀비가 죽을 때 생성된다.
/// 죽은 좀비의 시체가 Fireball에게 끌려가다가 충돌하여 점수를 준다.
/// </summary>
public partial class ScoreAttractorComponent : BaseComponent
{
	/// <summary>
	/// 시체를 끌어당길 Fireball(플레이어) 노드
	/// Zombie.cs에서 GetTree().GetFirstNodeInGroup<Fireball>()로 설정됨
	/// </summary>
	[Export]
	public Node2D AttractorNode;

	/// <summary>
	/// Fireball으로 끌려가는 애니메이션 지속시간
	/// Inspector에서 조정 가능 (기본값: 2.0초)
	/// </summary>
	[Export]
	public float Duration = 2.0f;

	/// <summary>
	/// 0 ~ 1 범위의 보간 값
	/// Tween이 Duration 시간 동안 0 → 1로 애니메이션
	/// _PhysicsProcess에서 이 값을 사용하여 이동 거리 계산
	/// </summary>
	private float Amount { get; set; }

	/// <summary>시체 노드 (부모 노드이며, Zombie)를 캐시</summary>
	private Node2D _node;

	/// <summary>시체의 초기 위치 - 애니메이션 계산의 시작점</summary>
	private Vector2 _nodeInitialGlobalPos;

	/// <summary>
	/// 노드가 준비되었을 때(씬 로드 완료) 호출되는 라이프사이클 메서드
	/// 애니메이션 시퀀스와 이벤트를 설정한다.
	/// </summary>
	public override void _Ready()
	{
		// 부모 노드(좀비 시체)를 Node2D로 캐시
		_node = GetParent<Node2D>();

		// 시체의 초기 위치 저장
		_nodeInitialGlobalPos = _node.GlobalPosition;

		// 시체의 모든 컴포넌트 비활성화
		// 이동, 회전, 체력 등의 컴포넌트를 비활성화하여 Fireball으로 끌려가기만 함
		_node.EnableComponent<FacingComponent>(false);
		_node.EnableComponent<FollowPlayerComponent>(false);
		_node.EnableComponent<VelocityComponent>(false);

		// Tween 시퀀스 생성
		var tween = CreateTween();

		// Amount를 0 → 1로 애니메이션
		// Duration 시간 동안 Back 트랜지션으로 in 이징
		tween.TweenProperty(
				this,
				nameof(Amount),
				1.0f,
				Duration)
			.SetTrans(Tween.TransitionType.Back)  // Back 트랜지션: 뒤로 물러났다가 나아오는 효과
			.SetEase(Tween.EaseType.In);          // In 이징: 처음엔 느리고 나중에 빠름

		// 동시에 시체를 회전시키는 애니메이션
		tween.Parallel().TweenProperty(
				_node,
				Node2D.PropertyName.Rotation.ToString(),
				Mathf.Pi * 7.0,  // 약 3.5바퀴 회전
				Duration)
			.SetTrans(Tween.TransitionType.Expo)  // 지수 트랜지션
			.SetEase(Tween.EaseType.In);          // In 이징

		// 애니메이션 완료 후 PlayerHit 이벤트 발행
		// 이벤트로 Score UI를 업데이트
		tween.TweenCallback(
			Callable.From(() =>
			{
				// Fireball에서 시체로의 방향 계산
				var hitDirection = AttractorNode.GlobalPosition.DirectionTo(_node.GlobalPosition);

				// Fireball에서 시체로의 각도 계산
				var hitAngle = AttractorNode.GlobalPosition.AngleToPoint(_node.GlobalPosition);

				// PlayerHit 이벤트 발행 (점수 증가 트리거)
				GameEvents.EmitPlayerHit(AttractorNode, hitAngle, hitDirection);
			}));

		// 애니메이션 완료 후 이 노드 제거
		tween.TweenCallback(Callable.From(QueueFree));
	}

	/// <summary>
	/// 매 물리 프레임마다 호출되는 라이프사이클 메서드
	/// 시체를 Fireball을 향해 이동시킨다.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		// Fireball의 현재 위치
		var fireballPos = AttractorNode.GlobalPosition;

		// 시체가 이동해야 할 거리
		// 초기 위치에서 Fireball까지의 총 거리 × Amount (0 ~ 1)
		var distance = _nodeInitialGlobalPos.DistanceTo(fireballPos) * Amount;

		// 시체를 초기 위치에서 Fireball 방향으로 distance만큼 이동
		_node.GlobalPosition = _nodeInitialGlobalPos.MoveToward(fireballPos, distance);
	}
}
