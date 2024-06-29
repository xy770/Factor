using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class Teleporter : Interactable
{
    [Export(PropertyHint.File, "*.tscn")]
    string path;
    [Export]
    string entryPoint;
    [Export]
    public Game game;

    public override void _Ready()
    {
        base._Ready();
        game = GetNode<Game>("/root/Game");
    }

    public override void Interact()
    {
        base.Interact();
        game.ChangeScene(path, new ChangeSceneOption{EntryPoint = entryPoint});
    }
}
