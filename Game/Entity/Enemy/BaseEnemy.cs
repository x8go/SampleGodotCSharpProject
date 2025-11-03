namespace Game.Entity.Enemy;

/// <summary>
/// 모든 적(Enemy) 클래스의 기본 클래스
/// 현재는 좀비(Zombie)만 상속받고 있다.
/// 적의 죽음 상태를 추적하는 기본 기능을 제공한다.
/// </summary>
public partial class BaseEnemy : CharacterBody2D
{
	/// <summary>
	/// 적의 죽음 상태를 나타내는 플래그
	/// true이면 죽은 상태로 더 이상 처리되지 않음
	/// </summary>
	public bool IsDead;
}
