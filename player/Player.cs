using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class Player : CharacterBody2D
{
    [Export]
	public float Speed = 160.0f;
    [Export]
	public float JumpVelocity = -350.0f;
    [Export]
    public float DoubleJumpVelocity = -300.0f;
    [Export]
    public float ClimbFallSpeed = 70.0f;
    [Export]
    public float DashSpeed = 220.0f;
    [Export]
    public HurtboxComponent HurtboxComponent;
    [Export]
    public HurtboxComponent DashingHurtboxComponent;
    [Export]
    public HealthComponent HealthComponent;
    [Export]
    public Timer ImmunityTimer;

    private const float JUMP_THRESHOLD = 20;

    private AnimationTree _animationTree;
    private AnimationNodeStateMachinePlayback _animationState;
    private PlayerState _playerState;
    private CollisionShape2D _collisionShape;
    private CollisionShape2D _dashingCollisionShape;
    private CollisionShape2D _hurtboxShape;
    private CollisionShape2D _dashingHurtboxShape;
    private bool _isFalling = false;
    private RayCast2D _playerRaycast;
    private Timer _dashTimer;
    // Direction the player is facing. 1 for right, -1 for left
    private int _playerDirection = 1;
    private bool _airDash = false;
    private bool _canDash = true;
    private bool _canDoubleJump = false;

    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Climbing,
        Dashing,
        Attacking,
        Dying
    }

    public override void _Ready()
    {
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        _dashingCollisionShape = GetNode<CollisionShape2D>("DashingCollisionShape2D");
        _hurtboxShape = GetNode<CollisionShape2D>("HurtboxComponent/CollisionShape2D");
        _dashingHurtboxShape = GetNode<CollisionShape2D>("DashingHurtboxComponent/CollisionShape2D");
        _playerRaycast = GetNode<RayCast2D>("ClimbRayCast");
        _dashTimer = GetNode<Timer>("DashTimer");
        _animationTree = GetNode<AnimationTree>("AnimationTree");
        _animationTree.Active = true;
        _animationState = (AnimationNodeStateMachinePlayback)_animationTree.Get("parameters/playback");

        HurtboxComponent.DamageTaken += TakeDamage;
        DashingHurtboxComponent.DamageTaken += TakeDamage;
        HealthComponent.HealthDepleted += OnHealthDepleted;
        ChangeState(PlayerState.Idle);

        var eventBus = GetNode<EventBus>("/root/EventBus");
        eventBus.HeartPickedUp += IncreaseMaxHealth;
    }

    public override void _ExitTree()
    {
        HurtboxComponent.DamageTaken -= TakeDamage;
        DashingHurtboxComponent.DamageTaken -= TakeDamage;
        HealthComponent.HealthDepleted -= OnHealthDepleted;

        var eventBus = GetNode<EventBus>("/root/EventBus");
        eventBus.HeartPickedUp -= IncreaseMaxHealth;
    }

    public override void _PhysicsProcess(double delta)
	{
        switch (_playerState)
        {
            case PlayerState.Idle:
                HandleIdleState(delta);
                break;
            case PlayerState.Running:
                HandleRunningState(delta);
                break;
            case PlayerState.Jumping:
                HandleJumpingState(delta);
                break;
            case PlayerState.Falling:
                HandleFallingState(delta);
                break;
            case PlayerState.Climbing:
                HandleClimbingState(delta);
                break;
            case PlayerState.Dashing:
                HandleDashingState(delta);
                break;
            case PlayerState.Attacking:
                HandleAttackingState(delta);
                break;
            case PlayerState.Dying:
                HandleDyingState(delta);
                break;
        }
		MoveAndSlide();
	}

    // Method to change states
    private void ChangeState(PlayerState newState)
    {
        if (_playerState == newState)
            return;

        var airborneStates = new List<PlayerState>() { PlayerState.Jumping, PlayerState.Falling };

        var oldState = _playerState;
        _playerState = newState;

        if (newState == PlayerState.Jumping)
        {
            if (oldState == PlayerState.Falling)
            {
                Velocity = Velocity with { Y = DoubleJumpVelocity };
            }
            else
            {
                Velocity = Velocity with { Y = JumpVelocity };
                _canDoubleJump = true;
            }

            if (oldState == PlayerState.Attacking)
            {
                _animationState.Stop();
            }
        }
        else if (newState == PlayerState.Dashing)
        {
            _airDash = airborneStates.Contains(oldState);
            ToggleDashingCollisionShape(true);
        }
        else if (newState == PlayerState.Climbing)
        {
            _canDoubleJump = true;
        }

        if (oldState == PlayerState.Dashing)
        {
            ToggleDashingCollisionShape(false);
        }
    }

    private void HandleIdleState(double delta)
    {
        Vector2 direction = GetDirectionVector();

        if (direction.X != 0)
        {
            ChangeState(PlayerState.Running);
        }
        else
        {
            // Handle movement
            Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Speed) };

            // Handle animation
            _animationState.Travel("Idle");
        }

        if (Input.IsActionJustPressed("jump"))
            ChangeState(PlayerState.Jumping);

        if (Input.IsActionJustPressed("dash"))
            ChangeState(PlayerState.Dashing);

        if (Input.IsActionJustPressed("attack"))
            ChangeState(PlayerState.Attacking);

        if (!IsOnFloor())
            ChangeState(PlayerState.Falling);
    }

    private void HandleRunningState(double delta)
    {
        Vector2 direction = GetDirectionVector();

        if (direction.X != 0)
        {
            // Handle movement
            Velocity = Velocity with { X = direction.X * Speed };

            // Handle animation
            UpdateAnimationTreeDirections(direction);
            _animationState.Travel("Run");
        }
        else
        {
            ChangeState(PlayerState.Idle);
        }

        if (Input.IsActionJustPressed("jump"))
            ChangeState(PlayerState.Jumping);

        if (Input.IsActionJustPressed("dash"))
            ChangeState(PlayerState.Dashing);

        if (Input.IsActionJustPressed("attack"))
            ChangeState(PlayerState.Attacking);

        if (!IsOnFloor())
            ChangeState(PlayerState.Falling);
    }

    private void HandleJumpingState(double delta)
    {
        ApplyGravity(delta);
        // Jump apex reached once velocity changes direction
        var isJumpApex = Math.Abs(Velocity.Y) <= JUMP_THRESHOLD;

        Vector2 direction = GetDirectionVector();

        if (direction.X != 0)
        {
            // Handle movement
            Velocity = Velocity with { X = direction.X * Speed };

            // Handle animation
            UpdateAnimationTreeDirections(direction);
        } 
        else
        {
            // Handle movement
            Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Speed) };
        }

        // Handle animation
        _animationState.Travel("Jump");

        if (Input.IsActionJustPressed("jump") && _canDoubleJump)
        {
            Velocity = Velocity with { Y = DoubleJumpVelocity };
            _animationState.Travel("Idle");
            _animationState.Travel("Jump");
            _canDoubleJump = false;
        }

        // We pass to falling state once we reach the jump apex
        if (isJumpApex)
            ChangeState(PlayerState.Falling);

        if (Input.IsActionJustPressed("dash") && _canDash)
            ChangeState(PlayerState.Dashing);

        if (Input.IsActionJustPressed("attack"))
            ChangeState(PlayerState.Attacking);
    }

    private void HandleFallingState(double delta)
    {
        ApplyGravity(delta);

        Vector2 direction = GetDirectionVector();
        if (direction.X != 0)
        {
            // Handle horizontal movement
            Velocity = Velocity with { X = direction.X * Speed };
            UpdateAnimationTreeDirections(direction);

            if (_playerRaycast.IsColliding())
            {
                ChangeState(PlayerState.Climbing);
                _canDash = true;
            }
        }
        else
        {
            // Handle movement
            Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Speed) };
        }

        _animationState.Travel("Fall");

        if (IsOnFloor())
        {
            ChangeState(PlayerState.Idle);
            _canDash = true;
            _canDoubleJump = true;
        }

        if (Input.IsActionJustPressed("jump") && _canDoubleJump)
        {
            ChangeState(PlayerState.Jumping);
            _canDoubleJump = false;
        }

        if (Input.IsActionJustPressed("dash") && _canDash)
            ChangeState(PlayerState.Dashing);

        if (Input.IsActionJustPressed("attack"))
            ChangeState(PlayerState.Attacking);
    }

    // TODO: Change how raycasts are being directed
    private void HandleClimbingState(double delta)
    {
        Vector2 direction = GetDirectionVector();
        if (direction.X != 0)
        {
            // Handle movement
            Velocity = Velocity with { X = direction.X * Speed, Y = ClimbFallSpeed };   // TODO: figure out how to keep this logic in their corresponding states
            UpdateAnimationTreeDirections(direction);
            _animationState.Travel("Climb");

            if (!_playerRaycast.IsColliding())
            {
                ChangeState(PlayerState.Falling);
            }

            if (Input.IsActionJustPressed("jump"))
            {
                // TODO: Add a timer before changing to jump
                ChangeState(PlayerState.Jumping);
            }
        }
        else
        {
            ChangeState(PlayerState.Idle);
        }

        if (IsOnFloor())
            ChangeState(PlayerState.Idle);
    }

    private void HandleDashingState(double delta)
    {
        if (_dashTimer.IsStopped())
        {
            _dashTimer.Start();
        }
        Velocity = Velocity with { X = DashSpeed * _playerDirection, Y = 0 };

        // Handle animation
        _animationState.Travel("Dash");

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            ChangeState(PlayerState.Jumping);

        if (!IsOnFloor() && !_airDash)
            ChangeState(PlayerState.Falling);

        if (_airDash)
            _canDash = false;
    }

    private void ToggleDashingCollisionShape(bool enable)
    {
        _collisionShape.CallDeferred(CollisionShape2D.MethodName.SetDisabled, enable);
        _dashingCollisionShape.CallDeferred(CollisionShape2D.MethodName.SetDisabled, !enable);
        _hurtboxShape.CallDeferred(CollisionShape2D.MethodName.SetDisabled, enable);
        _dashingHurtboxShape.CallDeferred(CollisionShape2D.MethodName.SetDisabled, !enable);
    }

    public void OnDashTimerTimeout()
    {
        if (_playerState == PlayerState.Dashing)
        {
            if (IsOnFloor())
            {
                ChangeState(PlayerState.Idle);
            }
            else
            {
                _canDash = false;
                ChangeState(PlayerState.Falling);
            }
        }
    }

    private void HandleAttackingState(double delta)
    {
        ApplyGravity(delta);

        Vector2 direction = GetDirectionVector();
        if (direction.X != 0)
        {
            // Handle horizontal movement
            Velocity = Velocity with { X = direction.X * Speed };
        }
        else
        {
            // Handle movement
            Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Speed) };
        }
        // Handle animation
        _animationState.Travel("Attack1");

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            ChangeState(PlayerState.Jumping);
    }

    public void AttackAnimationFinished()
    {
        if (IsOnFloor())
        {
            ChangeState(PlayerState.Idle);
        } 
        else
        {
            ChangeState(PlayerState.Falling);
        }
    }

    private void ApplyGravity(double delta)
    {
        // Add the gravity.
        if (!IsOnFloor())
        {
            Velocity += GetGravity() * (float)delta;
        }
    }

    private void HandleDyingState(double delta)
    {
        // Handle movement
        Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, Speed) };
        ApplyGravity(delta);

        // Handle animation
        _animationState.Travel("Death");
    }

    private void UpdateAnimationTreeDirections(Vector2 direction)
    {
        _animationTree.Set("parameters/Idle/blend_position", direction.X);
        _animationTree.Set("parameters/Run/blend_position", direction.X);
        _animationTree.Set("parameters/Jump/blend_position", direction.X);
        _animationTree.Set("parameters/FallTransition/blend_position", direction.X);
        _animationTree.Set("parameters/Fall/blend_position", direction.X);
        _animationTree.Set("parameters/Climb/blend_position", direction.X);
        _animationTree.Set("parameters/DashTransition/blend_position", direction.X);
        _animationTree.Set("parameters/Dash/blend_position", direction.X);
        _animationTree.Set("parameters/Attack1/blend_position", direction.X);
        _animationTree.Set("parameters/Attack2/blend_position", direction.X);
        _animationTree.Set("parameters/Death/blend_position", direction.X);
    }

    private Vector2 GetDirectionVector()
    {
        var direction = Input.GetVector("left", "right", "up", "down").Normalized();
        if (direction.X != 0)
            _playerDirection = direction.X > 0 ? 1 : -1;
        return direction;
    }

    public void TakeDamage(float damage)
    {
        if (ImmunityTimer.TimeLeft != 0)
            return;

        GetNode<AnimationPlayer>("BlinkAnimationPlayer").Play("start");
        GetNode<EventBus>("/root/EventBus").EmitSignal("PlayerTakeDamage", damage);
        HealthComponent.TakeDamage(damage);
        ImmunityTimer.Start();
    }

    private void IncreaseMaxHealth(int amount)
    {
        GetNode<EventBus>("/root/EventBus").EmitSignal("PlayerMaxHealthChanged", amount);
        HealthComponent.MaxHealth += amount;
    }

    public void OnHealthDepleted()
    {
        ChangeState(PlayerState.Dying);
    }

    public void DeathAnimationFinished()
    {
        var eventBus = GetNode<EventBus>("/root/EventBus");
        eventBus.EmitSignal("PlayerDied");
    }

    public Vector2 GetTargetPosition()
    {
        return GetNode<Marker2D>("TargetPoint").GlobalPosition;
    }
}
