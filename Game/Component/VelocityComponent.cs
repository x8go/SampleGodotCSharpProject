namespace Game.Component;

using Game.Autoload;

/// <summary>
/// 이동 및 속도 관리 컴포넌트
/// 게임 오브젝트의 이동, 중력, 충돌 처리를 담당한다.
/// Fireball과 Zombie 모두 이 컴포넌트를 사용한다.
/// Main.tscn의 ZombieFollowPath도 이 컴포넌트를 포함한다(JustMove=true).
/// </summary>
public partial class VelocityComponent : BaseComponent
{
	/// <summary>충돌 감지 신호 - 오브젝트가 다른 오브젝트와 충돌할 때 발생</summary>
	[Signal]
	public delegate void CollidedEventHandler(KinematicCollision2D collision2D);

	/// <summary>
	/// 충돌 도형 노드
	/// Inspector에서 할당 가능하며, 충돌 감지에 사용됨
	/// </summary>
	[Export]
	public CollisionShape2D CollisionShape2D;

	/// <summary>
	/// 연속 물리 처리를 할 PhysicsBody2D 노드
	/// 중력이 적용되는 경우 사용됨
	/// </summary>
	[Export]
	public PhysicsBody2D ContinuousProcess;

	/// <summary>
	/// 중력 벡터
	/// Inspector에서 조정 가능 (기본값: (0, 0) - 중력 없음)
	/// Fireball: 중력 없음, Zombie: 아래쪽 중력 적용
	/// </summary>
	[Export]
	public Vector2 Gravity = new(0f, 0f);

	/// <summary>
	/// 현재 속도 벡터
	/// Inspector에서 초기값 설정 가능 (기본값: (0, 0))
	/// 매 프레임마다 MoveAndCollide 또는 MoveAndSlide에서 업데이트됨
	/// </summary>
	[Export]
	public Vector2 Velocity = Vector2.Zero;

	/// <summary>
	/// 진정한 물리 시뮬레이션을 하지 않고 위치만 변경할지 여부
	/// Inspector에서 조정 가능 (기본값: false)
	/// Main.tscn의 ZombieFollowPath: true (경로 따라가기용)
	/// </summary>
	[Export]
	public bool JustMove;

	/// <summary>
	/// 중력의 영향을 받고 있는지 여부 (낙하 상태)
	/// Gravity가 0이 아니면 true
	/// </summary>
	public bool Falling { get; private set; }

	/// <summary>
	/// 현재 이동 속도 (크기)
	/// 전 프레임의 위치와 현재 위치의 거리 제곱
	/// Zombie의 상태 전환(Idle/Walk)에 사용됨
	/// </summary>
	public float Speed { get; private set; }

	/// <summary>마지막 프레임의 위치 - Speed 계산에 사용</summary>
	private Vector2 _lastPosition = Vector2.Zero;

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
	/// 물리 처리를 기본적으로 비활성화한다.
	/// </summary>
	public override void _Ready()
	{
		// 기본적으로 물리 처리 비활성화 (필요할 때만 활성화)
		SetPhysicsProcess(false);
	}

	/// <summary>
	/// 충돌 감지를 활성화/비활성화하는 메서드
	/// </summary>
	/// <param name="flag">true면 비활성화, false면 활성화</param>
	public void DisableCollisionCheck(bool flag) =>
		CollisionShape2D?.CallDeferred(CollisionShape2D.MethodName.SetDisabled, flag);

	/// <summary>
	/// 물리 충돌을 감지하면서 오브젝트를 이동시키는 메서드
	/// Fireball.cs에서 _PhysicsProcess에서 호출된다.
	/// </summary>
	/// <param name="node">이동시킬 PhysicsBody2D 노드</param>
	/// <param name="delta">프레임 델타 시간</param>
	/// <returns>충돌 정보 (충돌이 없으면 null)</returns>
	public KinematicCollision2D MoveAndCollide(PhysicsBody2D node, double delta)
	{
		// 컴포넌트가 비활성화되면 아무것도 하지 않음
		if (!Enabled) return null;

		// 중력을 속도에 적용
		Velocity += Gravity;

		// 속도 계산과 이동을 함께 처리
		var collision2D = _CalculateSpeed(() =>
		{
			if (JustMove)
			{
				// JustMove가 true면 물리 없이 위치만 변경
				node.GlobalPosition += Velocity * (float)delta;
				return null;
			}
			// 진정한 물리 충돌 처리
			return node.MoveAndCollide(Velocity * (float)delta);
		});

		// 충돌이 없으면 반환
		if (collision2D == null) return null;

		// 충돌 신호 발행
		_EmitCollision(collision2D);

		return collision2D;
	}

