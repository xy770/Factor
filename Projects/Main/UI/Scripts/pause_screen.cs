using Godot;
using System;

public partial class pause_screen : Control
{
    [Export]
    public AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            Hide();
            GetWindow().SetInputAsHandled();
        }
    }

    public void ShowPauseScreen()
    {
        Show();
    }

    public void OnVisibilityChanged()
    {
        if(Visible)
        {
            animationPlayer.Play("Enter");
        }
        else
        {
            animationPlayer.Play("Exit");
        }
        if (IsNodeReady())
        {
            var tree = GetTree();
            if (tree != null)
            {
                tree.Paused = Visible;
            }
        }
    }

    public void OnResumePressed()
    {
        Hide();
    }

    public void OnQuitPressed()
    {
        GetNode<Game>("/root/Game").BackToTitle();
    }
}
