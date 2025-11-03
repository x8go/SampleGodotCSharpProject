using System.Collections.Generic;
using System.Linq;

namespace GodotUtilities;

/// <summary>
/// Node 클래스의 확장 메서드를 제공하는 정적 클래스
/// 노드 검색, 자식 노드 관리, 그룹 관리 등 다양한 유틸리티 메서드를 포함합니다
/// </summary>
public static class NodeExtension
{
	/// <summary>
	/// Node의 확장 메서드: 노드를 타입 이름과 동일한 이름의 그룹에 추가합니다
	/// </summary>
	/// <param name="node">그룹에 추가할 노드</param>
	public static void AddToGroup(this Node node)
	{
		// 노드의 타입 이름을 그룹 이름으로 사용
		node.AddToGroup(node.GetType().Name);
	}

	/// <summary>
	/// Node의 확장 메서드: 지정된 인덱스의 형제 노드를 타입 T로 반환합니다
	/// </summary>
	/// <typeparam name="T">반환할 노드 타입</typeparam>
	/// <param name="node">기준 노드</param>
	/// <param name="idx">형제 노드의 인덱스</param>
	/// <returns>형제 노드</returns>
	public static T GetSibling<T>(this Node node, int idx) where T : Node
	{
		// 부모의 자식 노드 중 지정된 인덱스의 노드 반환
		return (T)node.GetParent().GetChild(idx);
	}

	/// <summary>
	/// Node의 확장 메서드: 타입 이름을 경로로 사용하여 노드를 검색합니다
	/// </summary>
	/// <typeparam name="T">검색할 노드 타입</typeparam>
	/// <param name="node">기준 노드</param>
	/// <returns>찾은 노드</returns>
	public static T GetNode<T>(this Node node) where T : Node
	{
		// 타입 이름을 노드 경로로 사용
		return node.GetNode<T>(typeof(T).Name);
	}

	/// <summary>
	/// Node의 확장 메서드: 모든 자식 노드를 타입 T의 리스트로 반환합니다
	/// </summary>
	/// <typeparam name="T">반환할 타입</typeparam>
	/// <param name="node">부모 노드</param>
	/// <returns>타입 T의 자식 노드 리스트</returns>
	public static List<T> GetChildren<T>(this Node node) where T : class
	{
		var children = node.GetChildren().Cast<Node>();
		// 각 자식을 타입 T로 캐스팅
		return children.Select(x => x as T).ToList();
	}

	/// <summary>
	/// Node의 확장 메서드: 타입 T의 첫 번째 자식 노드를 반환합니다
	/// </summary>
	/// <typeparam name="T">검색할 타입</typeparam>
	/// <param name="node">부모 노드</param>
	/// <returns>타입 T의 첫 번째 자식 노드 또는 기본값</returns>
	public static T GetFirstNodeOfType<T>(this Node node)
	{
		var children = node.GetChildren();
		foreach (var child in children)
		{
			// 타입이 일치하는 첫 번째 자식 반환
			if (child is T t)
			{
				return t;
			}
		}
		return default;
	}

	/// <summary>
	/// Node의 확장 메서드: 타입 T의 모든 자식 노드를 리스트로 반환합니다
	/// </summary>
	/// <typeparam name="T">검색할 타입</typeparam>
	/// <param name="node">부모 노드</param>
	/// <returns>타입 T의 모든 자식 노드 리스트</returns>
	public static List<T> GetNodesOfType<T>(this Node node)
	{
		var result = new List<T>();
		var children = node.GetChildren();
		foreach (var child in children)
		{
			// 타입이 일치하는 모든 자식을 리스트에 추가
			if (child is T t)
			{
				result.Add(t);
			}
		}
		return result;
	}

	/// <summary>
	/// Node의 확장 메서드: 자식 노드를 지연 방식으로 추가합니다
	/// 현재 프레임의 처리가 끝난 후 다음 프레임에 추가됩니다
	/// </summary>
	/// <param name="node">부모 노드</param>
	/// <param name="child">추가할 자식 노드</param>
	public static void AddChildDeferred(this Node node, Node child)
	{
		// 다음 프레임에 자식 추가
		node.CallDeferred("add_child", child);
	}

