namespace Game.Manager;

/// <summary>
/// 게임 내 시각 효과(FX)를 관리하는 매니저 클래스 - 싱글톤 패턴으로 구현됩니다.
/// Main.tscn의 매니저 노드로 추가되어 화면 흔들림 등의 전역 시각 효과를 제공합니다.
/// 다른 스크립트에서 FxManager.ShakeScreen()을 호출하여 카메라 흔들림 효과를 적용할 수 있습니다.
/// </summary>
public partial class FxManager : Node
{
	/// <summary>
	/// FxManager의 전역 싱글톤 인스턴스
	/// 게임 전역에서 FxManager.Instance로 접근 가능합니다.
	/// </summary>
	public static FxManager Instance { get; private set; }

	/// <summary>
	/// 랜덤한 카메라 오프셋 생성을 위한 난수 생성기
	/// </summary>
	private readonly Random _random = new();

	/// <summary>
	/// 현재 화면 흔들림의 우선순위 (미사용 - 향후 확장용)
	/// </summary>
	private int _currentShakePriority;

	/// <summary>
	/// 화면을 흔드는 효과를 발생시킵니다. (정적 메서드)
	/// 게임의 어디서든 FxManager.ShakeScreen()으로 호출하여 화면 흔들림을 적용할 수 있습니다.
	/// </summary>
	/// <param name="shakeLength">흔들림 지속 시간(초)</param>
	/// <param name="shakePower">흔들림 강도(픽셀 단위)</param>
	public static void ShakeScreen(double shakeLength, float shakePower) =>
		Instance._ShakeScreen(shakeLength, shakePower);

	/// <summary>
	/// Godot 엔진의 알림 메시지를 처리합니다.
	/// 노드가 씬 트리에 진입할 때 싱글톤 인스턴스를 설정합니다.
	/// </summary>
	/// <param name="what">알림 타입</param>
	public override void _Notification(int what)
	{
		if (what != NotificationEnterTree) return;

		// 싱글톤 인스턴스 설정
		Instance = this;
	}

	/// <summary>
	/// min과 max 사이의 랜덤한 float 값을 생성합니다.
	/// </summary>
	/// <param name="min">최소값</param>
	/// <param name="max">최대값</param>
	/// <returns>min과 max 사이의 랜덤 값</returns>
	private float _RandRange(float min, float max) =>
		(float)_random.NextDouble() * (max - min) + min;

	/// <summary>
	/// 카메라를 랜덤한 위치로 이동시킵니다.
	/// Tween의 각 프레임마다 호출되어 흔들림 효과를 생성합니다.
	/// </summary>
	/// <param name="move">카메라 오프셋의 최대 범위 (X, Y 픽셀)</param>
	private void _MoveCamera(Vector2 move)
	{
		// "Camera" 그룹에서 카메라 찾기
		var camera = GetTree().GetFirstNodeInGroup<Camera2D>();
		// 카메라를 -move ~ +move 범위 내의 랜덤한 위치로 이동
		camera.Offset = new Vector2(_RandRange(-move.X, move.X), _RandRange(-move.Y, move.Y));
	}

	/// <summary>
	/// 화면을 흔드는 효과를 실제로 구현하는 내부 메서드입니다.
	/// Tween을 사용하여 흔들림 강도를 시간에 따라 점차 감소시킵니다.
	/// </summary>
	/// <param name="shakeLength">흔들림 지속 시간(초)</param>
	/// <param name="shakePower">초기 흔들림 강도(픽셀 단위)</param>
	private void _ShakeScreen(double shakeLength, float shakePower)
	{
		// 새로운 Tween 생성
		var tweenShake = CreateTween();
		// shakePower에서 0으로 점차 감소하는 Tween 설정
		tweenShake.TweenMethod(
				Callable.From<Vector2>(Instance._MoveCamera),
				new Vector2(shakePower, shakePower), // 시작 강도
				new Vector2(0, 0), // 끝 강도 (0 = 원래 위치)
				shakeLength) // 지속 시간
			.SetTrans(Tween.TransitionType.Sine) // 사인파 트랜지션
			.SetEase(Tween.EaseType.Out); // Out 이징 - 점차 느려짐
	}

	/// <summary>
	/// 현재 흔들림 우선순위를 리셋합니다. (미사용 - 향후 확장용)
	/// </summary>
	private void _resetCurrentShakePriority() => _currentShakePriority = 0;
}
