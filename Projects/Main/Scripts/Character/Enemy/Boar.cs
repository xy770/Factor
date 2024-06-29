using Godot;
using System;
using System.Linq.Expressions;

public partial class Boar : Enemy
{
    public enum State
    {
        Idle,
        Walk,
        Run,
        Hurt,
        Dying
    }

    [Export]
    public RayCast2D playerChecker;
    [Export]
    public RayCast2D wallChecker;
    [Export]
    public RayCast2D floorChecker;
    [Export]
    public Timer calmdownTimer;
    [Export]
    public Timer IdleTimer;

    public override void _Ready()
    {
        stateMachine.GetNextstate = new Callable(this, MethodName.GetNextState);
		stateMachine.TransitionState = new Callable(this, MethodName.TransitionState);
		stateMachine.TickPhsics = new Callable(this, MethodName.TickPhysics);

        defaultDirection = Direction.Left;
    }

    public int GetNextState(State state)
    {
        if (stats.health == 0)
        {
            if (state == State.Dying)
            {
                return StateMachine.keepCurrent;
            }
            else
            {
                return (int)State.Dying;
            }
        }
        if (pendingDamage != null)
        {
            return (int)State.Hurt;
        }

        switch (state)
        {
            case State.Idle:
                if (IdleTimer.IsStopped())
                {
                    return (int)State.Walk;
                }
                if (playerChecker.IsColliding())
                {
                    return (int)State.Run;
                }
                break;
            case State.Walk:
                if (wallChecker.IsColliding() || !floorChecker.IsColliding())
                {
                    return (int)State.Idle;
                }
                if (playerChecker.IsColliding())
                {
                    return (int)State.Run;
                }
                break;
            case State.Run:
                if (calmdownTimer.IsStopped())
                {
                    return (int)State.Walk;
                }
                break;
            case State.Hurt:
                if (!animationPlayer.IsPlaying())
                {
                    return (int)State.Run;
                }
                break;
        }

        return (int)StateMachine.keepCurrent;
    }

    public void TransitionState(State from, State to)
    {
        switch (to)
        {
            case State.Idle:
                if (from != State.Idle)
                {
                    IdleTimer.Start();
                }
                animationPlayer.Play("Idle");
                if (wallChecker.IsColliding())
                {
                    FilpDirection();
                }
                floorChecker.ForceRaycastUpdate();
                break;
            case State.Walk:
                animationPlayer.Play("Walk");
                if (!floorChecker.IsColliding())
                {
                    FilpDirection();
                }
                floorChecker.ForceRaycastUpdate();
                break;
            case State.Run:
                calmdownTimer.Start();
                animationPlayer.Play("Run");
                break;
            case State.Hurt:
                animationPlayer.Play("Hit");

                stats.ChangeHealth(-pendingDamage.amount);

                pendingDamage = null;
                break;
            case State.Dying:
                animationPlayer.Play("Die");
                break;
        }
    }

    public void TickPhysics(State state, double delta)
    {
        switch (state)
        {
            case State.Idle:
                Move(0f, delta);
                break;
            case State.Walk:
                Move(MaxSpeed / 3, delta);
                break;
            case State.Run:
                if (wallChecker.IsColliding() || !floorChecker.IsColliding())
                {
                    FilpDirection();
                }
                Move(MaxSpeed, delta);
                break;
            case State.Hurt:
                Move(0f, delta);
                break;
            case State.Dying:
                Move(0f, delta);
                break;
        }
    }
}
