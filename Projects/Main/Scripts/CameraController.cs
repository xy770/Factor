using Godot;
using System;

public partial class CameraController : Camera2D
{
    [Export]
    public int strength = 0;
    [Export]
    public float recoverySpeed = 16f;
    Random random = new Random();

    public override void _Ready()
    {
        GetNode<Game>("/root/Game").CameraShouldShake += OnShake;
    }

    public override void _Process(double delta)
    {
        Offset = new Vector2(random.Next(-strength, strength + 1), random.Next(-strength, strength + 1));
        strength = (int)Game.MoveToward(strength, 0, recoverySpeed * (float)delta);
    }

    public static float MoveToward(float from,float to,float delta)
    {
        Vector2 s = new Vector2(from, 0);
        s = s.MoveToward(new Vector2(to, 0), delta);
        return s.X;
    }

    public void OnShake(int amount)
    {
        strength += amount;
    }
}
