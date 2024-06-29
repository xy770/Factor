using Godot;
using System;

public partial class Sword : Weapon
{
    [Export]
    public Sprite2D swordEnergy;

    [Export]
    AnimationPlayer swordSetter;
    
    public SwordType swordType;

    public enum SwordType
    {
        Gold,
        Ice,
        Magic,
        Ordinary
    }

    public override void _Ready()
    {
        Random random = new Random();

        var type = (SwordType)random.Next(0, 4);

        SetSword(type);
        //SetSword(SwordType.Gold);
    }

    private void SetSword(SwordType type)
    {
        swordType = type;
        swordSetter.Play(type.ToString());
    }
}
