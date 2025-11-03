namespace Game.Component;

using Game.Component.Follow;

/// <summary>
/// 경로 추적 컴포넌트 - 엔티티가 미리 정의된 Path2D 경로를 따라 이동하도록 합니다.
/// Main.tscn의 엔티티에 추가하여 순찰, 레일 이동 등의 패턴화된 움직임을 구현합니다.
/// PathFollow2D 노드가 경로를 따라 이동하고, 엔티티는 그 위치를 추적합니다.
/// </summary>
public partial class FollowPathComponent : BaseComponent, IFollowComponent
{
	/// <summary>
	/// 경로상의 목표 지점을 향한 기본 추적 속도
	/// Inspector에서 설정 가능 - 거리에 따라 이 값이 곱해져 최종 속도가 결정됩니다.
	/// 기본값: 0.5
	/// </summary>
	[Export]
	public float FollowSpeed = 0.5f;

	/// <summary>
	/// 경로를 따라 PathFollow2D가 이동하는 속도 (초당 진행 비율)
	/// Inspector에서 설정 가능 - 0.1이면 10초에 경로 한 바퀴를 완주합니다.
	/// 기본값: 0.1 (10초에 한 바퀴)
	/// </summary>
	[Export]
	public float PathSpeed = 0.1f;

	/// <summary>
	/// 계산된 속도를 적용할 VelocityComponent 참조
	/// Inspector에서 설정 가능 - 실제 이동을 처리하는 컴포넌트와 연결됩니다.
	/// </summary>
	[Export]
	public VelocityComponent VelocityComponent;

	/// <summary>
	/// 경로상의 현재 목표 지점을 나타내는 PathFollow2D 노드
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// 이 노드가 경로를 따라 이동하면서 엔티티가 추적할 위치를 제공합니다.
	/// </summary>
	[Node]
	public PathFollow2D PathFollow2D;

	/// <summary>
	/// 이 컴포넌트가 부착된 부모 Node2D
	/// </summary>
	private Node2D _parent;

	/// <summary>
	/// 노드가 씬 트리에 진입할 때 호출됩니다.
	/// [Node] 속성이 붙은 필드를 자동 연결하고 부모 노드 참조를 저장합니다.
	/// </summary>
	public override void _EnterTree()
	{
		this.WireNodes();
		_parent = GetParent<Node2D>();
	}

	/// <summary>
	/// 물리 프레임마다 호출됩니다.
	/// PathFollow2D를 경로를 따라 이동시키고, 그 위치를 향해 엔티티를 이동시킵니다.
	/// </summary>
	/// <param name="delta">이전 프레임으로부터의 경과 시간(초)</param>
	public override void _PhysicsProcess(double delta)
	{
		if (!Enabled) return;

		// PathFollow2D를 경로를 따라 이동 (0.0~1.0 범위, 1.0이 되면 다시 0.0으로)
		PathFollow2D.ProgressRatio += PathSpeed * (float)delta;

		// 부모 위치에서 경로상의 목표 지점을 향하는 정규화된 방향 벡터
		var pathDir = _parent.GlobalPosition.DirectionTo(PathFollow2D.GlobalPosition);
		// 목표 지점까지의 거리 계산
		var distance = _parent.GlobalPosition.DistanceTo(PathFollow2D.GlobalPosition);
		// 거리에 비례하는 속도 계산 - 목표 지점이 멀수록 빠르게 이동
		var newVelocity = pathDir * FollowSpeed * distance;
		VelocityComponent.Velocity = newVelocity;
	}
}
