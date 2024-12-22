using Godot;
using System;

public partial class TitleScreen : Control
{
    public void OnPlayPressed()
    {
        
        GetNode<EventBus>("/root/EventBus").EmitSignal("ContinueTitleScreen");
        Visible = false;
    }
    public void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
