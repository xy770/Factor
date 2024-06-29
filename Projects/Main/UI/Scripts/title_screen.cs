using Godot;
using System;

public partial class title_screen : Control
{
    public override void _Ready()
    {

    }

    public void OnNewGamePressed()
    {
        GetNode<Game>("/root/Game").NewGame();
    }

    public void OnLoadGamePressed()
    {
        GetNode<Game>("/root/Game").LoadGame();
    }

    public void OnExitGamePressed()
    {
        GetTree().Quit();
    }
}
