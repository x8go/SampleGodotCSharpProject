using System.Collections.Generic;

namespace GodotUtilities.Logic;

/// <summary>
/// 즉시 실행형 상태 머신 클래스
/// 매 프레임 업데이트가 필요 없는 상태들을 위한 상태 머신입니다
/// 상태 전환 시 해당 상태의 로직이 즉시 한 번만 실행됩니다
/// </summary>
/// <typeparam name="T">상태를 나타내는 타입 (보통 Enum)</typeparam>
public class ImmediateStateMachine<T>
{
	/// <summary>
	/// 상태 델리게이트 (상태 진입 시 실행될 로직)
	/// </summary>
	public delegate void StateDelegate();

	/// <summary>
	/// 현재 활성 상태
	/// </summary>
	public T CurrentState { get; private set; }

	/// <summary>
	/// 상태 -> 상태 델리게이트 매핑 (상태 진입 시 실행)
	/// </summary>
	private readonly Dictionary<T, StateDelegate> _states = new();

	/// <summary>
	/// 델리게이트 -> 상태 역매핑
	/// </summary>
	private readonly Dictionary<StateDelegate, T> _delegates = new();

	/// <summary>
	/// 상태 -> 퇴장 델리게이트 매핑 (상태를 떠날 때 실행)
	/// </summary>
	private readonly Dictionary<T, StateDelegate> _leaveStates = new();

	/// <summary>
	/// 새로운 상태와 해당 상태의 진입 델리게이트를 추가합니다
	/// </summary>
	/// <param name="state">추가할 상태</param>
	/// <param name="del">상태에 진입할 때 즉시 실행될 델리게이트</param>
	public void AddState(T state, StateDelegate del)
	{
		_states.Add(state, del);
		_delegates.Add(del, state); // 역방향 매핑도 추가
	}

	/// <summary>
	/// 특정 상태를 떠날 때 실행될 델리게이트를 추가합니다
	/// </summary>
	/// <param name="stateToLeave">퇴장 이벤트를 설정할 상태</param>
	/// <param name="del">상태를 떠날 때 실행될 델리게이트</param>
	public void AddLeaveState(T stateToLeave, StateDelegate del)
	{
		_leaveStates.Add(stateToLeave, del);
	}

	/// <summary>
	/// 상태를 즉시 변경합니다
	/// 퇴장 델리게이트 -> 상태 변경 -> 진입 델리게이트 순서로 실행됩니다
	/// </summary>
	/// <param name="state">전환할 상태</param>
	public void ChangeState(T state)
	{
		// 현재 상태의 퇴장 델리게이트 실행
		if (_leaveStates.ContainsKey(CurrentState))
		{
			_leaveStates[CurrentState]();
		}
		// 상태 변경
		CurrentState = state;
		// 새 상태의 진입 델리게이트 실행
		if (_states.ContainsKey(CurrentState))
		{
			_states[CurrentState]();
		}
	}

	/// <summary>
	/// 델리게이트를 사용하여 상태를 즉시 변경합니다
	/// </summary>
	/// <param name="stateDelegate">전환할 상태의 델리게이트</param>
	public void ChangeState(StateDelegate stateDelegate)
	{
		// 델리게이트로부터 상태를 찾아 변경
		ChangeState(_delegates[stateDelegate]);
	}
}
