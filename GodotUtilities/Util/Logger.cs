using System.Linq;

namespace GodotUtilities.Util;

/// <summary>
/// 로깅 유틸리티를 제공하는 정적 클래스
/// 에러, 정보, 디버그 로그를 출력할 수 있습니다
/// </summary>
public static class Logger
{
	/// <summary>
	/// 에러 로그를 출력합니다
	/// [ERROR] 접두사와 함께 스택 트레이스를 출력합니다
	/// </summary>
	/// <param name="what">출력할 객체들</param>
	public static void Error(params object[] what)
	{
		// [ERROR] 접두사와 함께 에러 메시지 출력
		GD.PrintErr(new object[] { "[ERROR] " }.Concat(what).ToArray());
		// 스택 트레이스 출력 (에러 위치 추적용)
		GD.Print(System.Environment.StackTrace);
	}

	/// <summary>
	/// 정보 로그를 출력합니다
	/// [INFO] 접두사와 함께 메시지를 출력합니다
	/// </summary>
	/// <param name="what">출력할 객체들</param>
	public static void Info(params object[] what)
	{
		// [INFO] 접두사와 함께 정보 메시지 출력
		GD.PrintRaw(new object[] { "[INFO] " }.Concat(what).ToArray());
		GD.PrintRaw("\n");
	}

	/// <summary>
	/// 디버그 로그를 출력합니다 (디버그 빌드에서만 출력됨)
	/// [DEBUG] 접두사와 함께 메시지를 출력합니다
	/// </summary>
	/// <param name="what">출력할 객체들</param>
	public static void Debug(params object[] what)
	{
		// 디버그 빌드인 경우에만 출력
		if (OS.IsDebugBuild())
		{
			// [DEBUG] 접두사와 함께 디버그 메시지 출력
			GD.PrintRaw(new object[] { "[DEBUG] " }.Concat(what).ToArray());
			GD.PrintRaw("\n");
		}
	}
}
