using Godot;
using Godot.NativeInterop;
using System;

public partial class GameManager : Node2D
{
    [Export]
    public PauseMenu PauseMenu;
    [Export]
    public EndMenu EndMenu;
    [Export]
    public CoinManager CoinManager;
    [Export]
    public LevelSelection LevelSelection;
    [Export]
    public TitleScreen TitleScreen;
    [Export]
    public GemManager GemManager;
    [Export]
    public PlayerHealthBar HealthBar;
    [Export]
    public AudioStreamPlayer BackgroundMusic;

    private string _currentLevel;

    public override void _Ready()
    {
        var eventBus = GetNode<EventBus>("/root/EventBus");
        eventBus.PlayerDied += OnPlayerDied;
        eventBus.PauseGame += PauseGame;
        eventBus.QuitLevel += QuitLevel;
        eventBus.StartLevel += StartLevel;
        eventBus.ContinueTitleScreen += ContinueTitleScreen;
        eventBus.ExitLevelSelection += ExitLevelSelection;
        eventBus.RestartLevel += RestartLevel;
        eventBus.GameFinished += FinishGame;
    }

    private void PauseGame()
    {
        GetTree().Paused = true;
        PauseMenu.Visible = true;
    }

    public void StartLevel()
    {
        var packedScene = (PackedScene)ResourceLoader.Load("res://levels/level1/level_1.tscn");
        var scene = packedScene.Instantiate();
        scene.Name = "Level1";
        AddChild(scene);
        UpdateCoins("Level1");
        CoinManager.Visible = true;
        GemManager.Visible = true;
        HealthBar.Visible = true;
        _currentLevel = "Level1";
        GemManager.StartOver();
        HealthBar.StartOver();
        BackgroundMusic.Playing = false;
    }

    private void UpdateCoins(string level)
    {
        CoinManager.UpdateCoinCount(0);
        PlayerStats.Instance.PickedUpCoins = 0;
        var coinsChild = GetNode(level).FindChild("Coins");
        var maxCoins = coinsChild.GetChildren().Count;
        PlayerStats.Instance.MaxCoins = maxCoins;
    }

    public void OnPlayerDied()
    {
        GetTree().Paused = true;
        EndMenu.SetLabelText("You died!");
        EndMenu.Visible = true;
    }

    public void QuitLevel()
    {
        GetNode<Level>(_currentLevel).QueueFree();
        BackgroundMusic.Playing = true;
        LevelSelection.Visible = true;
        CoinManager.Visible = false;
        GemManager.Visible = false;
        HealthBar.Visible = false;
    }

    public void ContinueTitleScreen()
    {
        LevelSelection.Visible = true;
    }

    public void ExitLevelSelection()
    {
        TitleScreen.Visible = true;
    }

    public void RestartLevel()
    {
        var node = GetNode<Level>("Level1");
        RemoveChild(node);
        node.QueueFree();
        StartLevel();
    }
    
    public void FinishGame()
    {
        GetTree().Paused = true;
        EndMenu.SetLabelText("You finished the level!");
        EndMenu.Visible = true;
        GetNode<AudioStreamPlayer>("VictorySFX").Play();
    }
}
