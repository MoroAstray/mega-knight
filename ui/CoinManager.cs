using Godot;
using System;

public partial class CoinManager : Control
{
    private Label _coinLabel;
    private int _currentCoins = 0;
    private int _maxCoins = 0;
    public override void _Ready()
    {
        _coinLabel = GetNode<Label>("Label");
        PlayerStats.Instance.CoinNumberChanged += UpdateCoinCount;
        PlayerStats.Instance.MaxCoinsNumberChanged += UpdateMaxCoinsCount;

        UpdateCoinString();
    }

    public override void _ExitTree()
    {
        PlayerStats.Instance.CoinNumberChanged -= UpdateCoinCount;
        PlayerStats.Instance.MaxCoinsNumberChanged -= UpdateMaxCoinsCount;
    }

    public void UpdateCoinCount(int count)
    {
        _currentCoins = count;
        UpdateCoinString();
    }

    public void UpdateMaxCoinsCount(int maxCount)
    {
        _maxCoins = maxCount;
        UpdateCoinString();
    }

    private void UpdateCoinString()
    {
        _coinLabel.Text = $"{_currentCoins}/{_maxCoins}";
    }
}
