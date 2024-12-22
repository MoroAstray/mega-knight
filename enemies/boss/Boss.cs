using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Boss : Enemy
{
    [Export]
    public WanderComponent WanderComponent;
    [Export]
    public HealthComponent HealthComponent;
    [Export]
    public HurtboxComponent HurtboxComponent;
    [Export]
    public EnemySeekComponent EnemySeekComponent;
    [Export]
    public EnemySeekComponent AttackRange;

    private AnimationPlayer _animationPlayer;
    private EnemyState _enemyState;
    private AnimatedSprite2D _animatedSprite;
    private Node2D _hitboxes;

    public enum EnemyState
    {
        Idle,
        Wandering,
        Chasing,
        Attacking,
        Dying
    }

    public override void _Ready()
    {
        _enemyState = EnemyState.Idle;
        _hitboxes = GetNode<Node2D>("Hitboxes");
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        HurtboxComponent.DamageTaken += TakeDamage;
        HealthComponent.HealthDepleted += OnHealthDepleted;

        UpdateFacingDirection();
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
    
    public void HandleIdleState(double delta)
    {
        if (AttackRange.PlayerInRange())
        {
            ChangeState(EnemyState.Attacking);
            return;
        }

        SeekPlayer();
        Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Friction * (float)delta) };

        RestartWanderTimerIfOver();
        _animatedSprite.Play("idle");
    }

    public void HandleWanderingState(double delta)
    {
        RestartWanderTimerIfOver();

        if (AttackRange.PlayerInRange())
        {
            ChangeState(EnemyState.Attacking);
            return;
        }

        SeekPlayer();
        var wanderTargetPosition = WanderComponent.GetTargetPosition();
        var direction = GlobalPosition.DirectionTo(wanderTargetPosition);

        if (GlobalPosition.DistanceTo(wanderTargetPosition) <= WanderComponent.WANDER_EPSILON)
        {
            _animatedSprite.Play("idle");
            _enemyState = PickRandomState(new List<EnemyState> { EnemyState.Idle, EnemyState.Wandering, EnemyState.Wandering, EnemyState.Attacking, EnemyState.Attacking });
            WanderComponent.StartWanderTmer(GD.RandRange(1, 2));
            Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Friction * (float)delta) };
        }
        else
        {
            Velocity = Velocity with { X = Mathf.MoveToward(direction.X * Speed, 0, Friction * (float)delta) };
            _animatedSprite.Play("walk");
        }
        UpdateFacingDirection();
    }

    public void HandleChasingState(double delta)
    {
        var player = EnemySeekComponent.GetPlayerInRange();
        if (player == null)
        {
            ChangeState(PickRandomState(new List<EnemyState> { EnemyState.Idle, EnemyState.Wandering, EnemyState.Attacking }));
            return;
        }

        var direction = GlobalPosition.DirectionTo(player.GlobalPosition);
        Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, direction.X * Speed, Friction * (float)delta) };
        _animatedSprite.Play("walk");
        if (AttackRange.PlayerInRange())
        {
            ChangeState(EnemyState.Attacking);
        }
        UpdateFacingDirection();
    }

    public void HandleAttackingState(double delta)
    {
        Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Friction * (float)delta) };

        if (_animationPlayer.CurrentAnimation.Contains("attack"))
            return;

        if (!AttackRange.PlayerInRange())
        {
            ChangeState(EnemyState.Chasing);
            return;
        }

        var attackNumber = GD.Randi() % 3 + 1;  // generate 1 or 2 randomly
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

    public void RestartWanderTimerIfOver()
    {
        if (WanderComponent.GetTimeLeft() == 0)
        {
            ChangeState(PickRandomState(new List<EnemyState> { EnemyState.Idle, EnemyState.Wandering, EnemyState.Attacking, EnemyState.Wandering, EnemyState.Attacking }));
            WanderComponent.StartWanderTmer(GD.RandRange(1, 2));
        }
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

    private void OnHealthDepleted()
    {
        if (_enemyState != EnemyState.Dying)
            ChangeState(EnemyState.Dying);
    }

    public void AttackAnimationFinished()
    {
        if (AttackRange.PlayerInRange())
        {
            ChangeState(EnemyState.Attacking);
        } 
        else
        {
            _enemyState = PickRandomState(new List<EnemyState> { EnemyState.Idle, EnemyState.Wandering, EnemyState.Attacking });
        }
    }

    private void UpdateFacingDirection()
    {
        bool facingRight;
        if (Velocity.X == 0)
        {
            var player = (Player)GetParent().FindChild("Player");
            var direction = GlobalPosition.DirectionTo(player.GlobalPosition);
            facingRight = direction.X > 0;
        }
        else
        {
            facingRight = Velocity.X > 0;
        }

        var directionFactor = facingRight ? -1 : 1;
        _animatedSprite.FlipH = facingRight;
        _hitboxes.Scale = new Vector2(directionFactor, 1);
        AttackRange.Scale = new Vector2(directionFactor, 1);
    }

    public void OnDeathAnimationFinished()
    {
        GetNode<EventBus>("/root/EventBus").EmitSignal("GameFinished");
    }
}
