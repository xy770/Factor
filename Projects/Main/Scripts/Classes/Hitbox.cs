using Godot;
using System;

[GlobalClass]
public partial class Hitbox : Area2D
{
    [Signal]
    public delegate void HitEventHandler(Hurtbox hurtbox);

    [Export]
    public int damageAmount;
    [Export]
    public int knockbackAmount;

    public override void _Ready()
    {
        Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));
    }

    public void OnAreaEntered(Hurtbox hurtbox)
    {
        GD.Print("Hitbox: " + hurtbox.Name + " hit " + Name);
        EmitSignal(SignalName.Hit, hurtbox);
        hurtbox.EmitSignal(Hurtbox.SignalName.Hurt, this);
    }
}
