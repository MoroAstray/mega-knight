using Godot;
using System;

public partial class HeartPickup : Area2D
{
    [Export]
    public AudioStreamPlayer PickUpSFX;
    [Export]
    public int AddedHealth = 25;

    private bool _pickedUp = false;
    public void OnBodyEntered(Node2D body)
    {
        if (_pickedUp || body is not Player player)
            return;

        _pickedUp = true;
        PickUpSFX.Play();
        GetNode<EventBus>("/root/EventBus").EmitSignal("HeartPickedUp", AddedHealth);
        Visible = false;
    }

    public void OnAudioFinished()
    {
        QueueFree();
    }
}
