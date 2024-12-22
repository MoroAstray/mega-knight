using Godot;

public partial class HealthComponent : Node2D
{
    [Export]
    public float MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            Health = value;
            EmitSignal(SignalName.MaxHealthChanged, _maxHealth);
        }
    }
    private float _maxHealth;
    [Signal]
    public delegate void MaxHealthChangedEventHandler(float value);

    [Export]
    public float Health
    {
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            EmitSignal(SignalName.CurrentHealthChanged, _currentHealth);
            if (_currentHealth <= 0)
            {
                EmitSignal(SignalName.HealthDepleted);
            }
        }
    }
    private float _currentHealth;

    [Signal]
    public delegate void CurrentHealthChangedEventHandler(float value);
    [Signal]
    public delegate void HealthDepletedEventHandler();

    public void TakeDamage(float damage)
    {
        Health -= damage;
    }
}
