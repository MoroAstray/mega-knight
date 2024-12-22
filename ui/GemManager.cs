using Godot;
using System;
using static Gem;

public partial class GemManager : Control
{
    [Export]
    public TextureRect Ruby;
    [Export]
    public TextureRect Sapphire;
    [Export]
    public TextureRect Emerald;
    [Export]
    public TextureRect Diamond;

    public override void _Ready()
    {
        GetNode<EventBus>("/root/EventBus").GemPickedUp += OnGemPickedUp;
    }

    public override void _ExitTree()
    {
        GetNode<EventBus>("/root/EventBus").GemPickedUp -= OnGemPickedUp;
    }

    public void OnGemPickedUp(GemType gem)
    {
        switch (gem)
        {
            case GemType.Ruby:
                Ruby.Visible = true;
                break;
            case GemType.Sapphire:
                Sapphire.Visible = true;
                break;
            case GemType.Emerald:
                Emerald.Visible = true;
                break;
            case GemType.Diamond:
                Diamond.Visible = true;
                break;
        }
    }

    public void StartOver()
    {
        Ruby.Visible = false;
        Sapphire.Visible = false;
        Emerald.Visible = false;
        Diamond.Visible = false;
    }
}
