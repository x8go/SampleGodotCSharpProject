namespace GodotUtilities;

/// <summary>
/// Vector2 클래스의 확장 메서드를 제공하는 정적 클래스
/// 벡터 연산을 보다 편리하게 사용할 수 있는 유틸리티 메서드 포함
/// </summary>
public static class VectorExtension
{
	/// <summary>
	/// 벡터 길이 근사 계산에 사용되는 알파 상수
	/// </summary>
	private const float ALPHA = .9604339f;

	/// <summary>
	/// 벡터 길이 근사 계산에 사용되는 베타 상수
	/// </summary>
	private const float BETA = .3978247f;

	/// <summary>
	/// Vector2의 확장 메서드: 벡터의 길이를 근사값으로 빠르게 계산합니다
	/// 정확한 계산보다 빠르지만 약간의 오차가 있습니다
	/// </summary>
	/// <param name="vec">길이를 계산할 벡터</param>
	/// <returns>벡터 길이의 근사값</returns>
	public static float ApproximateLength(this Vector2 vec)
	{
		var absVec = vec.Abs();
		var min = Mathf.Min(absVec.X, absVec.Y);
		var max = Mathf.Max(absVec.X, absVec.Y);
		// 알파-베타 필터를 사용한 근사 계산
		return (ALPHA * max) + (BETA * min);
	}

	/// <summary>
	/// Vector2의 확장 메서드: 벡터를 도(degree) 단위로 회전시킵니다
	/// </summary>
	/// <param name="v">회전할 벡터</param>
	/// <param name="degrees">회전 각도 (도 단위)</param>
	/// <returns>회전된 벡터</returns>
	public static Vector2 RotatedDegrees(this Vector2 v, float degrees)
	{
		// 도를 라디안으로 변환하여 회전
		return v.Rotated(Mathf.DegToRad(degrees));
	}

	/// <summary>
	/// Vector2의 확장 메서드: 두 벡터 사이의 거리가 지정된 거리 이내인지 확인합니다
	/// 제곱근 연산을 피하기 위해 거리의 제곱을 비교합니다
	/// </summary>
	/// <param name="v1">첫 번째 벡터</param>
	/// <param name="v2">두 번째 벡터</param>
	/// <param name="distance">비교할 거리</param>
	/// <returns>거리 이내이면 true, 아니면 false</returns>
	public static bool IsWithinDistanceSquared(this Vector2 v1, Vector2 v2, float distance)
	{
		// 성능 최적화: 제곱근 계산 없이 제곱 거리로 비교
		return v1.DistanceSquaredTo(v2) <= distance * distance;
	}
}
