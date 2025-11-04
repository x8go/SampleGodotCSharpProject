namespace Game;

using Game.Autoload;
using Game.Entity.Enemy;
using Game.Helpers;

/// <summary>
/// 메인 게임 씬을 관리하는 클래스
/// 좀비 생성, 카메라 설정, HUD 관리 등 게임의 핵심 로직을 담당한다.
/// </summary>
public partial class Main : Node2D
{
	/// <summary>처치한 좀비의 총 개수를 추적하는 변수</summary>
	private int _killedZombies;

	/// <summary>게임 월드에서 랜덤한 위치에 좀비를 생성하기 위한 점 생성기</summary>
	private PointGenerator _pointGenerator;

	/// <summary>난수 생성을 위한 Godot의 내장 난수 생성기</summary>
	private RandomNumberGenerator _random = new();

	/// <summary>게임 화면의 렉트(사각형 영역) - 현재는 사용되지 않음</summary>
	private Rect2 _rect;

	/// <summary>게임 화면의 크기(가로, 세로 픽셀)</summary>
	private Vector2 _screenSize;

	/// <summary>Main.tscn의 Camera2D 노드 연결 - [Node] 속성으로 자동 연결됨</summary>
	[Node]
	public Camera2D Camera2D;

	/// <summary>Main.tscn의 FireBall(플레이어) 노드 연결 - 마우스를 따라다니는 객체</summary>
	[Node]
	public Node2D FireBall;

	/// <summary>Main.tscn의 Hud(UI 레이어) 노드 연결 - UI 요소들이 포함됨</summary>
	[Node]
	public CanvasLayer Hud;

	/// <summary>생성할 좀비의 총 개수 - Inspector에서 수정 가능 (기본값: 500)</summary>
	[Export]
	public int TotalZombies = 500;

	/// <summary>Main.tscn의 Entities(게임 오브젝트) 노드 연결 - Y축 정렬이 활성화되어 있음</summary>
	[Node]
	public CanvasGroup Entities;

	/// <summary>좀비 씬 리소스 - Inspector에서 Zombie.tscn 파일을 할당함</summary>
	[Export]
	public PackedScene ZombieScene;

	/// <summary>
	/// 노드가 트리에 들어올 때 호출되는 라이프사이클 메서드
	/// 노드 와이어링(자동 노드 연결)을 수행한다.
	/// </summary>
	public override void _EnterTree()
	{
		// [Node] 속성으로 마킹된 필드들을 자동으로 탐색하고 연결
		this.WireNodes();
	}

	/// <summary>
	/// 노드가 준비되었을 때(씬 로드 완료) 호출되는 라이프사이클 메서드
	/// 게임의 초기 설정을 수행한다.
	/// </summary>
	public override void _Ready()
	{
		// 싱글톤 패턴으로 Global에 HUD와 카메라 참조 저장
		Global.Instance.Hud = Hud;
		Global.Instance.Camera2D = Camera2D;

		// 난수 시드 설정 (고정된 시드로 동일한 패턴 생성)
		_random.Seed = 1234L;

		// 게임 화면의 크기 가져오기
		_screenSize = GetViewportRect().Size;

		// 플레이어(FireBall)를 화면 중앙에 배치
		FireBall.GlobalPosition = _screenSize / 2;

		// 점 생성기 초기화
		// - 중심: 화면의 중앙
		// - 범위 반경: 500 픽셀
		// - 최소 거리: 200 픽셀 (점들 사이의 최소 거리)
		_pointGenerator = new PointGenerator(
			_screenSize / 2.0f,
			500,
			200);

		// 초기 좀비 생성
		_CreateZombies();

		// 좀비가 처치될 때마다 호출되는 이벤트 리스너 등록
		GameEvents.Instance.ZombieKilled += _ =>
		{
			_killedZombies++;
			// 1개 또는 2개마다 새로운 좀비 1마리 생성
			if (_killedZombies % _random.RandiRange(1, 2) == 0)
			{
				_CreateZombies(_pointGenerator.GeneratePoint());
			}
		};
	}

	/// <summary>
	/// TotalZombies 개수만큼 좀비를 생성하는 메서드
	/// PointGenerator를 사용하여 분산된 위치에 배치한다.
	/// </summary>
	private void _CreateZombies()
	{
		// PointGenerator에서 원하는 개수의 좀비 위치 점들을 생성
		var points = _pointGenerator.GeneratePoints(TotalZombies);

		// 생성된 각 위치에서 좀비를 인스턴스화
		foreach (var point in points)
		{
			_CreateZombies(point);
		}
	}

	/// <summary>
	/// 지정된 위치에 좀비 1마리를 생성하는 메서드
	/// </summary>
	/// <param name="point">좀비를 생성할 월드 좌표 위치</param>
	private void _CreateZombies(Vector2 point)
	{
		// ZombieScene을 씬에서 인스턴스화 (복제)
		var zombie = ZombieScene.Instantiate<Zombie>();

		// 생성된 좀비를 지정된 위치에 배치
		zombie.GlobalPosition = point;

		// 좀비의 크기를 0.15배에서 0.45배 사이의 난수로 조정 (시각적 다양성)
		zombie.Scale *= (float)GD.RandRange(0.15, 0.45);

		// Entities 컨테이너에 좀비를 추가하여 게임 월드에 배치
		// (Y축 정렬이 활성화되어 있으므로 Y 좌표에 따라 깊이감 표현)
		Entities.AddChild(zombie);
	}
}
