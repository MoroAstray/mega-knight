using Godot;
using System;

public partial class HitboxComponent : Area2D
{
    [Export]
    public int Damage = 10;

    public void OnAreaEntered(Area2D area)
    {
        if (area is not HurtboxComponent hurtbox)
            return;

        if (hurtbox.Owner == this.Owner)
            return;

        hurtbox.TakeDamage(Damage);
    }
}
