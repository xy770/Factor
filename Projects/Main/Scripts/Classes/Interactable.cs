using Godot;
using System;

[GlobalClass]
public partial class Interactable : Area2D
{
    [Signal]
    public delegate void InteractedEventHandler();

    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 0;
        SetCollisionMaskValue(2,true);

        Connect("body_entered", new Callable(this, nameof(OnBodyEnterd)));
        Connect("body_exited", new Callable(this, nameof(OnBodyExited)));
    }

    public virtual void Interact()
    {
        EmitSignal("Interacted");
    }

    public void OnBodyEnterd(PlayerController player)
    {
        player.RegisterInteractable(this);
    }

    public void OnBodyExited(PlayerController player)
    {
        player.UnregisterInteractable(this);
    }
}
