using Godot;
using System;

[GlobalClass]
public partial class Weapon : Node2D
{
    [Export]
    public Sprite2D weaponSprite;

    [Export]
    public int baseHurt;
}
