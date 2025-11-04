namespace Game.Helpers;

/// <summary>
/// 원형 패턴으로 랜덤한 2D 좌표를 생성하는 헬퍼 클래스
/// Main.tscn에서 적(좀비 등)의 스폰 위치를 원형으로 배치하는 데 사용됩니다.
/// 중심점을 기준으로 반지름과 패딩을 고려하여 랜덤한 위치를 생성합니다.
/// </summary>
internal class PointGenerator
{
	/// <summary>
	/// 원의 중심점 좌표
	/// 생성되는 모든 점들은 이 중심을 기준으로 배치됩니다.
	/// </summary>
	private readonly Vector2 _center;

	/// <summary>
	/// 반지름에 추가되는 랜덤 패딩 값의 기본 배율
	/// 점들이 원의 경계에서 벗어나는 정도를 조절합니다.
	/// </summary>
	private readonly float _padding;

	/// <summary>
	/// 원의 기본 반지름
	/// 중심점으로부터의 기본 거리를 정의합니다.
	/// </summary>
	private readonly float _radius;

	/// <summary>
	/// 랜덤 숫자 생성기
	/// 시드를 고정하여 재현 가능한 랜덤 패턴을 생성합니다.
	/// </summary>
	private readonly RandomNumberGenerator _random = new();

	/// <summary>
	/// PointGenerator의 생성자
	/// 중심점, 반지름, 패딩을 설정하고 랜덤 시드를 초기화합니다.
	/// </summary>
	/// <param name="center">원의 중심점 좌표</param>
	/// <param name="radius">원의 기본 반지름</param>
	/// <param name="padding">반지름에 추가할 랜덤 패딩의 기본 배율</param>
	public PointGenerator(Vector2 center, float radius, float padding)
	{
		_center = center;
		_radius = radius;
		_padding = padding;

		// 고정된 시드로 재현 가능한 랜덤 패턴 생성
		_random.Seed = 1234L;
	}

	/// <summary>
	/// 지정된 개수만큼 랜덤한 점들을 생성합니다.
	/// 각 점은 원형 패턴으로 중심 주위에 분산됩니다.
	/// </summary>
	/// <param name="totalPoints">생성할 점의 총 개수</param>
	/// <returns>생성된 Vector2 좌표 배열</returns>
	public Vector2[] GeneratePoints(int totalPoints)
	{
		var points = new Vector2[totalPoints];

		// 지정된 개수만큼 점 생성
		for (var i = 0; i < totalPoints; i++)
		{
			var point = GeneratePoint();
			points[i] = point;
		}

		return points;
	}

	/// <summary>
	/// 랜덤한 하나의 점을 생성합니다.
	/// 중심점을 기준으로 랜덤한 반지름과 각도를 사용하여 원형 패턴의 점을 계산합니다.
	/// </summary>
	/// <returns>생성된 Vector2 좌표</returns>
	public Vector2 GeneratePoint()
	{
		// 랜덤 패딩을 추가한 반지름 계산 (0.1 ~ 3.0 배율)
		var radius = _radius + _padding * _random.RandfRange(0.1f, 3.0f);
		// 랜덤 각도 (0 ~ 720도 - 2바퀴 회전 가능)
		var angle = _random.RandfRange(0.0f, 720.0f);
		// 극좌표를 직교좌표로 변환 (X = 중심X + 반지름 * cos(각도))
		var x = _center.X + radius * Mathf.Cos(Mathf.DegToRad(angle));
		// 극좌표를 직교좌표로 변환 (Y = 중심Y + 반지름 * sin(각도))
		var y = _center.Y + radius * Mathf.Sin(Mathf.DegToRad(angle));
		return new Vector2(x, y);
	}
}
