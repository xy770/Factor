using Godot;
using System;

[GlobalClass]
public partial class Damage : RefCounted
{
    public int amount;
    public Node2D source;
}
