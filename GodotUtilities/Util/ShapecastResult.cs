namespace GodotUtilities.Util;

/// <summary>
/// 2D 셰이프캐스트(도형 투사) 충돌 결과를 저장하는 클래스
/// 셰이프캐스트는 도형을 이동시켜 충돌을 감지하는 방식입니다
/// </summary>
public class ShapecastResult
{
	/// <summary>
	/// 충돌 지점
	/// </summary>
	public Vector2 Point { get; set; }

	/// <summary>
	/// 충돌 표면의 법선 벡터
	/// </summary>
	public Vector2 Normal { get; set; }

	/// <summary>
	/// 충돌한 오브젝트
	/// </summary>
	public GodotObject Collider { get; set; }

	/// <summary>
	/// 충돌한 오브젝트의 ID
	/// </summary>
	public int ColliderId { get; set; }

	/// <summary>
	/// 충돌한 오브젝트의 RID (Resource ID)
	/// </summary>
	public Rid Rid { get; set; }

	/// <summary>
	/// 충돌한 셰이프의 인덱스
	/// </summary>
	public int Shape { get; set; }

	/// <summary>
	/// 셰이프캐스트 시작 위치
	/// </summary>
	public Vector2 FromPosition { get; set; }

	/// <summary>
	/// 셰이프캐스트 종료 위치
	/// </summary>
	public Vector2 ToPosition { get; set; }

	/// <summary>
	/// 충돌한 오브젝트의 선형 속도
	/// </summary>
	public Vector2 LinearVelocity { get; set; }

	/// <summary>
	/// ShapecastResult 생성자
	/// Godot의 셰이프캐스트 결과 딕셔너리를 파싱하여 객체를 생성합니다
	/// </summary>
	/// <param name="from">시작 위치</param>
	/// <param name="to">종료 위치</param>
	/// <param name="resultDict">Godot의 충돌 결과 딕셔너리</param>
	public ShapecastResult(Vector2 from, Vector2 to, Godot.Collections.Dictionary resultDict)
	{
		FromPosition = from;
		ToPosition = to;
		// 딕셔너리에서 각 값 추출
		Point = (Vector2)resultDict["point"];
		Normal = (Vector2)resultDict["normal"];
		Collider = (GodotObject)resultDict["collider"];
		ColliderId = (int)resultDict["collider_id"];
		Rid = (Rid)resultDict["rid"];
		Shape = (int)resultDict["shape"];
		LinearVelocity = (Vector2)resultDict["linear_velocity"];
	}
}