	/// <summary>
	/// 슬라이딩 기반 물리 이동을 수행하는 메서드
	/// Zombie.cs에서 상태 메서드에서 호출된다.
	/// CharacterBody2D 전용 메서드로, 벽을 따라 슬라이딩하는 효과를 제공한다.
	/// </summary>
	/// <param name="node">이동시킬 CharacterBody2D 노드</param>
	public void MoveAndSlide(CharacterBody2D node)
	{
		// 컴포넌트가 비활성화되면 아무것도 하지 않음
		if (!Enabled) return;

		// 중력을 속도에 적용
		Velocity += Gravity;

		// node.Velocity 설정 (CharacterBody2D용)
		node.Velocity = Velocity;

		// 속도 계산과 이동을 함께 처리
		var collided = _CalculateSpeed(() =>
		{
			if (JustMove)
			{
				// JustMove가 true면 물리 없이 위치만 변경
				node.GlobalPosition += Velocity * (float)GetPhysicsProcessDeltaTime();
				return false;
			}
			// Godot 4.x의 슬라이딩 물리 이동
			return node.MoveAndSlide();
		});

		// 충돌이 없으면 반환
		if (!collided) return;

		// 마지막 슬라이드 충돌 정보 가져오기
		var collision2D = node.GetLastSlideCollision();

		// 충돌 신호 발행
		_EmitCollision(collision2D);
	}

	/// <summary>
	/// 물리 처리 활성화/비활성화 메서드
	/// 중력 적용이 필요한 경우에만 활성화한다.
	/// </summary>
	/// <param name="flag">true면 활성화, false면 비활성화</param>
	public void EnablePhysics(bool flag)
	{
		SetPhysicsProcess(flag);

		// 물리 처리 비활성화 시 낙하 상태 초기화
		if (flag == false)
		{
			Falling = false;
		}
	}

	/// <summary>
	/// 중력을 적용하여 오브젝트가 자유낙하하도록 설정하는 메서드
	/// </summary>
	/// <param name="node">중력을 적용할 Node2D</param>
	/// <param name="gravity">적용할 중력 벡터</param>
	public void ApplyGravity(Node2D node, Vector2 gravity)
	{
		// 속도 초기화
		Velocity = Vector2.Zero;

		// 중력 설정
		Gravity = gravity;

		// 연속 처리할 노드 설정
		ContinuousProcess = node as PhysicsBody2D;

		// 물리 처리 활성화
		SetPhysicsProcess(true);
	}

	/// <summary>
	/// 충돌 신호를 발행하는 내부 메서드
	/// 로컬 Collided 신호와 글로벌 GameEvents 신호 모두 발행한다.
	/// </summary>
	/// <param name="collision2D">발생한 충돌 정보</param>
	private void _EmitCollision(KinematicCollision2D collision2D)
	{
		// 이 컴포넌트의 로컬 신호 발행
		EmitSignal(SignalName.Collided, collision2D);

		// 전역 GameEvents 신호 발행
		GameEvents.EmitCollision(collision2D);
	}

	/// <summary>
	/// 현재 속도(크기)를 계산하는 메서드
	/// 마지막 프레임 위치와 현재 위치의 거리로 속도 계산
	/// </summary>
	private void _UpdateSpeed()
	{
		// 속도 = (이전 위치 - 현재 위치)의 크기의 제곱
		Speed = (_lastPosition - GlobalPosition).LengthSquared();

		// 마지막 위치 업데이트
		_lastPosition = GlobalPosition;
	}

	/// <summary>
	/// 속도 계산을 포함하는 이동 처리 래퍼 메서드 (KinematicCollision2D 반환)
	/// </summary>
	/// <param name="action">이동을 수행할 액션</param>
	/// <returns>충돌 정보</returns>
	private KinematicCollision2D _CalculateSpeed(Func<KinematicCollision2D> action)
	{
		// 액션 실행
		var result = action.Invoke();

		// 속도 업데이트
		_UpdateSpeed();

		return result;
	}

	/// <summary>
	/// 속도 계산을 포함하는 이동 처리 래퍼 메서드 (bool 반환)
	/// </summary>
	/// <param name="action">이동을 수행할 액션</param>
	/// <returns>충돌 여부</returns>
	private bool _CalculateSpeed(Func<bool> action)
	{
		// 액션 실행
		var result = action.Invoke();

		// 속도 업데이트
		_UpdateSpeed();

		return result;
	}

	/// <summary>
	/// 매 물리 프레임마다 호출되는 라이프사이클 메서드
	/// 중력이 설정된 경우 연속 이동 처리를 수행한다.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		// 컴포넌트가 비활성화되면 아무것도 하지 않음
		if (!Enabled) return;

		// 연속 처리할 노드가 있으면 이동 처리
		MoveAndCollide(ContinuousProcess, delta);

		// Falling 플래그 업데이트 (중력이 0이 아니면 낙하 중)
		Falling = Gravity.LengthSquared() > 0.0f;
	}
}
