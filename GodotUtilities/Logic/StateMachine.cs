using System.Collections.Generic;

namespace GodotUtilities.Logic;

/// <summary>
/// 제네릭 상태 머신 클래스
/// 상태 전환과 각 상태의 업데이트, 진입/퇴장 이벤트를 관리합니다
/// </summary>
/// <typeparam name="T">상태를 나타내는 타입 (보통 Enum)</typeparam>
public partial class StateMachine<T> : RefCounted
{
	/// <summary>
	/// 상태 델리게이트 (상태의 업데이트, 진입, 퇴장 로직을 정의)
	/// </summary>
	public delegate void StateDelegate();

	/// <summary>
	/// 현재 활성 상태
	/// </summary>
	private T currentState;

	/// <summary>
	/// 상태 -> 상태 업데이트 델리게이트 매핑
	/// </summary>
	private readonly Dictionary<T, StateDelegate> states = new();

	/// <summary>
	/// 델리게이트 -> 상태 역매핑 (델리게이트로 상태 전환 시 사용)
	/// </summary>
	private readonly Dictionary<StateDelegate, T> delegates = new();

	/// <summary>
	/// 상태 -> 퇴장 델리게이트 매핑 (상태를 떠날 때 실행)
	/// </summary>
	private readonly Dictionary<T, StateDelegate> leaveStates = new();

	/// <summary>
	/// 상태 -> 진입 델리게이트 매핑 (상태에 진입할 때 실행)
	/// </summary>
	private readonly Dictionary<T, StateDelegate> enterStates = new();

	/// <summary>
	/// 새로운 상태와 해당 상태의 업데이트 델리게이트를 추가합니다
	/// </summary>
	/// <param name="state">추가할 상태</param>
	/// <param name="del">상태가 활성화되어 있는 동안 매 프레임 실행될 델리게이트</param>
	public void AddState(T state, StateDelegate del)
	{
		states.Add(state, del);
		delegates.Add(del, state); // 역방향 매핑도 추가
	}

	/// <summary>
	/// 특정 상태를 떠날 때 실행될 델리게이트를 추가합니다
	/// </summary>
	/// <param name="stateToLeave">퇴장 이벤트를 설정할 상태</param>
	/// <param name="del">상태를 떠날 때 실행될 델리게이트</param>
	public void AddLeaveState(T stateToLeave, StateDelegate del)
	{
		leaveStates.Add(stateToLeave, del);
	}

	/// <summary>
	/// 특정 상태에 진입할 때 실행될 델리게이트를 추가합니다
	/// </summary>
	/// <param name="enterState">진입 이벤트를 설정할 상태</param>
	/// <param name="del">상태에 진입할 때 실행될 델리게이트</param>
	public void AddEnterState(T enterState, StateDelegate del)
	{
		enterStates.Add(enterState, del);
	}

	/// <summary>
	/// 지연 방식으로 상태를 변경합니다 (다음 프레임에 변경됨)
	/// </summary>
	/// <param name="state">전환할 상태</param>
	public void ChangeState(T state)
	{
		// 다음 프레임에 상태 변경
		Callable.From(() => SetState(state)).CallDeferred();
	}

	/// <summary>
	/// 델리게이트를 사용하여 지연 방식으로 상태를 변경합니다
	/// </summary>
	/// <param name="stateDelegate">전환할 상태의 델리게이트</param>
	public void ChangeState(StateDelegate stateDelegate)
	{
		// 델리게이트로부터 상태를 찾아 변경
		ChangeState(delegates[stateDelegate]);
	}

	/// <summary>
	/// 초기 상태를 즉시 설정합니다
	/// </summary>
	/// <param name="state">초기 상태</param>
	public void SetInitialState(T state)
	{
		SetState(state);
	}

	/// <summary>
	/// 델리게이트를 사용하여 초기 상태를 설정합니다
	/// </summary>
	/// <param name="del">초기 상태의 델리게이트</param>
	public void SetInitialState(StateDelegate del)
	{
		SetInitialState(delegates[del]);
	}

	/// <summary>
	/// 현재 활성 상태를 반환합니다
	/// </summary>
	/// <returns>현재 상태</returns>
	public T GetCurrentState()
	{
		return currentState;
	}

	/// <summary>
	/// 현재 상태의 업데이트 델리게이트를 실행합니다
	/// 매 프레임마다 호출해야 합니다
	/// </summary>
	public void Update()
	{
		if (states.ContainsKey(currentState))
		{
			// 현재 상태의 업데이트 로직 실행
			states[currentState]();
		}
	}

	/// <summary>
	/// 상태를 즉시 변경합니다 (내부 메서드)
	/// 퇴장 -> 상태 변경 -> 진입 순서로 실행됩니다
	/// </summary>
	/// <param name="state">전환할 상태</param>
	private void SetState(T state)
	{
		// 현재 상태의 퇴장 델리게이트 실행
		if (leaveStates.ContainsKey(currentState))
		{
			leaveStates[currentState]();
		}
		// 상태 변경
		currentState = state;
		// 새 상태의 진입 델리게이트 실행
		if (enterStates.ContainsKey(currentState))
		{
			enterStates[currentState]();
		}
	}
}
