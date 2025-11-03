namespace Game.Component;

using Game.Component.Follow;

/// <summary>
/// 플레이어 추적 컴포넌트 - 엔티티가 플레이어를 따라가도록 합니다.
/// Main.tscn의 적(좀비 등)에 추가하여 플레이어를 추적하는 AI 동작을 구현합니다.
/// 플레이어까지의 거리에 비례하여 속도가 조절되므로 자연스러운 추격 동작을 제공합니다.
/// </summary>
public partial class FollowPlayerComponent : BaseComponent, IFollowComponent
{
	/// <summary>
	/// 이 컴포넌트가 부착된 부모 PhysicsBody2D (좀비, 적 등)
	/// </summary>
	private PhysicsBody2D _parent;

	/// <summary>
	/// 추적할 플레이어 노드 참조 ("Player" 그룹에서 자동 검색)
	/// </summary>
	private Node2D _player;

	/// <summary>
	/// 플레이어를 향한 기본 이동 속도
	/// Inspector에서 설정 가능 - 거리에 따라 이 값이 곱해져 최종 속도가 결정됩니다.
	/// 기본값: 0.5 (마우스 추적보다 느린 속도)
	/// </summary>
	[Export]
	public float Speed = 0.5f;

	/// <summary>
	/// 계산된 속도를 적용할 VelocityComponent 참조
	/// Inspector에서 설정 가능 - 실제 이동을 처리하는 컴포넌트와 연결됩니다.
	/// </summary>
	[Export]
	public VelocityComponent VelocityComponent;

	/// <summary>
	/// 노드가 씬 트리에 진입할 때 호출됩니다.
	/// [Node] 속성이 붙은 필드를 자동 연결하고, 부모 노드와 플레이어 참조를 저장합니다.
	/// </summary>
	public override void _EnterTree()
	{
		this.WireNodes();
		_parent = GetParent<PhysicsBody2D>();
		// "Player" 그룹에서 플레이어 노드 찾기
		_player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
	}

	/// <summary>
	/// 물리 프레임마다 호출됩니다.
	/// 플레이어 방향과 거리를 계산하여 속도를 업데이트합니다.
	/// </summary>
	/// <param name="delta">이전 프레임으로부터의 경과 시간(초)</param>
	public override void _PhysicsProcess(double delta)
	{
		if (!Enabled) return;

		// 현재 위치에서 플레이어를 향하는 정규화된 방향 벡터
		var playerDir = GlobalPosition.DirectionTo(_player.GlobalPosition);
		// 플레이어까지의 거리 계산
		var distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
		// 거리에 비례하는 속도 계산 - 멀수록 빠르게 이동
		var newVelocity = playerDir * Speed * distance;
		VelocityComponent.Velocity = newVelocity;
	}
}
