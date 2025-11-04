namespace Game.Manager;

using Game.Autoload;

/// <summary>
/// 게임 내 모든 사운드 효과를 관리하는 매니저 클래스
/// Main.tscn의 매니저 노드로 추가되어 게임 이벤트에 따라 적절한 사운드를 재생합니다.
/// GameEvents의 전역 이벤트를 구독하여 플레이어 피격, 좀비 사망, 불 효과 등의 사운드를 처리합니다.
/// </summary>
public partial class SoundManager : Node
{
	/// <summary>
	/// 사운드 매니저 활성화 여부
	/// Inspector에서 설정 가능 - false로 설정하면 모든 사운드가 비활성화됩니다.
	/// 기본값: true
	/// </summary>
	[Export]
	public bool Enabled = true;

	/// <summary>
	/// 폭발 사운드를 재생하는 AudioStreamPlayer2D (좀비 사망 시 사용)
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// 2D 공간에서 위치 기반 사운드를 재생합니다.
	/// </summary>
	[Node]
	public AudioStreamPlayer2D Explosion;

	/// <summary>
	/// 플레이어 피격 사운드를 재생하는 AudioStreamPlayer2D
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// 플레이어가 피격당한 위치에서 사운드를 재생합니다.
	/// </summary>
	[Node]
	public AudioStreamPlayer2D PlayerHit;

	/// <summary>
	/// 불 타는 사운드를 재생하는 AudioStreamPlayer (배경 사운드)
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// 위치와 무관한 전역 사운드로 재생됩니다.
	/// </summary>
	[Node]
	public AudioStreamPlayer Fire;

	/// <summary>
	/// 현재 활성화된 불의 강도 (불 사운드 볼륨 조절용)
	/// 불 컴포넌트가 활성화될 때마다 증가하고, 비활성화될 때 감소합니다.
	/// </summary>
	private float _fireIntensity;

	/// <summary>
	/// 노드가 씬 트리에 진입할 때 호출됩니다.
	/// [Node] 속성이 붙은 모든 필드를 자동으로 연결합니다.
	/// </summary>
	public override void _EnterTree()
	{
		this.WireNodes();
	}

	/// <summary>
	/// 노드가 준비되고 모든 자식 노드가 씬에 진입했을 때 호출됩니다.
	/// 게임의 전역 이벤트들을 구독하여 사운드를 재생합니다.
	/// </summary>
	public override void _Ready()
	{
		// 플레이어 피격 이벤트 구독
		GameEvents.Instance.PlayerHit += (player, _, _) =>
		{
			if (!Enabled) return;
			// 사운드를 플레이어 위치에서 재생
			PlayerHit.GlobalPosition = player.GlobalPosition;
			// 피치를 랜덤하게 변경하여 다양한 효과 연출 (0.8 ~ 1.1 범위)
			PlayerHit.PlayWithPitch((float)GD.RandRange(0.8f, 1.1f));
		};

		// 좀비 사망 이벤트 구독
		GameEvents.Instance.ZombieKilled += zombie =>
		{
			if (!Enabled) return;
			// 사운드를 좀비 위치에서 재생
			Explosion.GlobalPosition = zombie.GlobalPosition;
			// 피치를 랜덤하게 변경하여 다양한 폭발음 연출 (0.6 ~ 1.4 범위)
			Explosion.PlayWithPitch((float)GD.RandRange(0.6f, 1.4f));
		};

		// 원소(불) 강도 소진 이벤트 구독
		GameEvents.Instance.ElementIntensityDepleted += _ =>
		{
			if (!Enabled) return;
			// 불 강도 감소
			_fireIntensity = Mathf.Max(_fireIntensity - 1.0f, 0.0f);
			// 모든 불이 꺼지면 불 사운드 중지
			if (_fireIntensity <= 0.0f)
			{
				if (Fire.Playing) Fire.Stop();
			}
		};

		// 원소(불) 강도 최대 이벤트 구독
		GameEvents.Instance.ElementIntensityMaxed += _ =>
		{
			if (!Enabled) return;
			// 불 강도 증가
			_fireIntensity += 1.0f;
			// 불 사운드가 재생 중이 아니면 시작
			if (!Fire.Playing) Fire.Play();
		};
	}
}
