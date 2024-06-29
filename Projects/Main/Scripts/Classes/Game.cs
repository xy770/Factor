using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Channels;

public partial class Game : Node
{
    const string archivePath = "user://data.sav";
    const string configPath = "user://config.ini";
    [Export]
    public Stats playerStats;
    [Export]
    public ColorRect transitionRect;
    public SoundManager soundManager;

    private GameInfo gameInfo = new();

    [Signal]
    public delegate void CameraShouldShakeEventHandler(int amount);

    public override void _Ready()
    {
        soundManager = GetNode<SoundManager>("/root/SoundManager");
        LoadConfig();
        transitionRect.Color = new Color(0, 0, 0, 0);

        gameInfo = new();
    }

    public static float MoveToward(float from, float to, float delta)
    {
        Vector2 s = new Vector2(from, 0);
        s = s.MoveToward(new Vector2(to, 0), delta);
        return s.X;
    }

    public async void ChangeScene(string path, ChangeSceneOption option)
    {
        var tree = GetTree();
        tree.Paused = true;

        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(transitionRect, "color:a", 1, 0.3f);
        await ToSignal(tween, "finished");

        if (tree.CurrentScene.Name == "World")
        {
            SyncGameInfo();
        }

        tree.ChangeSceneToFile(path);

        await ToSignal(tree, "tree_changed");

        if (tree.CurrentScene.Name == "World" && !option.ResetAll)
        {
            //var newName = tree.CurrentScene.SceneFilePath.GetFile().GetBaseName();
            World world = GetTree().CurrentScene.GetNode<World>(GetTree().CurrentScene.GetPath());
            if (gameInfo.WorldsInfo != null)
            {
                foreach (WorldInfo worldInfo in gameInfo.WorldsInfo)
                {
                    if (worldInfo.WorldName == world.SceneFilePath.GetFile().GetBaseName())
                    {
                        world.SetWorldInfo(worldInfo);
                        break;
                    }
                }
            }

            if (option.EntryPoint != null)
            {
                foreach (EntryPoint node in tree.GetNodesInGroup("EntryPoints"))
                {
                    if (node.Name == option.EntryPoint)
                    {
                        tree.CurrentScene.Call("UpdatePlayer", node.GlobalPosition, (float)node.direction);
                    }
                }
            }
        }
        else
        {
            playerStats.ResetStats();
        }

        tree.Paused = false;

        tween = CreateTween();
        tween.TweenProperty(transitionRect, "color:a", 0, 0.3f);
    }

    public void SaveGame()
    {
        World world = GetNode<World>(GetTree().CurrentScene.GetPath());
        var json = world.Serializer(world.GetWorldInfo());
        var file = FileAccess.Open(archivePath, FileAccess.ModeFlags.Write);

        if (file == null)
        {
            return;
        }

        file.StoreString(json);
    }

    public void SaveConfig()
    {
        var config = new ConfigFile();

        config.SetValue("audio", "master_volume", soundManager.GetVolume((int)SoundManager.Bus.Master));
        config.SetValue("audio", "sfx_volume", soundManager.GetVolume((int)SoundManager.Bus.SFX));
        config.SetValue("audio", "bgm_volume", soundManager.GetVolume((int)SoundManager.Bus.BGM));

        config.Save(configPath);
    }

    public void LoadGame()
    {
        if (gameInfo.LastWorldPath != null)
        {
            ChangeScene(gameInfo.LastWorldPath, new ChangeSceneOption { });
        }
    }

    public void LoadConfig()
    {
        var config = new ConfigFile();

        config.Load(configPath);

        soundManager.SetVolume((int)SoundManager.Bus.Master, (float)config.GetValue("audio", "master_volume", 1));
        soundManager.SetVolume((int)SoundManager.Bus.SFX, (float)config.GetValue("audio", "sfx_volume", 1));
        soundManager.SetVolume((int)SoundManager.Bus.BGM, (float)config.GetValue("audio", "bgm_volume", 1));
    }

    public void NewGame()
    {
        gameInfo = new();
        ChangeScene("res://Prefabs/Scene/Forest.tscn", new ChangeSceneOption { ResetAll = true });
    }

    public void BackToTitle()
    {
        ChangeScene("res://UI/title_screen.tscn", new ChangeSceneOption { });
    }

    public void ShakeCamera(int amount)
    {
        EmitSignal(SignalName.CameraShouldShake, amount);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("debug"))
        {
            // World world = GetNode<World>(GetTree().CurrentScene.GetPath());
            // var json = world.Serializer(world.GetWorldInfo());

            // var file = FileAccess.Open(archivePath, FileAccess.ModeFlags.Write);
            // if (file == null)
            // {
            //     return;
            // }

            // file.StoreString(json);


            // var file = FileAccess.Open(archivePath, FileAccess.ModeFlags.Read);
            // if (file == null)
            // {
            //     return;
            // }

            // var json = file.GetAsText();
            // World world = GetNode<World>(GetTree().CurrentScene.GetPath());
            // world.DeSerializer(json);

            var gameInfo = new GameInfo();
            gameInfo.WorldsInfo.Add(new WorldInfo());
        }
    }

    public GameInfo GetGameInfo()
    {
        return gameInfo;
    }

    public void SyncGameInfo()
    {
        World world = GetTree().CurrentScene.GetNode<World>(GetTree().CurrentScene.GetPath());
        if (gameInfo.WorldsInfo != null)
        {
            foreach (WorldInfo worldInfo in gameInfo.WorldsInfo)
            {
                if (worldInfo.WorldName == world.SceneFilePath.GetFile().GetBaseName())
                {
                    if (gameInfo.WorldsInfo.Remove(worldInfo))
                    {
                        GD.Print("Successfully remove the info");
                    }
                    break;
                }
            }
        }

        WorldInfo worldinfo = world.GetWorldInfo();
        worldinfo.WorldName = world.SceneFilePath.GetFile().GetBaseName();
        gameInfo.WorldsInfo.Add(worldinfo);
        gameInfo.LastWorldPath = world.SceneFilePath;

        GD.Print("SyncGameInfo: " + Serializer(gameInfo));
    }

    public void SetGameInfo(GameInfo gameInfo)
    {
        this.gameInfo = gameInfo;
    }

    public string Serializer(GameInfo gameInfo)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(gameInfo, options);

        return json;
    }

    public void DeSerializer(string json)
    {
        GameInfo gameInfo = JsonSerializer.Deserialize<GameInfo>(json);
        SetGameInfo(gameInfo);
    }
}

public class ChangeSceneOption
{
    public string EntryPoint = null;
    public bool ResetAll = false;
}

public class GameInfo
{
    public List<WorldInfo> WorldsInfo { set; get; }
    public string LastWorldPath;

    public GameInfo()
    {
        WorldsInfo = new();
    }
}