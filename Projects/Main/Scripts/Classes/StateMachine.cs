using Godot;
using System;
using System.Runtime.InteropServices;

[GlobalClass]
public partial class StateMachine : Node
{
    public const int keepCurrent = -1;

    [Export]
    public Callable GetNextstate;
    [Export]
    public Callable TransitionState;
    [Export]
    public Callable TickPhsics;
    [Export]
    public Node2D owner;

    private int currentState = -1;

    public int CurrentState
    {
        get { return currentState; }
        set
        {
            TransitionState.Call(currentState, value);

            currentState = value;
        }
    }

    public override async void _Ready()
    {
        await ToSignal(owner,"ready");

        CurrentState = 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        while (true)
        {
            int nextState = 0;

            nextState = (int)GetNextstate.Call(CurrentState);
            
            if (nextState == keepCurrent)
            {
                break;
            }
            CurrentState = nextState;
        }

        TickPhsics.Call(CurrentState, delta);
    }
}
