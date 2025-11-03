using System.Collections.Generic;

namespace GodotUtilities.Logic;

/// <summary>
/// 델리게이트 기반 상태 머신 클래스
/// 델리게이트 자체를 상태로 사용하는 간단한 상태 머신입니다
/// </summary>
public partial class DelegateStateMachine : RefCounted
{
	/// <summary>
	/// 상태 델리게이트 타입
	/// </summary>
	public delegate void State();

	/// <summary>
	/// 현재 활성 상태 (델리게이트)
	/// </summary>
	private State currentState;

	/// <summary>
	/// 상태 델리게이트 -> StateFlows 매핑 (진입/퇴장 델리게이트 포함)
	/// </summary>
	private readonly Dictionary<State, StateFlows> states = new();

	/// <summary>
	/// 상태와 관련 델리게이트들을 추가합니다
	/// </summary>
	/// <param name="normal">매 프레임 실행될 일반 상태 델리게이트</param>
	/// <param name="enterState">상태 진입 시 실행될 델리게이트 (선택)</param>
	/// <param name="leaveState">상태 퇴장 시 실행될 델리게이트 (선택)</param>
	public void AddStates(State normal, State enterState = null, State leaveState = null)
	{
		// StateFlows 객체 생성 및 저장
		var stateFlows = new StateFlows(normal, enterState, leaveState);
		states[normal] = stateFlows;
	}

	/// <summary>
	/// 지연 방식으로 상태를 변경합니다 (다음 프레임에 변경됨)
	/// </summary>
	/// <param name="toStateDelegate">전환할 상태 델리게이트</param>
	public void ChangeState(State toStateDelegate)
	{
		// StateFlows 검색
		states.TryGetValue(toStateDelegate, out var stateDelegates);
		// 다음 프레임에 상태 변경
		Callable.From(() => SetState(stateDelegates)).CallDeferred();
	}

	/// <summary>
	/// 초기 상태를 즉시 설정합니다
	/// </summary>
	/// <param name="stateDelegate">초기 상태 델리게이트</param>
	public void SetInitialState(State stateDelegate)
	{
		// StateFlows 검색 및 설정
		states.TryGetValue(stateDelegate, out var stateFlows);
		SetState(stateFlows);
	}

	/// <summary>
	/// 현재 활성 상태 델리게이트를 반환합니다
	/// </summary>
	/// <returns>현재 상태 델리게이트</returns>
	public State GetCurrentState()
	{
		return currentState;
	}

	/// <summary>
	/// 현재 상태 델리게이트를 실행합니다
	/// 매 프레임마다 호출해야 합니다
	/// </summary>
	public void Update()
	{
		// 현재 상태의 업데이트 로직 실행
		currentState?.Invoke();
	}

	/// <summary>
	/// 상태를 즉시 변경합니다 (내부 메서드)
	/// 퇴장 -> 상태 변경 -> 진입 순서로 실행됩니다
	/// </summary>
	/// <param name="stateFlows">전환할 상태의 StateFlows</param>
	private void SetState(StateFlows stateFlows)
	{
		// 현재 상태의 퇴장 델리게이트 실행
		if (currentState != null)
		{
			states.TryGetValue(currentState, out var currentStateDelegates);
			currentStateDelegates?.LeaveState?.Invoke();
		}
		// 상태 변경
		currentState = stateFlows.Normal;
		// 새 상태의 진입 델리게이트 실행
		stateFlows?.EnterState?.Invoke();
	}

	/// <summary>
	/// 상태의 진입/일반/퇴장 델리게이트를 묶어 관리하는 내부 클래스
	/// </summary>
	private class StateFlows
	{
		/// <summary>
		/// 매 프레임 실행될 일반 상태 델리게이트
		/// </summary>
		public State Normal { get; private set; }

		/// <summary>
		/// 상태 진입 시 실행될 델리게이트
		/// </summary>
		public State EnterState { get; private set; }

		/// <summary>
		/// 상태 퇴장 시 실행될 델리게이트
		/// </summary>
		public State LeaveState { get; private set; }

		/// <summary>
		/// StateFlows 생성자
		/// </summary>
		/// <param name="normal">일반 상태 델리게이트</param>
		/// <param name="enterState">진입 델리게이트 (선택)</param>
		/// <param name="leaveState">퇴장 델리게이트 (선택)</param>
		public StateFlows(State normal, State enterState = null, State leaveState = null)
		{
			Normal = normal;
			EnterState = enterState;
			LeaveState = leaveState;
		}
	}
}
