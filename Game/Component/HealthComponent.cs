namespace Game.Component;

/// <summary>
/// 체력 관리 컴포넌트
/// 게임 오브젝트(주로 좀비)의 체력을 관리하고 시각적으로 표현한다.
/// Zombie.cs에서 사용되며, 좀비의 건강 상태를 추적한다.
/// Main.tscn의 ZombieFollowPath도 이 컴포넌트를 포함한다.
/// </summary>
public partial class HealthComponent : BaseComponent
{
	/// <summary>
	/// 현재 체력값
	/// Inspector에서 초기값 설정 가능
	/// Zombie.cs의 Visuals 색상을 결정한다.
	/// </summary>
	[Export]
	public float Health;

	/// <summary>
	/// 체력에 따른 색상 그래디언트
	/// Inspector에서 Gradient 리소스 할당
	/// 0% 체력 = 어두운 색, 100% 체력 = 밝은 색으로 좀비의 시각적 상태를 표현
	/// </summary>
	[Export]
	public Gradient HealthGradient;

	/// <summary>
	/// 체력값을 표시하는 Label 노드
	/// 안보이거나 null이면 무시됨
	/// </summary>
	[Node]
	public Label Label;

	/// <summary>
	/// 노드가 씬 트리에 들어올 때 호출되는 라이프사이클 메서드
	/// [Node] 속성으로 표시된 필드들을 자동으로 탐색하고 연결한다.
	/// </summary>
	public override void _EnterTree()
	{
		// [Node] 속성으로 마킹된 필드들을 자동으로 탐색하고 연결
		this.WireNodes();
	}

	/// <summary>
	/// 노드가 준비되었을 때(씬 로드 완료) 호출되는 라이프사이클 메서드
	/// 시각적 표현(Label과 색상)을 초기화한다.
	/// </summary>
	public override void _Ready()
	{
		_UpdateVisuals();
	}

	/// <summary>
	/// 체력값을 시각적으로 업데이트하는 메서드
	/// Label이 보이면 현재 체력값을 표시한다.
	/// </summary>
	private void _UpdateVisuals()
	{
		// Label이 보이면 현재 체력값을 텍스트로 표시
		if (Label.Visible)
			Label.Text = $"{Health}";
	}

	/// <summary>
	/// 지정된 데미지만큼 체력을 감소시키는 메서드
	/// Zombie.cs의 _CheckHealth에서 호출된다.
	/// </summary>
	/// <param name="damage">감소시킬 체력값 (양수)</param>
	/// <returns>변경 전의 체력값</returns>
	public float DecreaseHealth(float damage)
	{
		// 비활성화 상태면 변경하지 않음
		if (!Enabled) return Health;

		// 변경 전 체력값 저장
		var oldHealth = Health;

		// 체력 감소
		Health -= damage;

		// 시각적 표현 업데이트
		_UpdateVisuals();

		return oldHealth;
	}

	/// <summary>
	/// 지정된 양만큼 체력을 회복시키는 메서드
	/// </summary>
	/// <param name="damage">증가시킬 체력값 (양수)</param>
	/// <returns>변경 전의 체력값</returns>
	public float IncreaseHealth(float damage)
	{
		// 비활성화 상태면 변경하지 않음
		if (!Enabled) return Health;

		// 변경 전 체력값 저장
		var oldHealth = Health;

		// 체력 증가
		Health += damage;

		// 시각적 표현 업데이트
		_UpdateVisuals();

		return oldHealth;
	}

	/// <summary>
	/// 체력값을 직접 설정하는 메서드
	/// </summary>
	/// <param name="health">설정할 체력값</param>
	public void SetHealth(float health)
	{
		// 체력값 직접 설정
		Health = health;

		// 시각적 표현 업데이트
		_UpdateVisuals();
	}
}
