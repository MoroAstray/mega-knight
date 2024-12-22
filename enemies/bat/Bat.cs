using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static EvilSpirit;

public partial class Bat : Enemy
{
    [Export]
    public EnemySeekComponent EnemySeekComponent;
    [Export]
    public EnemySeekComponent AttackRange;
    [Export]
    public WanderComponent WanderComponent;
    [Export]
    public HealthComponent HealthComponent;
    [Export]
    public HurtboxComponent HurtboxComponent;

    private AnimationPlayer _animationPlayer;
    private EnemyState _enemyState;
    private AnimatedSprite2D _animatedSprite;
    private Node2D _hitboxes;
    public enum EnemyState
    {
        Sleeping,
        Idle,
        Wandering,
        Chasing,
        Attacking,
        Dying
    }

    public override void _Ready()
    {
        _enemyState = EnemyState.Sleeping;
        _hitboxes = GetNode<Node2D>("Hitboxes");
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

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
        switch (_enemyState)
        {
            case EnemyState.Sleeping:
                HandleSleepingState(delta);
                break;
            case EnemyState.Idle:
                HandleIdleState(delta);
                break;
            case EnemyState.Wandering:
                HandleWanderingState(delta);
                break;
            case EnemyState.Chasing:
                HandleChasingState(delta);
                break;
            case EnemyState.Attacking:
                HandleAttackingState(delta);
                break;
            case EnemyState.Dying:
                HandleDyingState(delta);
                break;
        }
        MoveAndSlide();
    }

    public void ChangeState(EnemyState state)
    {
        var oldState = _enemyState;
        _enemyState = state;

        if (state == EnemyState.Dying)
        {
            GetNode("Hitboxes").QueueFree();
        }
    }

    public void HandleSleepingState(double delta)
    {
        _animatedSprite.Play("sleep");
        SeekPlayer();
    }

    public void HandleIdleState(double delta)
    {
        Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);

        SeekPlayer();
        RestartWanderTimerIfOver();
        _animatedSprite.Play("idle");
    }

    public void HandleWanderingState(double delta)
    {
        //SeekPlayer();
        RestartWanderTimerIfOver();

        var wanderTargetPosition = WanderComponent.GetTargetPosition();
        var direction = GlobalPosition.DirectionTo(wanderTargetPosition);

        if (GlobalPosition.DistanceTo(wanderTargetPosition) <= WanderComponent.WANDER_EPSILON)
        {
            _animatedSprite.Play("idle");
            _enemyState = PickRandomState(new List<EnemyState> { EnemyState.Idle, EnemyState.Wandering, EnemyState.Wandering });
            WanderComponent.StartWanderTmer(GD.RandRange(2, 4));
            Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
        }
        else
        {
            Velocity = Velocity.MoveToward(direction * Speed, Acceleration * (float)delta);
            _animatedSprite.Play("run");
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

        var direction = GlobalPosition.DirectionTo(player.GetTargetPosition());
        Velocity = Velocity.MoveToward(direction * Speed, Acceleration * (float)delta);
        _animatedSprite.Play("run");
        if (AttackRange.PlayerInRange())
        {
            ChangeState(EnemyState.Attacking);
        }
        UpdateFacingDirection();
    }

    public void HandleAttackingState(double delta)
    {
        Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);

        if (_animationPlayer.CurrentAnimation.Contains("attack"))
            return;

        if (!AttackRange.PlayerInRange())
        {
            ChangeState(EnemyState.Chasing);
            return;
        }

        var attackNumber = GD.Randi() % 2 + 1;  // generate 1 or 2 randomly
        _animationPlayer.Play($"attack{attackNumber}");
    }

    public void HandleDyingState(double delta)
    {
        _animationPlayer.Play("death");
        Velocity = Vector2.Zero;
    }

    public void SeekPlayer()
    {
        if (EnemySeekComponent.PlayerInRange())
        {
            ChangeState(EnemyState.Chasing);
        }
    }

    private void UpdateFacingDirection()
    {
        var facingRight = Velocity.X > 0;
        var directionFactor = facingRight ? -1 : 1;
        _animatedSprite.FlipH = facingRight;
        _hitboxes.Scale = new Vector2(directionFactor, 1);
        AttackRange.Scale = new Vector2(directionFactor, 1);
    }

    public void RestartWanderTimerIfOver()
    {
        if (WanderComponent.GetTimeLeft() == 0)
        {
            ChangeState(PickRandomState(new List<EnemyState> { EnemyState.Idle, EnemyState.Wandering, EnemyState.Wandering }));
            WanderComponent.StartWanderTmer(GD.RandRange(1, 3));
        }
    }

    private void OnHealthDepleted()
    {
        if (_enemyState != EnemyState.Dying)
            ChangeState(EnemyState.Dying);
    }

    public void OnDeathAnimationFinished()
    {
        QueueFree();
    }

    public EnemyState PickRandomState(List<EnemyState> stateList)
    {
        // Order the list by new randomly created guids and return the first one
        return stateList.OrderBy(x => System.Guid.NewGuid()).FirstOrDefault();
    }

    public override void TakeDamage(float damage)
    {
        if (_enemyState != EnemyState.Dying)
        {
            base.TakeDamage(damage);
            HealthComponent.TakeDamage(damage);
        }
    }
}
