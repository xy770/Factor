using Godot;
using System;

[GlobalClass]
public partial class EntryPoint : Marker2D
{
    [Export]
    public PlayerController.Direction direction = PlayerController.Direction.Right;

    public override void _Ready()
    {
        AddToGroup("EntryPoints");
    }
}
