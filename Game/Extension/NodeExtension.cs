namespace Game.Extension;

using Game.Component;

/// <summary>
/// Node 클래스의 확장 메서드를 제공하는 정적 클래스
/// 게임 전반에서 노드 조작을 간편하게 만드는 유틸리티 메서드들을 포함합니다.
/// Main.tscn의 모든 노드에서 this.메서드명() 형태로 사용 가능합니다.
/// 주요 기능: 리소스 인스턴스화, 색상 조정, 지연 자식 추가, 컴포넌트 관리
/// </summary>
public static class NodeExtension
{
	/// <summary>
	/// 전역 리소스 프리로더 참조 (싱글톤 패턴)
	/// /root/RsPreloader에서 씬과 리소스를 캐시하여 효율적으로 인스턴스화합니다.
	/// </summary>
	private static ResourcePreloader _preloader;

	/// <summary>
	/// 전역 ResourcePreloader에서 타입에 맞는 씬을 인스턴스화합니다.
	/// 리소스를 미리 로드해두고 효율적으로 재사용하기 위해 사용됩니다.
	/// </summary>
	/// <typeparam name="T">인스턴스화할 노드 타입</typeparam>
	/// <param name="node">호출 노드 (GetNode 접근용)</param>
	/// <returns>인스턴스화된 노드, 없으면 null</returns>
	public static T InstantiateFromResources<T>(this Node node) where T : Node
	{
		// 프리로더가 없으면 전역 노드에서 가져오기 (null 병합 할당)
		_preloader ??= node.GetNode<ResourcePreloader>("/root/RsPreloader");
		return _preloader.InstanceSceneOrNull<T>();
	}

	/// <summary>
	/// 색상의 강도를 조절합니다 (RGB 값에 factor를 곱함).
	/// 알파 채널은 항상 1.0으로 유지되어 완전 불투명 상태를 유지합니다.
	/// </summary>
	/// <param name="node">호출 노드 (사용되지 않음, 확장 메서드 문법용)</param>
	/// <param name="color">원본 색상</param>
	/// <param name="factor">강도 배율 (1.0보다 크면 밝아지고, 작으면 어두워짐)</param>
	/// <returns>강도가 조절된 새 색상</returns>
	public static Color IntensifyColor(this Node node, Color color, float factor)
	{
		var result = new Color(color);
		// RGB 채널에 강도 적용
		result *= factor;
		// 알파는 항상 1.0 (완전 불투명)
		result.A = 1.0f;
		return result;
	}

	/// <summary>
	/// 리소스에서 노드를 인스턴스화하고 지연 방식으로 자식에 추가합니다.
	/// CallDeferred를 사용하여 현재 프레임의 처리가 끝난 후 안전하게 추가합니다.
	/// </summary>
	/// <typeparam name="T">추가할 노드 타입</typeparam>
	/// <param name="node">부모가 될 노드</param>
	/// <returns>인스턴스화된 자식 노드</returns>
	public static T AddResourceDeferred<T>(this Node node) where T : Node
	{
		var child = node.InstantiateFromResources<T>();
		// 지연 호출로 자식 추가 (물리/렌더링 처리 중 충돌 방지)
		node.CallDeferred("add_child", child);
		return child;
	}

	/// <summary>
	/// 리소스에서 노드를 인스턴스화하고 지연 방식으로 추가한 뒤,
	/// 씬 트리에 진입하면 특정 액션을 실행합니다.
	/// 비동기로 TreeEntered 시그널을 대기하여 안전하게 초기화합니다.
	/// </summary>
	/// <typeparam name="T">추가할 노드 타입</typeparam>
	/// <param name="node">부모가 될 노드</param>
	/// <param name="action">노드가 트리에 진입한 후 실행할 액션</param>
	public static async void AddResourceDeferredWithAction<T>(this Node node, Action<T> action) where T : Node
	{
		var child = node.AddResourceDeferred<T>();
		// 자식이 씬 트리에 진입할 때까지 대기
		await child.ToSignal(child, Node.SignalName.TreeEntered);
		// 트리 진입 후 액션 실행
		action(child);
	}

	/// <summary>
	/// 노드를 QueueFreeComponent의 대기 목록에 추가합니다.
	/// QueueFreeComponent가 있으면 지정된 노드가 종료될 때까지 대기하도록 설정합니다.
	/// </summary>
	/// <param name="node">QueueFreeComponent를 검색할 노드</param>
	/// <param name="nodeToAdd">대기 목록에 추가할 노드</param>
	public static void AddNodeToQueueFreeComponent(this Node node, Node nodeToAdd)
	{
		var queueFreeComponent = node.GetFirstNodeOfType<QueueFreeComponent>();
		if (queueFreeComponent == null)
		{
			return;
		}

		// QueueFreeComponent에 노드 종료 대기 등록
		queueFreeComponent.AddWaitForNodeExit(nodeToAdd);
	}

	/// <summary>
	/// 리소스에서 노드를 인스턴스화하고 지연 방식으로 추가한 뒤,
	/// QueueFreeComponent에 등록하여 관리하도록 합니다.
	/// </summary>
	/// <typeparam name="T">추가할 노드 타입</typeparam>
	/// <param name="node">부모가 될 노드</param>
	public static void AddResourceAndQueueFree<T>(this Node node) where T : Node =>
		node.AddResourceDeferredWithAction<T>(node.AddNodeToQueueFreeComponent);

	/// <summary>
	/// 지정된 타입의 자식 컴포넌트를 활성화 또는 비활성화합니다.
	/// 컴포넌트를 찾지 못하면 아무 동작도 하지 않습니다.
	/// </summary>
	/// <typeparam name="T">활성화/비활성화할 컴포넌트 타입 (BaseComponent의 서브클래스)</typeparam>
	/// <param name="node">컴포넌트를 검색할 노드</param>
	/// <param name="enabled">활성화 여부 (true: 활성화, false: 비활성화)</param>
	public static void EnableComponent<T>(this Node node, bool enabled) where T : BaseComponent =>
		node.GetFirstNodeOfType<T>()?.SetEnabled(enabled);
}
