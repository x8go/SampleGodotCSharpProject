using Godot.Collections;

namespace GodotUtilities.Util;

/// <summary>
/// 2D 레이캐스트 충돌 결과를 저장하는 클래스
/// 레이캐스트는 광선을 발사하여 충돌을 감지하는 방식입니다
/// </summary>
public class RaycastResult
{
	/// <summary>
	/// 충돌 위치
	/// </summary>
	public Vector2 Position { get; set; }

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
	/// 레이캐스트 시작 위치
	/// </summary>
	public Vector2 FromPosition { get; set; }

	/// <summary>
	/// 레이캐스트 종료 위치
	/// </summary>
	public Vector2 ToPosition { get; set; }

	/// <summary>
	/// RaycastResult 생성자
	/// Godot의 레이캐스트 결과 딕셔너리를 파싱하여 객체를 생성합니다
	/// </summary>
	/// <param name="from">시작 위치</param>
	/// <param name="to">종료 위치</param>
	/// <param name="resultDict">Godot의 충돌 결과 딕셔너리</param>
	public RaycastResult(Vector2 from, Vector2 to, Dictionary resultDict)
	{
		FromPosition = from;
		ToPosition = to;
		// 딕셔너리에서 각 값 추출
		Position = (Vector2)resultDict["position"];
		Normal = (Vector2)resultDict["normal"];
		Collider = (GodotObject)resultDict["collider"];
		ColliderId = (int)resultDict["collider_id"];
		Rid = (Rid)resultDict["rid"];
		Shape = (int)resultDict["shape"];
	}
}
