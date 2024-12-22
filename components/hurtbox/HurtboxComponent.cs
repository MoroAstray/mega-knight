using Godot;
using System;

public partial class HurtboxComponent : Area2D
{
    [Signal]
    public delegate void DamageTakenEventHandler(float damage);
    public void TakeDamage(float damage)
    {
        EmitSignal(SignalName.DamageTaken, damage);
    }

    // For interaction with damaging tiles like spikes
    public void OnBodyShapeEntered(Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex)
    {
        if (body is not TileMapLayer tileMapLayer)
            return;

        var tileCoords = tileMapLayer.GetCoordsForBodyRid(bodyRid); // tile coordinates are based on the world's coordinates
        var tileData = tileMapLayer.GetCellTileData(tileCoords);
        var tileDamage = tileData.GetCustomData("damage").As<float>();

        TakeDamage(tileDamage);
    }
}
