namespace Game.UI;

/// <summary>
/// FPS(초당 프레임 수) 표시 UI 클래스
/// Main.tscn의 UI 레이어에 Label로 추가되어 화면에 현재 프레임률을 실시간으로 표시합니다.
/// 성능 모니터링 및 디버깅 용도로 사용됩니다.
/// </summary>
public partial class Fps : Label
{
	/// <summary>
	/// 물리 프레임마다 호출됩니다.
	/// 현재 FPS를 엔진에서 가져와 레이블 텍스트를 업데이트합니다.
	/// </summary>
	/// <param name="delta">이전 프레임으로부터의 경과 시간(초)</param>
	public override void _PhysicsProcess(double delta)
	{
		// 엔진의 현재 FPS 값을 가져와 레이블에 표시 (예: "60/s")
		Text = $"{Engine.GetFramesPerSecond()}/s";
	}
}
