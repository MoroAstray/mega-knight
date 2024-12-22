using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EvilSpirit : Enemy
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
    private RayCast2D _groundDetectionRayCast;
    private Node2D _hitboxes;
    private Timer _jumpTimer;
	public enum EnemyState
	{
        Spawning,
		Idle,
        Wandering,
		Chasing,
		Attacking,
		Dying
	}

    public override void _Ready()
    {
        _enemyState = EnemyState.Spawning;
        _hitboxes = GetNode<Node2D>("Hitboxes");
        _jumpTimer = GetNode<Timer>("JumpTimer");
        _groundDetectionRayCast = GetNode<RayCast2D>("GroundDetectionRayCast2D");
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
        ApplyGravity(EnemyGravity, delta);
        switch (_enemyState)
        {
            case EnemyState.Spawning:
                HandleSpawningState(delta);
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

        if (state == EnemyState.Chasing)
        {
            _jumpTimer.Start(3);
        }
        else if (state == EnemyState.Dying)
        {
            GetNode("Hitboxes").QueueFree();
        }
    }

    public void HandleSpawningState(double delta)
    {
        _animationPlayer.Play("spawn");
    }

    public void HandleIdleState(double delta)
    {
        Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Friction * (float)delta) };

        SeekPlayer();
        RestartWanderTimerIfOver();
        _animationPlayer.Play("idle");
    }

    public void HandleWanderingState(double delta)
    {
        SeekPlayer();
        RestartWanderTimerIfOver();

        var wanderTargetPosition = WanderComponent.GetTargetPosition();
        var direction = GlobalPosition.DirectionTo(wanderTargetPosition);
        UpdateGroundRayCast(wanderTargetPosition);

        if (GlobalPosition.DistanceTo(wanderTargetPosition) <= WanderComponent.WANDER_EPSILON || !_groundDetectionRayCast.IsColliding())
        {
            _animatedSprite.Play("idle");
            _enemyState = PickRandomState(new List<EnemyState> { EnemyState.Idle, EnemyState.Wandering, EnemyState.Wandering });
            WanderComponent.StartWanderTmer(GD.RandRange(1, 3));
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
            ChangeState(EnemyState.Idle);
            return;
        }

        JumpWhenTimerIsOver();

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

        var attackNumber = GD.Randi() % 2 + 1;  // generate 1 or 2 randomly
        _animationPlayer.Play($"attack{attackNumber}");
    }

    public void HandleDyingState(double delta)
    {
        _animationPlayer.Play("death");
        Velocity = Vector2.Zero;
    }

    private void UpdateFacingDirection()
    {
        var facingRight = Velocity.X > 0;
        var directionFactor = facingRight ? -1 : 1;
        _animatedSprite.FlipH = facingRight;
        _hitboxes.Scale = new Vector2(directionFactor, 1);
        AttackRange.Scale = new Vector2(directionFactor, 1);
    }

    private void UpdateGroundRayCast(Vector2 targetVector) 
    {
        var directionFactor = GlobalPosition.X > targetVector.X ? -1 : 1;
        _groundDetectionRayCast.Position = _groundDetectionRayCast.Position with { X = Math.Abs(_groundDetectionRayCast.Position.X) * directionFactor };
    }

    public void FinshedSpawning()
    {
        if (_enemyState == EnemyState.Spawning)
            ChangeState(EnemyState.Idle);
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
            ChangeState(PickRandomState(new List<EnemyState> { EnemyState.Idle, EnemyState.Wandering, EnemyState.Wandering }));
            WanderComponent.StartWanderTmer(GD.RandRange(1, 3));
        }
    }

    public EnemyState PickRandomState(List<EnemyState> stateList)
    {
        // Order the list by new randomly created guids and return the first one
        return stateList.OrderBy(x => System.Guid.NewGuid()).FirstOrDefault();
    }

    private void JumpWhenTimerIsOver()
    {
        if (_jumpTimer.TimeLeft == 0)
        {
            Jump();
            _jumpTimer.Start(GD.RandRange(2, 4));
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

    private void OnHealthDepleted()
    {
        if (_enemyState != EnemyState.Dying)
            ChangeState(EnemyState.Dying);
    }

    public void OnDeathAnimationFinished() 
    {
        QueueFree();
    }
}
