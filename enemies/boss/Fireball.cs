using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Fireball : Area2D
{
    [Export]
    public int AnimationNumber;
    [Export]
    public float Speed;
    [Export]
    public Vector2 Direction;

    private Timer _startTimer;
    private Timer _queueFreeTimer;
    private AnimatedSprite2D _sprite;
    private bool _moving = false;

    public override void _Ready()
    {
        _startTimer = GetNode<Timer>("StartTimer");
        _queueFreeTimer = GetNode<Timer>("QueueFreeTimer");
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _sprite.Animation = $"fireball{AnimationNumber}";
        _sprite.Play();
        UpdateFacingDirection();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_moving)
            Position += Direction * Speed * (float)delta;
    }

    private void UpdateFacingDirection()
    {
        if (Direction.X < 0)
        {
            Rotation = -Mathf.Pi / 2;
        } 
        else if (Direction.Y > 0)
        {
            Rotation = Mathf.Pi;
        } 
        else
        {
            Rotation = 0;
        }
    }

    public void OnStartTimerTimeout()
    {
        _moving = true;
    }

    public void OnQueueFreeTimerTimeout()
    {
        QueueFree();
    }
    
    public void OnBodyEntered(Node2D body)
    {
        QueueFree();
    }
}
