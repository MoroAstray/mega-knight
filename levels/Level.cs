using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Level : Node2D
{
	[Export]
	public Timer FireballSpawnTimer;
	[Export]
	public PackedScene FireballScene;
	[Export]
	public AudioStreamPlayer BossMusic;
    [Export]
    public AudioStreamPlayer LevelMusic;
    [Export]
    public AudioStreamPlayer GameOverMusic;

	private bool _spawnFireballs = false;
    public override void _Ready()
    {
        var eventBus = GetNode<EventBus>("/root/EventBus");
        eventBus.PlayerDied += OnPlayerDied;
		eventBus.BossFirstPhaseDefeated += BossSecondPhase;
		GameOverMusic.Stop();
		BossMusic.Stop();
    }

    public override void _ExitTree()
    {
        var eventBus = GetNode<EventBus>("/root/EventBus");
        eventBus.PlayerDied -= OnPlayerDied;
        eventBus.BossFirstPhaseDefeated -= BossSecondPhase;
    }
    public override void _Process(double delta)
    {
		if (Input.IsActionJustPressed("pause"))
			GetNode<EventBus>("/root/EventBus").EmitSignal("PauseGame");
    }

	public void OnFireballSpawnTimerTimeout()
	{
		if (!_spawnFireballs)
			return;

		var spawnMarkers = ChooseRandomSpawnMarkers();
		SpawnFireballs(spawnMarkers);
	}

	private List<Marker2D> ChooseRandomSpawnMarkers()
	{
		var combination = GD.RandRange(1, 4);
		var markersNode = GetNode<Node2D>("FireballMarkers");
		var spawnMarkers = markersNode.GetChildren().OfType<Marker2D>().ToList();

        switch (combination)
		{
			case 1:
				spawnMarkers = spawnMarkers.Where(marker => marker.Name.ToString().Contains("Up")).ToList();
				break;
			case 2:
                spawnMarkers = spawnMarkers.Where(marker => marker.Name.ToString().Contains("Down")).ToList();
				break;
			case 3:
                spawnMarkers = spawnMarkers.Where(marker => marker.Name.ToString().Contains("Horizontal")).ToList();
				break;
			case 4:
				spawnMarkers = spawnMarkers.OrderBy(x => new Random().Next()).Take(10).ToList();
				break;
        }
		return spawnMarkers;
    }

	private void SpawnFireballs(List<Marker2D> markers)
	{
        foreach (var marker in markers)
        {
            var fireball = FireballScene.Instantiate<Fireball>();
			fireball.Speed = 120;
            var direction = Vector2.Zero;
			if (marker.Name.ToString().Contains("Horizontal"))
			{
				direction = new Vector2(-1, 0);
			} 
			else if (marker.Name.ToString().Contains("Up"))
			{
				direction = new Vector2(0, 1);
			}
			else if (marker.Name.ToString().Contains("Down"))
			{
				direction = new Vector2(0, -1);
			}
			fireball.Direction = direction;
			marker.AddChild(fireball);
        }
    }

	public void BossAreaEntered(Node2D body)
	{
		if (body is not Player player)
			return;

		if (BossMusic.Playing)
			return;

		BossMusic.Play();
		LevelMusic.Stop();
	}

	public void OnPlayerDied()
	{
		GameOverMusic.Play();
        BossMusic.Stop();
        LevelMusic.Stop();
    }

    public void BossSecondPhase()
    {
		_spawnFireballs = true;
        BossMusic.Play();
        LevelMusic.Stop();
    }
}
