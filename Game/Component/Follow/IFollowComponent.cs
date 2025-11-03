namespace Game.Component.Follow;

/// <summary>
/// 추적(Follow) 컴포넌트들의 공통 인터페이스
/// 엔티티가 특정 대상(마우스, 플레이어, 경로 등)을 따라가는 동작을 정의하는 컴포넌트들의 마커 인터페이스입니다.
/// 이 인터페이스를 구현하는 컴포넌트: FollowMouseComponent, FollowPlayerComponent, FollowPathComponent
/// Main.tscn의 엔티티들이 다양한 추적 방식을 가질 수 있도록 합니다.
/// </summary>
public interface IFollowComponent
{
}
