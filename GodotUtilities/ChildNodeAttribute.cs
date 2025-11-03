using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GodotUtilities;

/// <summary>
/// 자식 노드를 자동으로 필드나 프로퍼티에 할당하기 위한 어트리뷰트
/// Node 클래스의 WireNodes() 확장 메서드와 함께 사용됩니다
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class NodeAttribute : Attribute
{
	/// <summary>
	/// 노드의 경로 (null인 경우 필드/프로퍼티 이름으로 자동 매칭)
	/// </summary>
	public string NodePath { get; }

	/// <summary>
	/// NodeAttribute 생성자
	/// </summary>
	/// <param name="nodePath">노드 경로 (null이면 자동 매칭)</param>
	public NodeAttribute(string nodePath = null)
	{
		NodePath = nodePath;
	}
}

/// <summary>
/// Node 클래스의 확장 메서드를 제공하는 정적 클래스
/// NodeAttribute와 ParentAttribute를 사용하여 자동으로 노드를 연결합니다
/// </summary>
public static class ChildNodeAttributeExtension
{
	/// <summary>
	/// 리플렉션에 사용할 바인딩 플래그 (public, private, instance 멤버 모두 포함)
	/// </summary>
	private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	/// <summary>
	/// Node 클래스의 확장 메서드: NodeAttribute와 ParentAttribute가 지정된 필드/프로퍼티에 자동으로 노드를 할당합니다
	/// </summary>
	/// <param name="n">노드 연결을 수행할 대상 노드</param>
	public static void WireNodes(this Node n)
	{
		// 자식 노드들을 소문자 이름으로 딕셔너리에 저장 (대소문자 무시 검색용)
		var lowerCaseChildNameToChild = n.GetChildren().Cast<Node>().ToDictionary(x => x.Name.ToString().ToLowerInvariant(), x => x);

		// 모든 필드에 대해 노드 연결 수행
		var fields = n.GetType().GetFields(BINDING_FLAGS);
		foreach (var memberInfo in fields)
		{
			SetChildNode(n, memberInfo, lowerCaseChildNameToChild);
			SetParentNode(n, memberInfo);
		}

		// 모든 프로퍼티에 대해 노드 연결 수행
		var properties = n.GetType().GetProperties(BINDING_FLAGS);
		foreach (var memberInfo in properties)
		{
			SetChildNode(n, memberInfo, lowerCaseChildNameToChild);
			SetParentNode(n, memberInfo);
		}
	}

	/// <summary>
	/// NodeAttribute가 지정된 멤버에 자식 노드를 할당합니다
	/// </summary>
	/// <param name="node">부모 노드</param>
	/// <param name="memberInfo">노드를 할당할 멤버 정보</param>
	/// <param name="lowerCaseChildNameToChild">자식 노드 딕셔너리 (소문자 이름 -> 노드)</param>
	private static void SetChildNode(Node node, MemberInfo memberInfo, Dictionary<string, Node> lowerCaseChildNameToChild)
	{
		// NodeAttribute가 있는지 확인
		var attribute = Attribute.GetCustomAttribute(memberInfo, typeof(NodeAttribute));
		if (attribute is not NodeAttribute childNodeAttribute)
		{
			return;
		}

		Node childNode;
		if (childNodeAttribute.NodePath != null)
		{
			// 명시적 경로가 지정된 경우 해당 경로로 노드 검색
			childNode = node.GetNodeOrNull(childNodeAttribute.NodePath);
		}
		else
		{
			// 경로가 없으면 멤버 이름으로 자동 매칭 시도
			var memberNameLower = memberInfo.Name.ToLower();
			var lookupSuccess = lowerCaseChildNameToChild.TryGetValue(memberNameLower, out childNode);
			if (!lookupSuccess)
			{
				// 고유 노드(%) 검색 시도
				childNode = TryGetUniqueNode(node, memberInfo);

				if (childNode == null)
				{
					// 부분 매칭으로 최선의 후보 찾기
					childNode = lowerCaseChildNameToChild
						.Where(x => memberNameLower.Contains(x.Key))
						.OrderByDescending(x => x.Key.Length)
						.FirstOrDefault().Value;

					if (childNode != null)
					{
						GD.PushWarning($"Assigned member {memberInfo.Name} to node {childNode.Name} in {node?.SceneFilePath ?? "the scene"} as a best-guess.");
					}
				}
			}
		}

		// 노드를 찾지 못한 경우 에러 출력
		if (childNode == null)
		{
			var filename = !string.IsNullOrEmpty(node.SceneFilePath) ? node.SceneFilePath : "the scene.";
			GD.PrintErr($"Could not match member {memberInfo.Name} to any Node in {filename}.");
		}

		// 멤버에 노드 할당 시도
		try
		{
			SetMemberValue(node, memberInfo, childNode);
		}
		catch
		{
			// 타입이 맞지 않는 경우 에러 출력
			var filename = !string.IsNullOrEmpty(node.SceneFilePath) ? node.SceneFilePath : "the scene.";
			Type memberType = memberInfo.GetUnderlyingType();
			if (!memberType.IsAssignableFrom(childNode.GetType()))
			{
				GD.PrintErr($"Could not match member {memberInfo.Name} to any Node of type {memberType} in {filename}. Found {childNode.GetType()}.");
			}
		}
	}

