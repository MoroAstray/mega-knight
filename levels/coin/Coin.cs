using Godot;
using System;

public partial class Coin : Area2D
{
	private AudioStreamPlayer _pickupSFX;
    private bool _pickedUp = false;

    public override void _Ready()
    {
        _pickupSFX = GetNode<AudioStreamPlayer>("PickupSFX");
        _pickupSFX.StreamPaused = false;
    }

    public void OnBodyEntered(Node2D body)
    {
        if (_pickedUp || body is not Player player)
            return;

        _pickedUp = true;
        _pickupSFX.Play();
        PlayerStats.Instance.PickedUpCoins++;
        Visible = false;
    }

    public void OnAudioFinished()
    {
        QueueFree();
    }
}
