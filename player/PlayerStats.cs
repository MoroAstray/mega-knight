using Godot;
using System;
using System.Collections.Generic;
using static Gem;

public partial class PlayerStats : Node
{
    public static PlayerStats Instance { get; private set; }

    public int PickedUpCoins
    {
        get => _coins;
        set
        {
            _coins = value;
            EmitSignal(SignalName.CoinNumberChanged, _coins);
        }
    }
    private int _coins = 0;
    [Signal]
    public delegate void CoinNumberChangedEventHandler(int value);

    public int MaxCoins
    {
        get => _maxCoins;
        set
        {
            _maxCoins = value;
            EmitSignal(SignalName.MaxCoinsNumberChanged, _maxCoins);
        }
    }
    private int _maxCoins = 0;
    [Signal]
    public delegate void MaxCoinsNumberChangedEventHandler(int value);

    public List<GemType> PickedUpGems;

    public override void _Ready()
    {
        Instance = this;
        PickedUpGems = new List<GemType>();
    }
}
