using Godot;

public partial class EndMenu : Control
{
    public void OnQuitPressed()
    {
        GetTree().Paused = false;
        Visible = false;
        GetNode<EventBus>("/root/EventBus").EmitSignal("QuitLevel");
    }

    public void OnRestartPressed()
    {
        GetTree().Paused = false;
        GetNode<EventBus>("/root/EventBus").EmitSignal("RestartLevel");
        Visible = false;
    }

    public void SetLabelText(string text)
    {
        GetNode<Label>("Label").Text = text;
    }
}
