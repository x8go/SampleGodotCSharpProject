namespace Game.Component;

/// <summary>
/// 모든 게임 컴포넌트의 기본 클래스
/// 컴포넌트의 활성화/비활성화 기능을 제공한다.
/// Inspector에서 Enabled 속성을 통해 컴포넌트를 제어할 수 있다.
/// </summary>
public partial class BaseComponent : Node2D
{
	/// <summary>컴포넌트의 활성화 상태를 나타내는 내부 플래그</summary>
	private bool _enabled = true;

	/// <summary>
	/// 컴포넌트의 활성화/비활성화를 제어하는 속성
	/// Inspector에서 수정 가능 (기본값: true)
	/// Setter를 통해 SetEnabled 메서드가 호출되어 활성화 상태 변경 처리
	/// </summary>
	[Export]
	public bool Enabled
	{
		set => SetEnabled(value);
		get => _enabled;
	}

	/// <summary>
	/// 컴포넌트의 활성화 상태가 변경된 후 필요한 후처리를 수행하는 가상 메서드
	/// 자식 클래스에서 오버라이드하여 활성화/비활성화 시 추가 처리를 할 수 있다.
	/// </summary>
	protected virtual void _EnabledPostProcess()
	{
	}

	/// <summary>
	/// 컴포넌트의 활성화 상태를 설정하는 메서드
	/// 상태 변경 후 _EnabledPostProcess를 호출하여 필요한 후처리를 수행한다.
	/// </summary>
	/// <param name="enabled">true면 활성화, false면 비활성화</param>
	public void SetEnabled(bool enabled)
	{
		_enabled = enabled;
		// 활성화 상태 변경 후 자식 클래스의 추가 처리 로직 호출
		_EnabledPostProcess();
	}
}
