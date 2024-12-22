using Godot;
using System;

public partial class EnemySeekComponent : Area2D
{
    private Player _player = null;

    public bool PlayerInRange()
    {
        return _player != null;
    }

    public void OnBodyEntered(Node2D body)
    {
        if (body is not Player player)
            return;

        _player = player;
    }

    public void OnBodyExited(Node2D body)
    {
        if (body is not Player player)
            return;

        _player = null;
    }

    public Player GetPlayerInRange()
    {
        return _player;
    }
}
