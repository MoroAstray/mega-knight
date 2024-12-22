using Godot;

public partial class LevelSelection : Control
{
    public void OnLevelPressed()
    {
        GetNode<EventBus>("/root/EventBus").EmitSignal("StartLevel");
        Visible = false;
    }

    public void OnBackPressed()
    {
        GetNode<EventBus>("/root/EventBus").EmitSignal("ExitLevelSelection");
        Visible = false;
    }
}
