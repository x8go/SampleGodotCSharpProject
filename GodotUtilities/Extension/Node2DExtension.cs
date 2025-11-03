namespace GodotUtilities;

/// <summary>
/// Node2D 클래스의 확장 메서드를 제공하는 정적 클래스
/// 2D 노드의 마우스 관련 기능을 편리하게 사용할 수 있습니다
/// </summary>
public static class Node2DExtension
{
	/// <summary>
	/// Node2D의 확장 메서드: 노드에서 마우스 위치로 향하는 정규화된 방향 벡터를 반환합니다
	/// </summary>
	/// <param name="node">기준 노드</param>
	/// <returns>노드에서 마우스로 향하는 정규화된 방향 벡터</returns>
	public static Vector2 GetMouseDirection(this Node2D node)
	{
		// 전역 마우스 위치 - 노드의 전역 위치를 계산하고 정규화
		return (node.GetGlobalMousePosition() - node.GlobalPosition).Normalized();
	}
}
