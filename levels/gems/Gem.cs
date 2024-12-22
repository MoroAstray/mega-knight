using Godot;

public partial class Gem : Area2D
{
    [Export]
    public GemType _gemType;
    [Export]
    public AudioStreamPlayer PickUpSFX;
    [Signal]
    public delegate void GemPickedUpEventHandler(GemType gem);
    public enum GemType
    {
        Ruby,
        Sapphire,
        Emerald,
        Diamond
    };

    private bool _pickedUp = false;
    private Tween _tween;

    public override void _Ready()
    {
        _tween = GetTree().CreateTween();
        _tween.SetLoops();

        MoveVertically();
    }

    public override void _ExitTree()
    {
        _tween.Kill();
    }

    private void MoveVertically()
    {
        _tween.TweenProperty(this, "position:y", Position.Y + 5, 1.0f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        _tween.TweenProperty(this, "position:y", Position.Y - 5, 1.0f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }

    public void OnBodyEntered(Node2D body)
    {
        if (_pickedUp || body is not Player player)
            return;

        _pickedUp = true;
        PickUpSFX.Play();
        GetNode<EventBus>("/root/EventBus").EmitSignal(SignalName.GemPickedUp, (int)_gemType);
        Visible = false;
    }

    public void OnAudioFinished()
    {
        QueueFree();
    }
}
