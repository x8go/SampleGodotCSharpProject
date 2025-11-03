namespace Game.Component;

/// <summary>
/// 게임 오브젝트가 이동 방향을 바라보도록 회전시키는 컴포넌트
/// Zombie.cs에서 사용되며, 좀비의 비주얼이 이동 방향을 향하도록 회전시킨다.
/// </summary>
public partial class FacingComponent : BaseComponent
{
	/// <summary>
	/// 이동 방향에 따라 회전시킬 Node2D
	/// Zombie.tscn의 Visuals 노드로 설정됨
	/// Inspector에서 할당 가능
	/// </summary>
	[Export]
	public Node2D Node2DToRotate;

	/// <summary>
	/// 이동 속도 정보를 얻기 위한 VelocityComponent 참조
	/// 이동 방향(Velocity)을 이용하여 회전각을 계산
	/// Inspector에서 할당 가능
	/// </summary>
	[Export]
	public VelocityComponent VelocityComponent;

	/// <summary>
	/// 매 물리 프레임마다 호출되는 라이프사이클 메서드
	/// 이동 방향에 따라 노드를 회전시킨다.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		// 컴포넌트가 비활성화되면 아무것도 하지 않음
		if (!Enabled) return;

		// VelocityComponent의 속도 벡터를 정규화하여 방향만 추출
		// Angle() 메서드로 그 방향의 각도(라디안)를 계산
		// Node2D의 Rotation 속성에 할당하여 회전
		Node2DToRotate.Rotation = VelocityComponent.Velocity.Normalized().Angle();
	}
}
