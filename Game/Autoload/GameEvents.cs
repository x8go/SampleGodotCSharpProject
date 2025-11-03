namespace Game.Autoload;

using Game.Component.Element;
using Game.Entity.Enemy;

/// <summary>
/// 게임 이벤트 시스템을 관리하는 싱글톤 클래스
/// Godot의 Signal 시스템을 활용하여 게임의 주요 이벤트들을 전역으로 발행한다.
/// 좀비 처치, 충돌, 원소 강도 변화 등의 게임 이벤트를 다른 시스템에서 구독할 수 있다.
/// </summary>
public partial class GameEvents : Node
{
	/// <summary>GameEvents 싱글톤 인스턴스 - 게임 어디서나 GameEvents.Instance로 접근 가능</summary>
	public static GameEvents Instance { get; private set; }

	/// <summary>
	/// 충돌 이벤트 신호
	/// Main.tscn의 Fireball이 물체와 충돌할 때 발생
	/// </summary>
	[Signal]
	public delegate void CollisionEventHandler(KinematicCollision2D collision2D);

	/// <summary>
	/// 원소 강도 고갈 이벤트 신호
	/// 원소(불 등)의 강도가 완전히 소진되었을 때 발생
	/// </summary>
	[Signal]
	public delegate void ElementIntensityDepletedEventHandler(ElementComponent element);

	/// <summary>
	/// 원소 강도 최대화 이벤트 신호
	/// 원소의 강도가 최대치에 도달했을 때 발생
	/// </summary>
	[Signal]
	public delegate void ElementIntensityMaxedEventHandler(ElementComponent element);

	/// <summary>
	/// 좀비 처치 이벤트 신호
	/// Main.cs에서 좀비가 제거될 때마다 발생하여 새 좀비 생성을 트리거함
	/// </summary>
	[Signal]
	public delegate void ZombieKilledEventHandler(Zombie zombie);

	/// <summary>
	/// 좀비 스폰 이벤트 신호
	/// 새로운 좀비가 생성되었을 때 발생
	/// </summary>
	[Signal]
	public delegate void ZombieSpawnedEventHandler(Zombie zombie);

	/// <summary>
	/// 플레이어 피격 이벤트 신호
	/// Fireball이 좀비에게 공격받았을 때 발생
	/// </summary>
	[Signal]
	public delegate void PlayerHitEventHandler(Node2D player, float angle, Vector2 direction);

	/// <summary>
	/// 원소 강도 최대화 이벤트를 발행하는 정적 메서드
	/// 원소(불 등)의 강도가 최대치에 도달했을 때 호출된다.
	/// </summary>
	/// <param name="element">이벤트를 발생시킨 원소 컴포넌트</param>
	public static void EmitElementIntensityMaxed(ElementComponent element) =>
		Instance.EmitSignal(SignalName.ElementIntensityMaxed, element);

	/// <summary>
	/// 원소 강도 고갈 이벤트를 발행하는 정적 메서드
	/// 원소의 강도가 완전히 소진되었을 때 호출된다.
	/// </summary>
	/// <param name="element">이벤트를 발생시킨 원소 컴포넌트</param>
	public static void EmitElementIntensityDepleted(ElementComponent element) =>
		Instance.EmitSignal(SignalName.ElementIntensityDepleted, element);

	/// <summary>
	/// 좀비 처치 이벤트를 발행하는 정적 메서드
	/// Main.cs에서 좀비가 제거될 때 호출되어 처치 카운트를 증가시키고 새 좀비를 생성한다.
	/// </summary>
	/// <param name="zombie">처치된 좀비 객체</param>
	public static void EmitZombieKilled(Zombie zombie) =>
		Instance.EmitSignal(SignalName.ZombieKilled, zombie);

	/// <summary>
	/// 충돌 이벤트를 발행하는 정적 메서드
	/// Fireball과 다른 오브젝트(좀비)가 충돌할 때 호출된다.
	/// </summary>
	/// <param name="collision2D">충돌 정보를 담은 KinematicCollision2D 객체</param>
	public static void EmitCollision(KinematicCollision2D collision2D) =>
		Instance.EmitSignal(SignalName.Collision, collision2D);

	/// <summary>
	/// 좀비 스폰 이벤트를 발행하는 정적 메서드
	/// Main.cs에서 새로운 좀비가 생성될 때 호출된다.
	/// </summary>
	/// <param name="zombie">스폰된 좀비 객체</param>
	public static void EmitSpawnZombie(Zombie zombie) =>
		Instance.EmitSignal(SignalName.ZombieSpawned, zombie);

	/// <summary>
	/// 플레이어 피격 이벤트를 발행하는 정적 메서드
	/// Fireball(플레이어)이 좀비에게 공격받았을 때 호출된다.
	/// </summary>
	/// <param name="player">피격된 플레이어(Fireball) 객체</param>
	/// <param name="angle">공격 방향의 각도</param>
	/// <param name="direction">공격 방향 벡터</param>
	public static void EmitPlayerHit(Node2D player, float angle, Vector2 direction) =>
		Instance.EmitSignal(SignalName.PlayerHit, player, angle, direction);

	/// <summary>
	/// 노드가 씬 트리에 들어올 때 호출되는 알림 메서드
	/// 싱글톤 인스턴스를 초기화한다.
	/// </summary>
	public override void _Notification(int what)
	{
		// NotificationEnterTree: 노드가 씬 트리에 진입할 때 발생하는 알림
		if (what != NotificationEnterTree) return;

		// 싱글톤 인스턴스 할당
		Instance = this;
	}
}
