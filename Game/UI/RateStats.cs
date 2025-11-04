namespace Game.UI;

using Game.Autoload;

/// <summary>
/// 플레이어 피격률 통계 UI 클래스
/// Main.tscn의 UI 레이어에 추가되어 초당 피격 횟수와 최대 피격 횟수를 실시간으로 표시합니다.
/// GameEvents의 PlayerHit 이벤트를 구독하여 피격 통계를 추적합니다.
/// </summary>
public partial class RateStats : Control
{
	/// <summary>
	/// 1초마다 피격 통계를 갱신하기 위한 타이머
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// </summary>
	[Node]
	public Timer Timer;

	/// <summary>
	/// 초당 피격 횟수를 표시하는 레이블
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// </summary>
	[Node]
	public Label HitRate;

	/// <summary>
	/// 최대 피격 횟수를 표시하는 레이블
	/// Inspector에서 [Node] 속성으로 자동 연결됩니다.
	/// </summary>
	[Node]
	public Label MaxRate;

	/// <summary>
	/// 현재 초 동안의 피격 횟수 카운터
	/// 타이머가 만료되면 _rate로 이동되고 0으로 리셋됩니다.
	/// </summary>
	private int _counter;

	/// <summary>
	/// 마지막 초의 피격 횟수
	/// </summary>
	private int _rate;

	/// <summary>
	/// 게임 시작 이후 기록된 최대 피격 횟수 (1초 기준)
	/// </summary>
	private int _maxRate;

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
	/// 타이머 이벤트와 게임 이벤트를 구독하여 통계를 추적합니다.
	/// </summary>
	public override void _Ready()
	{
		// 타이머 타임아웃 이벤트 구독 (1초마다 호출)
		Timer.Timeout += () =>
		{
			// 현재 카운터를 레이트로 저장
			_rate = _counter;
			// 카운터 리셋
			_counter = 0;
			// UI 업데이트 - 초당 피격 횟수 표시
			HitRate.Text = $"Hits: {_rate:D0}/s";
		};

		// 플레이어 피격 이벤트 구독
		GameEvents.Instance.PlayerHit += (_, _, _) =>
		{
			// 피격 횟수 증가
			_counter += 1;
			// 최대 피격 횟수 갱신 확인
			if (_maxRate <= _counter)
			{
				_maxRate = _counter;
				// UI 업데이트 - 최대 피격 횟수 표시
				MaxRate.Text = $"Max: {_maxRate:D0}";
			}
		};
	}
}
