namespace Game.Component.Element;

/// <summary>
/// 원소 컴포넌트의 기본 클래스 - 게임 내 모든 원소(불, 물 등)의 기본 동작을 정의합니다.
/// Main.tscn에서 다양한 엔티티(플레이어, 좀비 등)에 추가되어 원소 속성을 부여합니다.
/// </summary>
public partial class ElementComponent : BaseComponent
{
	/// <summary>
	/// 원소의 강도가 0이 되어 소진되었을 때 발생하는 시그널
	/// 게임 내 다른 시스템(예: 불 끄기, 이펙트 중지)에서 이 신호를 받아 반응합니다.
	/// </summary>
	[Signal]
	public delegate void IntensityDepletedEventHandler(ElementComponent element);

	/// <summary>
	/// 원소의 강도가 최대치(1.0)에 도달했을 때 발생하는 시그널
	/// 완전히 활성화된 상태를 다른 시스템에 알립니다.
	/// </summary>
	[Signal]
	public delegate void IntensityMaxedEventHandler(ElementComponent element);

	/// <summary>
	/// 원소의 현재 에너지 레벨 (0.0 ~ 1.0 범위)
	/// 0.0은 완전히 비활성화, 1.0은 최대 활성화 상태를 의미합니다.
	/// </summary>
	protected float Energy;

	/// <summary>
	/// 에너지를 증가 또는 감소시킵니다 (factor가 음수면 감소)
	/// 파생 클래스에서 오버라이드하여 구체적인 동작을 구현합니다.
	/// </summary>
	/// <param name="factor">에너지 변화량 (양수: 증가, 음수: 감소)</param>
	/// <param name="emitSignals">true면 에너지 변화 시 시그널을 발생시킴</param>
	/// <returns>변경 전 에너지 값</returns>
	public virtual float AddEnergy(float factor, bool emitSignals = true)
	{
		return 0.0f;
	}

	/// <summary>
	/// 에너지를 특정 값으로 직접 설정합니다.
	/// 자동으로 0.0 ~ 1.0 범위로 제한됩니다.
	/// </summary>
	/// <param name="energy">설정할 에너지 값</param>
	/// <param name="emitSignals">true면 에너지 변화 시 시그널을 발생시킴</param>
	/// <returns>변경 전 에너지 값</returns>
	public virtual float SetEnergy(float energy, bool emitSignals = true)
	{
		var oldEnergy = Energy;
		// 에너지를 0.0 ~ 1.0 범위로 제한
		Energy = Mathf.Clamp(energy, 0f, 1f);
		return oldEnergy;
	}

	/// <summary>
	/// 현재 에너지 레벨을 반환합니다.
	/// </summary>
	/// <returns>현재 에너지 값 (0.0 ~ 1.0)</returns>
	public float GetEnergy() => Energy;
}
