namespace Game.UI;

using Game.Autoload;

/// <summary>
/// 게임 점수를 표시하고 관리하는 UI 라벨 클래스
/// Main.tscn의 Hud/Top Right/Score에 연결된다.
/// Fireball이 좀비에게 피격될 때마다 점수를 증가시키고 애니메이션을 표시한다.
/// </summary>
public partial class Score : Label
{
	/// <summary>누적된 총 점수</summary>
	private int _totalScore;

	/// <summary>스케일 애니메이션의 피벗 오프셋 계산에 사용되는 벡터</summary>
	private Vector2 _pivot;

	/// <summary>
	/// 라벨이 준비되었을 때 호출되는 라이프사이클 메서드
	/// 점수 업데이트 이벤트 리스너를 등록한다.
	/// </summary>
	public override void _Ready()
	{
		// 라벨의 크기 기반 피벗 계산
		_pivot = Size / 2.0f;

		// GameEvents의 PlayerHit 신호 구독
		// Fireball이 좀비에게 피격될 때마다 호출됨
		GameEvents.Instance.PlayerHit += (_, _, _) =>
		{
			// 점수 증가
			_totalScore += 1;

			// 점수 텍스트 업데이트
			Text = $"Score: {_totalScore}";

			// 피벗 오프셋 조정 (오른쪽 정렬 애니메이션)
			_pivot.X = Size.X;
			PivotOffset = _pivot;

			// 점수 증가 시각 효과 애니메이션 시작
			var tween = CreateTween();

			// 스케일 애니메이션: 1.8배 → 1배로 축소
			// 기간: 0.25초, 선형 트랜지션
			tween.TweenProperty(
					this,
					Node2D.PropertyName.Scale.ToString(),
					Vector2.One,
					0.25f)
				.From(Vector2.One * 1.8f);  // 시작 크기: 1.8배
		};
	}
}
