namespace GodotUtilities;

/// <summary>
/// 수학 관련 유틸리티 메서드를 제공하는 정적 클래스
/// 전역 난수 생성기 및 보간 함수 등을 포함합니다
/// </summary>
public static class MathUtil
{
	/// <summary>
	/// 전역 난수 생성기
	/// 프로젝트 전체에서 공유되는 RandomNumberGenerator 인스턴스
	/// </summary>
	public static RandomNumberGenerator RNG { get; private set; } = new RandomNumberGenerator();

	/// <summary>
	/// 정적 생성자
	/// MathUtil 클래스가 처음 사용될 때 난수 생성기를 초기화합니다
	/// </summary>
	static MathUtil()
	{
		// 난수 생성기를 랜덤 시드로 초기화
		RNG.Randomize();
	}

	/// <summary>
	/// 델타 타임 기반 선형 보간 값을 계산합니다
	/// 프레임 독립적인 부드러운 전환을 구현할 때 사용합니다
	/// </summary>
	/// <param name="smoothing">부드러움 정도 (0에 가까울수록 빠른 전환)</param>
	/// <param name="delta">델타 타임</param>
	/// <returns>보간에 사용할 t 값</returns>
	public static float DeltaLerp(float smoothing, float delta)
	{
		// 프레임 독립적인 보간 계산 (exponential smoothing)
		return 1f - Mathf.Pow(smoothing, delta);
	}

	/// <summary>
	/// 난수 생성기를 지정된 시드로 재초기화합니다
	/// 재현 가능한 랜덤 시퀀스가 필요할 때 사용합니다
	/// </summary>
	/// <param name="seed">난수 생성기 시드</param>
	public static void SeedRandomNumberGenerator(ulong seed)
	{
		// 새로운 난수 생성기를 지정된 시드로 생성
		RNG = new RandomNumberGenerator
		{
			Seed = seed
		};
	}
}
