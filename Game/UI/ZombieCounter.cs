namespace Game.UI;

using Game.Autoload;

/// <summary>
/// 게임에 존재하는 좀비의 개수를 표시하는 UI 라벨 클래스
/// Main.tscn의 Hud/Top Right/ZombieCounter에 연결된다.
/// 좀비가 생성되거나 처치될 때마다 카운트를 업데이트한다.
/// </summary>
public partial class ZombieCounter : Label
{
	/// <summary>현재 게임 월드에 존재하는 총 좀비의 개수</summary>
	private int _totalZombies;

	/// <summary>
	/// 라벨이 준비되었을 때 호출되는 라이프사이클 메서드
	/// 좀비 생성 및 처치 이벤트 리스너를 등록한다.
	/// </summary>
	public override void _Ready()
	{
		// GameEvents의 ZombieSpawned 신호 구독
		// Main.cs에서 새로운 좀비가 생성될 때마다 호출됨
		GameEvents.Instance.ZombieSpawned += _ =>
		{
			// 좀비 개수 증가
			_totalZombies += 1;

			// 좀비 카운트 텍스트 업데이트
			Text = $"Zombies: {_totalZombies}";
		};

		// GameEvents의 ZombieKilled 신호 구독
		// Zombie.cs의 _CheckHealth에서 체력이 0이 되었을 때 호출됨
		GameEvents.Instance.ZombieKilled += _ =>
		{
			// 좀비 개수 감소
			_totalZombies -= 1;

			// 좀비 카운트 텍스트 업데이트
			Text = $"Zombies: {_totalZombies}";
		};
	}
}
