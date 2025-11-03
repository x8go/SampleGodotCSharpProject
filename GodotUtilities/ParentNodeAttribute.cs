namespace GodotUtilities;

/// <summary>
/// 부모 노드를 자동으로 필드나 프로퍼티에 할당하기 위한 어트리뷰트
/// Node 클래스의 WireNodes() 확장 메서드와 함께 사용됩니다
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class ParentAttribute : Attribute
{
	/// <summary>
	/// ParentAttribute 생성자
	/// </summary>
	public ParentAttribute()
	{ }
}
