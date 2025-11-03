namespace GodotUtilities;

/// <summary>
/// Control 클래스의 확장 메서드를 제공하는 정적 클래스
/// UI 컨트롤의 피벗 설정을 편리하게 할 수 있습니다
/// </summary>
public static class ControlExtension
{
	/// <summary>
	/// Control의 확장 메서드: 피벗 오프셋을 컨트롤의 중앙으로 설정합니다
	/// 회전이나 스케일 변환 시 중심점을 기준으로 변환하고 싶을 때 유용합니다
	/// </summary>
	/// <param name="control">피벗을 설정할 컨트롤</param>
	public static void CenterPivotOffset(this Control control)
	{
		// 피벗을 컨트롤 크기의 절반 위치 (중앙)로 설정
		control.PivotOffset = control.Size / 2f;
	}
}
