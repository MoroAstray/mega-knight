using Godot;
using System;

public partial class PauseMenu : Control
{
	public void OnResumeButtonPressed()
    {
        GetTree().Paused = false;
        Visible = false;
	}

	public void OnQuitButtonPressed()
    {
        GetTree().Paused = false;
        Visible = false;
        GetNode<EventBus>("/root/EventBus").EmitSignal("QuitLevel");
    }
}
