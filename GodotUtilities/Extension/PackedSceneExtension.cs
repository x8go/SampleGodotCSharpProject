namespace GodotUtilities;

/// <summary>
/// PackedScene 클래스의 확장 메서드를 제공하는 정적 클래스
/// 씬 인스턴스화를 보다 안전하고 편리하게 수행할 수 있습니다
/// </summary>
public static class PackedSceneExtension
{
	/// <summary>
	/// PackedScene의 확장 메서드: 씬을 인스턴스화하여 지정된 타입으로 반환하거나, 실패 시 자동으로 메모리 해제합니다
	/// </summary>
	/// <typeparam name="T">인스턴스화할 타입</typeparam>
	/// <param name="scene">인스턴스화할 PackedScene</param>
	/// <returns>성공 시 타입 T의 인스턴스, 실패 시 null</returns>
	public static T InstanceOrFree<T>(this PackedScene scene) where T : class
	{
		// 씬 인스턴스 생성
		var node = scene.Instantiate();
		if (node is T t)
		{
			// 타입이 일치하면 반환
			return t;
		}
		// 타입이 일치하지 않으면 메모리 해제하고 경고 출력
		node.QueueFree();
		GD.PushWarning($"Could not instance PackedScene {scene} as {typeof(T).Name}");
		return null;
	}

	/// <summary>
	/// PackedScene의 확장 메서드: 씬을 임시로 인스턴스화하여 데이터를 추출하고 즉시 해제합니다
	/// 씬에서 데이터만 가져오고 싶을 때 유용합니다
	/// </summary>
	/// <typeparam name="T">인스턴스화할 노드 타입</typeparam>
	/// <param name="scene">데이터를 추출할 PackedScene</param>
	/// <param name="action">인스턴스를 받아 실행할 액션</param>
	public static void ExtractData<T>(this PackedScene scene, Action<T> action) where T : Node
	{
		// 씬 인스턴스 생성
		var node = scene.InstanceOrFree<T>();
		// 액션 실행 (데이터 추출)
		action(node);
		// 인스턴스 해제
		node.QueueFree();
	}
}
