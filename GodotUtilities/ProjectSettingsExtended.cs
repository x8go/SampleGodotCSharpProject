namespace GodotUtilities;

/// <summary>
/// ProjectSettings 클래스의 확장 기능을 제공하는 정적 클래스
/// Godot 프로젝트 설정 값을 안전하게 가져오기 위한 유틸리티 메서드를 포함
/// </summary>
public static class ProjectSettingsExtended
{
	/// <summary>
	/// 지정된 이름의 프로젝트 설정 값을 가져오거나, 존재하지 않으면 기본값을 반환합니다
	/// </summary>
	/// <typeparam name="T">설정 값의 타입</typeparam>
	/// <param name="name">설정 이름</param>
	/// <returns>설정 값 또는 기본값</returns>
	// TODO: check if this casting of variant works
	public static T GetSettingOrDefault<T>(string name)
	{
		// 설정이 존재하면 해당 값을 반환, 없으면 기본값 반환
		return ProjectSettings.HasSetting(name) ? (T)(object)ProjectSettings.GetSetting(name) : default;
	}

	/// <summary>
	/// 디버그 빌드일 때만 프로젝트 설정 값을 가져오고, 릴리즈 빌드에서는 기본값을 반환합니다
	/// </summary>
	/// <typeparam name="T">설정 값의 타입</typeparam>
	/// <param name="name">설정 이름</param>
	/// <returns>디버그 빌드에서는 설정 값, 릴리즈 빌드에서는 기본값</returns>
	public static T GetDebugSettingOrDefault<T>(string name)
	{
		// 디버그 빌드인 경우에만 설정 값을 가져옴
		return OS.IsDebugBuild() ? GetSettingOrDefault<T>(name) : default;
	}
}
