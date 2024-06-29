using Godot;
using System;

[GlobalClass]
public partial class Hurtbox : Area2D
{
    [Signal]
    public delegate void HurtEventHandler(Hitbox hitbox);
}
