using GodotUtilities.Util;

namespace GodotUtilities;

/// <summary>
/// PhysicsDirectSpaceState2D 클래스의 확장 메서드를 제공하는 정적 클래스
/// 2D 물리 공간에서의 레이캐스트 기능을 보다 편리하게 사용할 수 있습니다
/// </summary>
public static class Physics2DDirectSpaceStateExtensions
{
	/// <summary>
	/// PhysicsDirectSpaceState2D의 확장 메서드: 레이캐스트를 수행하고 결과를 RaycastResult 객체로 반환합니다
	/// 충돌이 있으면 RaycastResult를 반환하고, 없으면 null을 반환합니다
	/// </summary>
	/// <param name="state">물리 공간 상태</param>
	/// <param name="query">레이캐스트 쿼리 파라미터</param>
	/// <returns>충돌이 있으면 RaycastResult, 없으면 null</returns>
	public static RaycastResult Raycast(this PhysicsDirectSpaceState2D state, PhysicsRayQueryParameters2D query)
	{
		// 레이캐스트 실행 및 결과 딕셔너리 획득
		var raycastDict = state.IntersectRay(query);
		if (raycastDict?.Count > 0)
		{
			// 충돌이 있으면 RaycastResult 객체로 변환하여 반환
			return new RaycastResult(query.From, query.To, raycastDict);
		}
		// 충돌이 없으면 null 반환
		return null;
	}
}
