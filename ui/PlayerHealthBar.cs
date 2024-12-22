using Godot;
using System;

public partial class PlayerHealthBar : Control
{
    TextureProgressBar HealthBar;
    private float _initialSize;
    public override void _Ready()
    {
        HealthBar = GetNode<TextureProgressBar>("TextureProgressBar");
        _initialSize = HealthBar.Size.X;

        var eventBus = GetNode<EventBus>("/root/EventBus");
        eventBus.PlayerMaxHealthChanged += IncreaseMaxHealth;
        eventBus.PlayerTakeDamage += TakeDamage;
    }

    public override void _ExitTree()
    {
        var eventBus = GetNode<EventBus>("/root/EventBus");
        eventBus.HeartPickedUp += IncreaseMaxHealth;
        eventBus.PlayerTakeDamage -= TakeDamage;
    }

    public void TakeDamage(float damage)
    {
        HealthBar.Value = HealthBar.Value - damage;
    }

    public void IncreaseMaxHealth(int amount)
    {
        var size = HealthBar.Size;
        HealthBar.Size = size with { X = size.X + amount };
        HealthBar.MaxValue = HealthBar.MaxValue + amount;
        HealthBar.Value = HealthBar.MaxValue;
    }

    public void StartOver()
    {
        HealthBar.Value = 30;
        HealthBar.MaxValue = 30;
        HealthBar.Size = HealthBar.Size with { X = _initialSize };
    }
}