	/// <summary>
	/// Node의 확장 메서드: NodePath가 null이 아닐 때만 노드를 검색합니다
	/// </summary>
	/// <typeparam name="T">검색할 노드 타입</typeparam>
	/// <param name="n">기준 노드</param>
	/// <param name="nodePath">노드 경로 (null 가능)</param>
	/// <returns>찾은 노드 또는 null</returns>
	public static T GetNullableNodePath<T>(this Node n, NodePath nodePath) where T : Node
	{
		// nodePath가 null이면 null 반환
		if (nodePath == null) return null;
		return n.GetNodeOrNull<T>(nodePath);
	}

	/// <summary>
	/// Node의 확장 메서드: 모든 자식 노드를 씬 트리에서 제거하고 메모리에서 해제합니다
	/// </summary>
	/// <param name="n">부모 노드</param>
	public static void RemoveAndQueueFreeChildren(this Node n)
	{
		foreach (var child in n.GetChildren())
		{
			if (child is Node childNode)
			{
				// 씬 트리에서 제거
				n.RemoveChild(childNode);
				// 메모리에서 해제 예약
				childNode.QueueFree();
			}
		}
	}

	/// <summary>
	/// Node의 확장 메서드: 모든 자식 노드를 메모리에서 해제 예약합니다
	/// 씬 트리에서는 제거하지 않습니다
	/// </summary>
	/// <param name="n">부모 노드</param>
	public static void QueueFreeChildren(this Node n)
	{
		foreach (var child in n.GetChildren())
		{
			if (child is Node childNode)
			{
				// 메모리에서 해제 예약
				childNode.QueueFree();
			}
		}
	}

	/// <summary>
	/// Node의 확장 메서드: 타입 T의 조상 노드를 찾아 반환합니다
	/// 부모를 따라 올라가며 검색하고, 루트에 도달하면 null을 반환합니다
	/// </summary>
	/// <typeparam name="T">검색할 조상 노드 타입</typeparam>
	/// <param name="n">기준 노드</param>
	/// <returns>타입 T의 조상 노드 또는 null</returns>
	public static T GetAncestor<T>(this Node n) where T : Node
	{
		Node currentNode = n;
		// 루트가 아니고 타입이 T가 아닐 때까지 부모로 이동
		while (currentNode != n.GetTree().Root && currentNode is not T)
		{
			currentNode = currentNode.GetParent();
		}

		// 타입 T의 조상을 찾았으면 반환, 아니면 null
		return currentNode is T ancestor ? ancestor : null;
	}

	/// <summary>
	/// Node의 확장 메서드: 마지막 자식 노드를 반환합니다
	/// </summary>
	/// <param name="n">부모 노드</param>
	/// <returns>마지막 자식 노드 또는 null</returns>
	public static Node GetLastChild(this Node n)
	{
		var count = n.GetChildCount();
		// 자식이 없으면 null 반환
		if (count == 0) return null;
		// 마지막 자식 반환
		return n.GetChild(count - 1);
	}

	/// <summary>
	/// Node의 확장 메서드: 노드를 지연 방식으로 메모리에서 해제 예약합니다
	/// 현재 프레임의 처리가 끝난 후 다음 프레임에 해제됩니다
	/// </summary>
	/// <param name="n">해제할 노드</param>
	public static void QueueFreeDeferred(this Node n)
	{
		// 다음 프레임에 메모리 해제
		n.CallDeferred("queue_free");
	}

	/// <summary>
	/// IEnumerable&lt;Node&gt;의 확장 메서드: 컬렉션의 모든 노드를 메모리에서 해제 예약합니다
	/// </summary>
	/// <param name="objects">해제할 노드들의 컬렉션</param>
	public static void QueueFreeAll(this IEnumerable<Node> objects)
	{
		foreach (var obj in objects)
		{
			// 각 노드를 메모리에서 해제 예약
			obj.QueueFree();
		}
	}
}
