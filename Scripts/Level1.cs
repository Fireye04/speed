using Godot;
using System;

public partial class Level1 : Node3D
{	

	[Export]
	private PackedScene playerScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (GameManager.Players.Count == 0) {
			GameManager.Players.Add(new PlayerInfo(){Name="Tester", Id=1});
		}
		int index = 0;
		foreach (var item in GameManager.Players) {
			Player currentPlayer = playerScene.Instantiate<Player>();
			currentPlayer.Name = item.Id.ToString();
			AddChild(currentPlayer);
			foreach (var spawnPoint in GetTree().GetNodesInGroup("Spawn Points")){
				Node3D sp = (Node3D)spawnPoint;
				if (int.Parse(sp.Name) == index) {
					currentPlayer.GlobalPosition = sp.GlobalPosition;
				}
			}
			index ++;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
