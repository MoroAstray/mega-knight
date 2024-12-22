using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    [Export]
    public float Speed;
    [Export]
    public float Acceleration;
    [Export]
    public float Friction;
    [Export]
    public float JumpVelocity;
    [Export]
    public float EnemyGravity;
    [Export]
    public AnimationPlayer BlinkAnimationPlayer;
    public virtual void TakeDamage(float damage)
    {
        BlinkAnimationPlayer.Play("start");
    }

    protected void Jump()
    {
        Velocity = Velocity with { Y = JumpVelocity };
    }

    protected void ApplyGravity(float gravity, double delta)
    {
        // Add the gravity.
        if (!IsOnFloor())
        {
            Velocity = Velocity with { Y = Velocity.Y + gravity * (float)delta };
        }
    }
}
