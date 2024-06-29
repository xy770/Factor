using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

[GlobalClass]
public partial class World : Node
{
	public TileMap Terrain;
	public Camera2D camera2D;
	[Export]
	public PlayerController player;

	public override void _Ready()
	{
		AddToGroup("World");

		Terrain = GetNode<TileMap>("Terrain");
		camera2D = GetNode<Camera2D>("Player/Camera2D");

		var used = Terrain.GetUsedRect().Grow(-1);
		var tileSize = Terrain.TileSet.TileSize;

		camera2D.LimitTop = used.Position.Y * tileSize.Y;
		camera2D.LimitRight = used.End.X * tileSize.X;
		camera2D.LimitBottom = used.End.Y * tileSize.Y;
		camera2D.LimitLeft = used.Position.X * tileSize.X;
	}

	public async void UpdatePlayer(Vector2 pos, float direction)
	{
		await ToSignal(this, "ready");

		player.GlobalPosition = pos;
		player.SetDirection((int)direction);
		camera2D.ResetSmoothing();
		camera2D.ForceUpdateScroll();
	}

	public WorldInfo GetWorldInfo()
	{
		var enemiesInfo = new List<EnemyInfo>();

		foreach (Node node in GetTree().GetNodesInGroup("Enemies"))
		{
			Enemy enemy = GetTree().CurrentScene.GetNode<Enemy>(node.GetPath());
			enemiesInfo.Add(enemy.GetEnemyInfo());
		}

		return new WorldInfo
		{
			WorldName = SceneFilePath.GetFile().GetBaseName(),
			PlayerInfo = player.GetPlayerInfo(),
			enemiesInfo = enemiesInfo,
		};
	}

	public void SetWorldInfo(WorldInfo worldInfo)
	{
		GD.Print("Set World Info: " + Serializer(worldInfo));
		player.SetPlayerInfo(worldInfo.PlayerInfo);

		foreach (Node node in GetTree().GetNodesInGroup("Enemies"))
		{
			Enemy enemy = GetTree().CurrentScene.GetNode<Enemy>(node.GetPath());
			var isFind = false;

			foreach (EnemyInfo enemyInfo in worldInfo.enemiesInfo)
			{
				if (enemyInfo.name == enemy.Name)
				{
					enemy.SetEnemyInfo(enemyInfo);
					isFind = true;
					break;
				}
			}

			if(!isFind)
			{
				enemy.QueueFree();
			}
		}
	}

	public string Serializer(WorldInfo worldInfo)
	{
		var options = new JsonSerializerOptions { WriteIndented = true };
		string json = JsonSerializer.Serialize(worldInfo, options);
		GD.Print(json);
		return json;
	}

	public void DeSerializer(string json)
	{
		WorldInfo worldInfo = JsonSerializer.Deserialize<WorldInfo>(json);
		SetWorldInfo(worldInfo);
	}
}

public class WorldInfo
{
	public string WorldName { get; set; }
	public PlayerInfo PlayerInfo { get; set; }
	public List<EnemyInfo> enemiesInfo { get; set; }
}
