namespace GodotUtilities;

/// <summary>
/// AudioStreamPlayer 및 AudioStreamPlayer2D 클래스의 확장 메서드를 제공하는 정적 클래스
/// 오디오 재생 시 피치 변조를 편리하게 적용할 수 있습니다
/// </summary>
public static class AudioStreamPlayerExtension
{
	/// <summary>
	/// AudioStreamPlayer의 확장 메서드: 랜덤한 피치로 사운드를 재생합니다
	/// 같은 사운드를 다양한 느낌으로 재생하고 싶을 때 유용합니다
	/// </summary>
	/// <param name="audioStreamPlayer">오디오 플레이어</param>
	/// <param name="minPitchScale">최소 피치 스케일</param>
	/// <param name="maxPitchScale">최대 피치 스케일</param>
	public static void PlayWithPitchRange(this AudioStreamPlayer audioStreamPlayer, float minPitchScale, float maxPitchScale)
	{
		// 지정된 범위 내에서 랜덤 피치 설정
		audioStreamPlayer.PitchScale = MathUtil.RNG.RandfRange(minPitchScale, maxPitchScale);
		audioStreamPlayer.Play();
	}

	/// <summary>
	/// AudioStreamPlayer의 확장 메서드: 지정된 피치로 사운드를 재생합니다
	/// </summary>
	/// <param name="audioStreamPlayer">오디오 플레이어</param>
	/// <param name="pitchScale">피치 스케일 (1.0이 기본)</param>
	public static void PlayWithPitch(this AudioStreamPlayer audioStreamPlayer, float pitchScale)
	{
		// 피치 설정 후 재생
		audioStreamPlayer.PitchScale = pitchScale;
		audioStreamPlayer.Play();
	}

	/// <summary>
	/// AudioStreamPlayer2D의 확장 메서드: 지정된 피치로 사운드를 재생합니다
	/// </summary>
	/// <param name="audioStreamPlayer">2D 오디오 플레이어</param>
	/// <param name="pitchScale">피치 스케일 (1.0이 기본)</param>
	public static void PlayWithPitch(this AudioStreamPlayer2D audioStreamPlayer, float pitchScale)
	{
		// 피치 설정 후 재생
		audioStreamPlayer.PitchScale = pitchScale;
		audioStreamPlayer.Play();
	}

	/// <summary>
	/// AudioStreamPlayer2D의 확장 메서드: 랜덤한 피치로 사운드를 재생합니다
	/// 같은 사운드를 다양한 느낌으로 재생하고 싶을 때 유용합니다
	/// </summary>
	/// <param name="audioStreamPlayer">2D 오디오 플레이어</param>
	/// <param name="minPitchScale">최소 피치 스케일</param>
	/// <param name="maxPitchScale">최대 피치 스케일</param>
	public static void PlayWithPitchRange(this AudioStreamPlayer2D audioStreamPlayer, float minPitchScale, float maxPitchScale)
	{
		// 지정된 범위 내에서 랜덤 피치 설정
		audioStreamPlayer.PitchScale = MathUtil.RNG.RandfRange(minPitchScale, maxPitchScale);
		audioStreamPlayer.Play();
	}
}