	/// <summary>
	/// ParentAttribute가 지정된 멤버에 부모 노드를 할당합니다
	/// </summary>
	/// <param name="node">자식 노드</param>
	/// <param name="memberInfo">부모 노드를 할당할 멤버 정보</param>
	private static void SetParentNode(Node node, MemberInfo memberInfo)
	{
		// ParentAttribute가 있는지 확인
		var attribute = Attribute.GetCustomAttribute(memberInfo, typeof(ParentAttribute));
		if (attribute is not ParentAttribute)
		{
			return;
		}

		// 부모 노드 가져오기 및 할당
		Node parentNode = node.GetParent();
		SetMemberValue(node, memberInfo, parentNode);
	}

	/// <summary>
	/// 멤버(필드 또는 프로퍼티)에 노드 값을 설정합니다
	/// </summary>
	/// <param name="node">멤버를 소유한 노드</param>
	/// <param name="memberInfo">값을 설정할 멤버 정보</param>
	/// <param name="childNode">설정할 노드 값</param>
	private static void SetMemberValue(Node node, MemberInfo memberInfo, Node childNode)
	{
		if (memberInfo is PropertyInfo propertyInfo)
		{
			// 프로퍼티인 경우
			propertyInfo.SetValue(node, childNode);
		}
		else if (memberInfo is FieldInfo fieldInfo)
		{
			// 필드인 경우
			fieldInfo.SetValue(node, childNode);
		}
	}

	/// <summary>
	/// 고유 노드(% 접두사를 사용하는 노드)를 찾습니다
	/// </summary>
	/// <param name="node">부모 노드</param>
	/// <param name="memberInfo">멤버 정보</param>
	/// <returns>찾은 노드 또는 null</returns>
	private static Node TryGetUniqueNode(Node node, MemberInfo memberInfo)
	{
		var name = memberInfo.Name;
		// 멤버 이름 그대로 시도
		var child = node.GetNodeOrNull($"%{name}");
		if (child == null && name.Length > 1)
		{
			// 첫 글자를 대문자로 변경하여 시도 (PascalCase)
			name = string.Concat(name[0].ToString().ToUpper(), name.AsSpan(1));
			child = node.GetNodeOrNull($"%{name}");
		}
		return child;
	}

	/// <summary>
	/// MemberInfo의 확장 메서드: 멤버의 실제 타입을 반환합니다
	/// </summary>
	/// <param name="member">멤버 정보</param>
	/// <returns>멤버의 타입</returns>
	public static Type GetUnderlyingType(this MemberInfo member)
	{
		return member.MemberType switch
		{
			MemberTypes.Event => ((EventInfo)member).EventHandlerType,
			MemberTypes.Field => ((FieldInfo)member).FieldType,
			MemberTypes.Method => ((MethodInfo)member).ReturnType,
			MemberTypes.Property => ((PropertyInfo)member).PropertyType,
			_ => throw new ArgumentException("Input MemberInfo must be of type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"),
		};
	}
}
