using Godot;
using System;
using static Gem;

public partial class EventBus : Node
{
	[Signal]
	public delegate void HeartPickedUpEventHandler(int amount);
	[Signal]
	public delegate void StartLevelEventHandler();
	[Signal]
	public delegate void PlayerDiedEventHandler();
    [Signal]
    public delegate void PauseGameEventHandler();
	[Signal]
	public delegate void QuitLevelEventHandler();
	[Signal]
	public delegate void ContinueTitleScreenEventHandler();
	[Signal]
	public delegate void ExitLevelSelectionEventHandler();
	[Signal]
	public delegate void RestartLevelEventHandler();
    [Signal]
    public delegate void GameFinishedEventHandler();
	[Signal]
	public delegate void GemPickedUpEventHandler(GemType gem);
	[Signal]
	public delegate void PlayerTakeDamageEventHandler(float damage);
    [Signal]
    public delegate void PlayerMaxHealthChangedEventHandler(int amount);
	[Signal]
	public delegate void BossFirstPhaseDefeatedEventHandler();
}
