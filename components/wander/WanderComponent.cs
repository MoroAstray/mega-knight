using Godot;
public partial class WanderComponent : Node2D
{
    [Export]
    public int WanderRange = 30;
    [Export]
    public bool IsGroundEnemy;
    [Export]
    public bool FixedLength = false;

    public const float WANDER_EPSILON = 15.0f;

    private Vector2 _startPosition;
    private Vector2 _targetPosition;

    private Timer _timer;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        _targetPosition = GlobalPosition;
        _timer = GetNode<Timer>("Timer");
    }

    public void UpdateTargetPosition()
    {
        var isGroundVectorMultiplier = IsGroundEnemy ? 0 : 1;
        Vector2 targetVector;
        if (FixedLength)
        {
            var directionX = GD.Randf() > 0.5 ? 1 : -1;
            var directionY = GD.Randf() > 0.5 ? 1 : -1;
            targetVector = new Vector2(directionX * WanderRange, directionY * WanderRange * isGroundVectorMultiplier);
        } 
        else
        {
            targetVector = new Vector2(GD.RandRange(-WanderRange, WanderRange), GD.RandRange(-WanderRange, WanderRange) * isGroundVectorMultiplier);
        }
        _targetPosition = _startPosition + targetVector;
    }

    public double GetTimeLeft()
    {
        return _timer.TimeLeft;
    }

    public void OnTimerTimeout()
    {
        UpdateTargetPosition();
    }

    public void StartWanderTmer(double duration)
    {
        _timer.Start(duration);
    }

    public Vector2 GetTargetPosition()
    {
        return _targetPosition;
    }
}
