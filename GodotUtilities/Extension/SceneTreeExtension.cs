using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GodotUtilities;

/// <summary>
/// SceneTree 클래스의 확장 메서드를 제공하는 정적 클래스
/// 그룹 내 노드 검색 및 비동기 처리를 편리하게 할 수 있습니다
/// </summary>
public static class SceneTreeExtension
{
	/// <summary>
	/// SceneTree의 확장 메서드: 지정된 그룹에서 첫 번째 노드를 타입 T로 반환합니다
	/// </summary>
	/// <typeparam name="T">검색할 노드 타입</typeparam>
	/// <param name="sceneTree">씬 트리</param>
	/// <param name="group">그룹 이름</param>
	/// <returns>첫 번째 노드 또는 null</returns>
	public static T GetFirstNodeInGroup<T>(this SceneTree sceneTree, string group) where T : Node
	{
		var nodes = sceneTree.GetNodesInGroup(group);
		// 그룹에 노드가 있으면 첫 번째 노드를 타입 T로 반환
		return nodes.Count > 0 ? nodes[0] as T : null;
	}

	/// <summary>
	/// SceneTree의 확장 메서드: 타입 이름을 그룹 이름으로 사용하여 첫 번째 노드를 반환합니다
	/// </summary>
	/// <typeparam name="T">검색할 노드 타입 (타입 이름이 그룹 이름으로 사용됨)</typeparam>
	/// <param name="sceneTree">씬 트리</param>
	/// <returns>첫 번째 노드 또는 null</returns>
	public static T GetFirstNodeInGroup<T>(this SceneTree sceneTree) where T : Node
	{
		// 타입 이름을 그룹 이름으로 사용
		var name = typeof(T).Name;
		return GetFirstNodeInGroup<T>(sceneTree, name);
	}

	/// <summary>
	/// SceneTree의 확장 메서드: 지정된 그룹의 모든 노드를 타입 T의 컬렉션으로 반환합니다
	/// </summary>
	/// <typeparam name="T">검색할 노드 타입</typeparam>
	/// <param name="sceneTree">씬 트리</param>
	/// <param name="group">그룹 이름</param>
	/// <returns>타입 T의 노드 컬렉션</returns>
	public static IEnumerable<T> GetNodesInGroup<T>(this SceneTree sceneTree, string group) where T : Node
	{
		// 그룹의 모든 노드를 타입 T로 캐스팅
		return sceneTree.GetNodesInGroup(group).Cast<T>();
	}

	/// <summary>
	/// SceneTree의 확장 메서드: 타입 이름을 그룹 이름으로 사용하여 모든 노드를 반환합니다
	/// </summary>
	/// <typeparam name="T">검색할 노드 타입 (타입 이름이 그룹 이름으로 사용됨)</typeparam>
	/// <param name="sceneTree">씬 트리</param>
	/// <returns>타입 T의 노드 컬렉션</returns>
	public static IEnumerable<T> GetNodesInGroup<T>(this SceneTree sceneTree) where T : Node
	{
		// 타입 이름을 그룹 이름으로 사용
		var name = typeof(T).Name;
		return GetNodesInGroup<T>(sceneTree, name);
	}

	/// <summary>
	/// SceneTree의 확장 메서드: 다음 프레임(ProcessFrame)까지 비동기 대기합니다
	/// </summary>
	/// <param name="sceneTree">씬 트리</param>
	/// <returns>비동기 작업</returns>
	public static async Task NextIdle(this SceneTree sceneTree)
	{
		// ProcessFrame 시그널이 발생할 때까지 대기
		await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
	}
}
