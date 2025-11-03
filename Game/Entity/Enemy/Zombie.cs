namespace Game.Entity.Enemy;

using Game.Autoload;
using Game.Component;
using Game.Component.Follow;
using Game.Extension;

/// <summary>
/// 게임의 적 유닛인 좀비 클래스
/// Main.cs에서 대량으로 생성되며 플레이어(Fireball)를 따라가거나 경로를 따라간다.
/// 상태 머신을 사용하여 유휴(Idle) 상태와 걷기(Walk) 상태를 관리한다.
/// </summary>
public partial class Zombie : BaseEnemy
{
	/// <summary>좀비의 애니메이션을 관리하는 Godot 노드 - 걷기 애니메이션 재생에 사용</summary>
	[Node]
	public AnimatedSprite2D AnimatedSprite2D;

	/// <summary>좀비의 충돌 영역을 정의하는 노드 - Zombie.tscn에서 설정됨</summary>
	[Node]
	public CollisionShape2D CollisionShape2D;

	/// <summary>좀비의 체력 관리 컴포넌트 - 데미지 적용 및 체력 추적</summary>
	[Node]
	public HealthComponent HealthComponent;

	/// <summary>좀비의 이동 속도 및 방향을 관리하는 컴포넌트</summary>
	[Node]
	public VelocityComponent VelocityComponent;

	/// <summary>좀비가 바라보는 방향(좌/우)을 관리하는 컴포넌트</summary>
	[Node]
	public FacingComponent FacingComponent;

	/// <summary>좀비의 시각적 표현(그래픽)을 포함하는 노드 - 색상 변경으로 체력 표시</summary>
	[Node]
	public Node2D Visuals;

	/// <summary>좀비의 상태를 관리하는 상태 머신 (Idle, Walk 상태 전환)</summary>
	private DelegateStateMachine _stateMachine = new();

	/// <summary>이전 프레임의 체력값 - 체력 변화 감지에 사용</summary>
	private float _oldHealth;

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
	/// 상태 머신을 초기화하고 스폰 이벤트를 발행한다.
	/// </summary>
	public override void _Ready()
	{
		// 상태 머신에 Idle 상태와 Walk 상태를 등록
		// AddStates(상태 메서드, 상태 진입 메서드)
		_stateMachine.AddStates(StateIdle, EnterStateIdle);
		_stateMachine.AddStates(StateWalk, EnterStateWalk);

		// 초기 상태를 Idle로 설정
		_stateMachine.SetInitialState(StateIdle);

		// 이 좀비가 스폰되었음을 알리는 이벤트 발행
		GameEvents.EmitSpawnZombie(this);
	}

	/// <summary>
	/// 매 물리 프레임마다 호출되는 라이프사이클 메서드
	/// 상태 머신을 업데이트하여 현재 상태의 로직을 실행한다.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		// 상태 머신 업데이트 - 현재 상태의 메서드(StateIdle 또는 StateWalk) 실행
		_stateMachine.Update();
	}

	/// <summary>
	/// 좀비의 체력을 확인하고 필요한 처리를 수행하는 메서드
	/// 체력이 0 이하면 죽은 것으로 처리하고 폭발 및 점수 생성
	/// </summary>
	private void _CheckHealth()
	{
		if (HealthComponent != null)
		{
			// 체력이 0 이하이고 아직 죽음 처리되지 않았으면 처리
			if (HealthComponent.Health <= 0.0f && !IsDead)
			{
				// 죽은 상태로 표시
				IsDead = true;

				// 좀비가 죽을 때 폭발 이펙트 추가하고 노드 제거 예약
				this.AddResourceAndQueueFree<ExplosionComponent>();

				// 좀비가 죽을 때 플레이어(Fireball)에게 끌려갈 점수 객체(ScoreAttractorComponent) 생성
				var scoreAttractorComponent = this.InstantiateFromResources<ScoreAttractorComponent>();

				// 점수 객체가 플레이어(Fireball)에게 끌려가도록 설정
				scoreAttractorComponent.AttractorNode = GetTree().GetFirstNodeInGroup<Fireball>();

				// 점수 객체가 플레이어에게 도달하면 삭제되도록 설정
				this.AddNodeToQueueFreeComponent(scoreAttractorComponent);

				// 점수 객체를 씬에 추가
				this.AddChildDeferred(scoreAttractorComponent);

				// 좀비가 처치되었음을 알리는 이벤트 발행
				// Main.cs에서 구독하여 처치 카운트 증가 및 새 좀비 생성
				GameEvents.EmitZombieKilled(this);
			}
			// 체력이 변화했으면 시각적 표현 업데이트 (색상 변경)
			else if (Math.Abs(HealthComponent.Health - _oldHealth) > 0.001f)
			{
				_oldHealth = HealthComponent.Health;

				// HealthGradient를 사용하여 체력에 따라 좀비의 색상 변경
				// 체력 100% = 흰색, 체력 0% = 어두운 색
				Visuals.Modulate = HealthComponent.HealthGradient.Sample(1.0f - _oldHealth);
			}
		}
	}

	/// <summary>
	/// 좀비의 이동 속도를 확인하고 상태를 변경하는 메서드
	/// 속도가 0.1 이상이면 Walk 상태, 미만이면 Idle 상태로 변경
	/// </summary>
	private void _CheckSpeed()
	{
		var speed = VelocityComponent.Speed;

		// 속도가 매우 낮으면 Idle 상태로 변경
		if (speed <= 0.1f)
		{
			if (_stateMachine.GetCurrentState() != StateIdle)
			{
				_stateMachine.ChangeState(StateIdle);
			}
		}
		// 속도가 있으면 Walk 상태로 변경
		else
		{
			if (_stateMachine.GetCurrentState() != StateWalk)
			{
				_stateMachine.ChangeState(StateWalk);
			}
		}
	}

	/// <summary>
	/// Walk 상태에서 Idle 상태로 진입할 때 호출되는 콜백
	/// 애니메이션 재생을 중지한다.
	/// </summary>
	private void EnterStateIdle() => AnimatedSprite2D.Pause();

	/// <summary>
	/// Idle 상태에서 실행되는 메서드 (매 프레임마다 호출)
	/// 좀비가 정지한 상태에서의 이동, 체력 확인, 속도 체크를 수행한다.
	/// </summary>
	private void StateIdle()
	{
		// 중력 및 충돌을 고려하여 좀비를 이동시킴
		VelocityComponent.MoveAndSlide(this);

		// 체력 확인 (체력이 0이면 죽음 처리)
		_CheckHealth();

		// 속도 확인 (속도 변화에 따라 상태 전환)
		_CheckSpeed();
	}

	/// <summary>
	/// Idle 상태에서 Walk 상태로 진입할 때 호출되는 콜백
	/// 걷기 애니메이션을 재생한다.
	/// </summary>
	private void EnterStateWalk() => AnimatedSprite2D.Play("walk");

	/// <summary>
	/// Walk 상태에서 실행되는 메서드 (매 프레임마다 호출)
	/// 좀비가 이동 중인 상태에서의 이동, 체력 확인, 속도 체크를 수행한다.
	/// </summary>
	private void StateWalk()
	{
		// 중력 및 충돌을 고려하여 좀비를 이동시킴
		VelocityComponent.MoveAndSlide(this);

		// 체력 확인 (체력이 0이면 죽음 처리)
		_CheckHealth();

		// 속도 확인 (속도 변화에 따라 상태 전환)
		_CheckSpeed();
	}
}
