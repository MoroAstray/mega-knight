using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SlimeBoss : Enemy
{
    [Export]
    public EnemySeekComponent EnemySeekComponent;
    [Export]
    public WanderComponent WanderComponent;
    [Export]
    public HealthComponent HealthComponent;
    [Export]
    public HurtboxComponent HurtboxComponent;
    [Export]
    public PackedScene SecondPhaseBoss;

    private AnimationPlayer _animationPlayer;
    private EnemyState _enemyState;
    private AnimatedSprite2D _animatedSprite;
    private Node2D _hitboxes;
    private float _jumpWidth = 50;
    private bool _moving = false;

    public enum EnemyState
    {
        Idle,
        Wandering,
        Chasing,
        Dying
    };

    public override void _Ready()
    {
        _enemyState = EnemyState.Idle;
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _hitboxes = GetNode<Node2D>("Hitboxes");

        HurtboxComponent.DamageTaken += TakeDamage;
        HealthComponent.HealthDepleted += OnHealthDepleted;
    }

    public override void _ExitTree()
    {
        HurtboxComponent.DamageTaken -= TakeDamage;
        HealthComponent.HealthDepleted -= OnHealthDepleted;
    }

    public override void _PhysicsProcess(double delta)
    {
        ApplyGravity(EnemyGravity, delta);
        switch (_enemyState)
        {
            case EnemyState.Idle:
                HandleIdleState(delta);
                break;
            case EnemyState.Wandering:
                HandleWanderingState(delta);
                break;
            case EnemyState.Chasing:
                HandleChasingState(delta);
                break;
            case EnemyState.Dying:
                HandleDyingState(delta);
                break;
        }
        MoveAndSlide();
    }

    private void ChangeState(EnemyState state)
    {
        var oldState = _enemyState;
        _enemyState = state;

        if (state == EnemyState.Dying)
        {
            GetNode("Hitboxes").QueueFree();
        } else if (state == EnemyState.Wandering)
        {
            _moving = false;
        }
    }

    public void HandleIdleState(double delta)
    {
        Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Friction * (float)delta) };

        SeekPlayer();
        RestartWanderTimerIfOver();
        _animatedSprite.Play("idle");
    }

    public void HandleWanderingState(double delta)
    {
        SeekPlayer();
        RestartWanderTimerIfOver();

        var wanderTargetPosition = WanderComponent.GetTargetPosition();
        var direction = GlobalPosition.DirectionTo(wanderTargetPosition);

        if (GlobalPosition.DistanceTo(wanderTargetPosition) <= WanderComponent.WANDER_EPSILON)
        {
            _animatedSprite.Play("idle");
            _enemyState = PickRandomState(new List<EnemyState> { EnemyState.Idle, EnemyState.Wandering, EnemyState.Wandering });
            WanderComponent.StartWanderTmer(GD.RandRange(1, 3));
            Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Friction * (float)delta) };
        }
        else
        {
            Velocity = Velocity with { X = Mathf.MoveToward(direction.X * Speed, 0, Friction * (float)delta) };
            if (!_moving)
            {
                _animationPlayer.Play("move");
                _moving = true;
            }
        }
        UpdateFacingDirection();
    }

    public void HandleChasingState(double delta)
    {
        var player = EnemySeekComponent.GetPlayerInRange();
        if (player == null)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        var direction = GlobalPosition.DirectionTo(player.GlobalPosition);
        Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, direction.X * Speed, Friction * (float)delta) };
        _animatedSprite.Play("move");
        UpdateFacingDirection();
    }

    public void HandleDyingState(double delta)
    {
        if (_animationPlayer.CurrentAnimation == "death")
            return;

        _animationPlayer.Play("death");
        Velocity = Vector2.Zero;
        GetNode<EventBus>("/root/EventBus").EmitSignal("BossFirstPhaseDefeated");
    }

    public void RestartWanderTimerIfOver()
    {
        if (WanderComponent.GetTimeLeft() == 0)
        {
            ChangeState(PickRandomState(new List<EnemyState> { EnemyState.Idle, EnemyState.Wandering, EnemyState.Wandering }));
            WanderComponent.StartWanderTmer(GD.RandRange(1, 3));
        }
    }

    public EnemyState PickRandomState(List<EnemyState> stateList)
    {
        // Order the list by new randomly created guids and return the first one
        return stateList.OrderBy(x => System.Guid.NewGuid()).FirstOrDefault();
    }

    public void SeekPlayer()
    {
        if (EnemySeekComponent.PlayerInRange())
        {
            ChangeState(EnemyState.Chasing);
        }
    }

    public override void TakeDamage(float damage)
    {
        if (_enemyState != EnemyState.Dying)
        {
            base.TakeDamage(damage);
            HealthComponent.TakeDamage(damage);
        }
    }

    private void UpdateFacingDirection()
    {
        var facingRight = Velocity.X > 0;
        var directionFactor = facingRight ? -1 : 1;
        _animatedSprite.FlipH = facingRight;
    }

    public void OnHealthDepleted()
    {
        if (_enemyState != EnemyState.Dying)
            ChangeState(EnemyState.Dying);
    }

    public void OnDeathAnimationFinished()
    {
        var boss = SecondPhaseBoss.Instantiate<Boss>();
        boss.GlobalPosition = GlobalPosition;
        GetParent().AddChild(boss);
        QueueFree();
    }
}
