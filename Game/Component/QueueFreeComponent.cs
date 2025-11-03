namespace Game.Component;

/// <summary>
/// 특정 자식 노드들의 종료를 대기한 후 부모 노드를 제거하는 컴포넌트
/// Zombie.cs의 _CheckHealth에서 사용되어 ScoreAttractorComponent가 완료될 때까지 좀비를 유지한다.
/// 여러 개의 노드 종료를 추적할 수 있다.
/// </summary>
public partial class QueueFreeComponent : BaseComponent
{
	/// <summary>아직 종료되지 않은 노드의 개수</summary>
	private int _exitCount;

	/// <summary>이 컴포넌트를 소유한 부모 노드</summary>
	private Node _parent;

	/// <summary>
	/// 노드가 준비되었을 때(씬 로드 완료) 호출되는 라이프사이클 메서드
	/// 부모 노드를 캐시한다.
	/// </summary>
	public override void _Ready()
	{
		// 부모 노드 캐시 (TreeExited 신호를 기다리는 노드의 부모)
		_parent = GetParent();
	}

	/// <summary>
	/// 특정 노드의 종료를 대기하도록 등록하는 메서드
	/// Zombie.cs에서 ScoreAttractorComponent의 종료를 대기할 때 호출된다.
	/// </summary>
	/// <param name="node">종료를 대기할 노드</param>
	public void AddWaitForNodeExit(Node node)
	{
		// 컴포넌트가 활성화된 경우만 처리
		if (Enabled)
		{
			// 추적할 노드 개수 증가
			_exitCount++;

			// 노드의 TreeExited 신호 등록 (노드가 트리에서 제거될 때 발생)
			node.TreeExited += _NodeExitedTree;
		}
	}

	/// <summary>
	/// 등록된 노드 중 하나가 트리를 빠져나갔을 때 호출되는 콜백
	/// 모든 노드가 종료되면 부모 노드를 제거 큐에 추가한다.
	/// </summary>
	public void _NodeExitedTree()
	{
		// 컴포넌트가 활성화된 경우만 처리
		if (Enabled)
		{
			// 추적할 노드 개수 감소
			_exitCount--;

			// 모든 노드가 종료되었으면 부모 노드 제거
			if (_exitCount <= 0)
			{
				_parent.QueueFree();
			}
		}
	}
}
