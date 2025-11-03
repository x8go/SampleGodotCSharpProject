global using Godot;
global using GodotUtilities;
global using GodotUtilities.Logic;
global using System;

namespace Game.Autoload;

/// <summary>
/// 게임 전역 싱글톤 클래스
/// 게임 전체에서 접근 가능한 Global 인스턴스를 제공한다.
/// Main.tscn에서 설정된 카메라와 HUD에 접근할 수 있다.
/// </summary>
public partial class Global : Node
{
	/// <summary>Main.tscn의 Camera2D 노드 참조 - 게임 카메라에 접근하기 위해 사용</summary>
	public Camera2D Camera2D;

	/// <summary>Main.tscn의 Hud(UI 레이어) 노드 참조 - UI 요소들에 접근하기 위해 사용</summary>
	public CanvasLayer Hud;

	/// <summary>Global 클래스의 싱글톤 인스턴스 - 게임 어디서나 Global.Instance로 접근 가능</summary>
	public static Global Instance { get; private set; }

	/// <summary>
	/// 노드가 씬 트리에 들어올 때 호출되는 알림 메서드
	/// 싱글톤 인스턴스를 초기화한다.
	/// </summary>
	public override void _Notification(int what)
	{
		// NotificationEnterTree: 노드가 씬 트리에 진입할 때 발생하는 알림
		if (what == NotificationEnterTree) Instance = this;
	}
}
