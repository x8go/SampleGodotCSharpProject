using GodotUtilities.Util;

namespace GodotUtilities;

/// <summary>
/// ResourcePreloader 클래스의 확장 메서드를 제공하는 정적 클래스
/// 프리로드된 씬을 편리하게 인스턴스화할 수 있습니다
/// </summary>
public static class ResourcePreloaderExtension
{
	/// <summary>
	/// ResourcePreloader의 확장 메서드: 리소스 이름으로 씬을 찾아 인스턴스화합니다
	/// 리소스가 없거나 PackedScene이 아니면 null을 반환합니다
	/// </summary>
	/// <typeparam name="T">인스턴스화할 노드 타입</typeparam>
	/// <param name="preloader">리소스 프리로더</param>
	/// <param name="name">리소스 이름</param>
	/// <returns>성공 시 타입 T의 인스턴스, 실패 시 null</returns>
	public static T InstanceSceneOrNull<T>(this ResourcePreloader preloader, string name) where T : Node
	{
		// 리소스 존재 여부 확인
		if (!preloader.HasResource(name))
		{
			//Logger.Error("Preloader did not have a resource with name " + name);
			return null;
		}

		// 리소스가 PackedScene인지 확인
		if (!(preloader.GetResource(name) is PackedScene resource))
		{
			//Logger.Error("Resource with name " + name + " was not a " + nameof(PackedScene));
			return null;
		}

		// PackedScene 인스턴스화
		return resource.InstantiateOrNull<T>();
	}

	/// <summary>
	/// ResourcePreloader의 확장 메서드: 타입 이름을 리소스 이름으로 사용하여 씬을 인스턴스화합니다
	/// </summary>
	/// <typeparam name="T">인스턴스화할 노드 타입 (타입 이름이 리소스 이름으로 사용됨)</typeparam>
	/// <param name="preloader">리소스 프리로더</param>
	/// <returns>성공 시 타입 T의 인스턴스, 실패 시 null</returns>
	public static T InstanceSceneOrNull<T>(this ResourcePreloader preloader) where T : Node
	{
		// 타입 이름을 리소스 이름으로 사용
		return preloader.InstanceSceneOrNull<T>(typeof(T).Name);
	}
}
