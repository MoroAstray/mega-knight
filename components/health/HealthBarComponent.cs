using Godot;
using System;

public partial class HealthBarComponent : Control
{
    [Export]
    public ProgressBar ProgressBar { get; set; }
    [Export]
    public HealthComponent HealthComponent { get; set; }

    public override void _Ready()
    {
        SetMaxHealth(HealthComponent.MaxHealth);
        HealthComponent.CurrentHealthChanged += SetHealth;
        HealthComponent.MaxHealthChanged += SetMaxHealth;
    }

    public override void _ExitTree()
    {
        HealthComponent.CurrentHealthChanged -= SetHealth;
        HealthComponent.MaxHealthChanged -= SetMaxHealth;
    }
    public void SetHealth(float health)
    {
        ProgressBar.Value = health;
    }

    public void SetMaxHealth(float max)
    {
        ProgressBar.MaxValue = max;
        ProgressBar.Value = max;
    }
}
