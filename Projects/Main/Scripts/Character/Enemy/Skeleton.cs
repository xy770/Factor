using Godot;
using System;
using System.ComponentModel;

public partial class Skeleton : Enemy
{
    [Export]
    public NavigationAgent2D navigationAgent;
    [Export]
    public Timer navigationTimer;

    public PlayerController player;
    [Export]
    public RayCast2D playerChecker;
    [Export]
    public RayCast2D wallChecker;
    [Export]
    public RayCast2D floorChecker;
    [Export]
    public RayCast2D attackChecker;

    [Export]
    public Timer waitTimer;

    public enum State
    {
        Idle,
        Walk,
        Hurt,
        Attack,
        Dying,
        Catch
    }

    public override void _Ready()
    {
        stateMachine.GetNextstate = new Callable(this, MethodName.GetNextState);
        stateMachine.TransitionState = new Callable(this, MethodName.TransitionState);
        stateMachine.TickPhsics = new Callable(this, MethodName.TickPhysics);

        player = GetTree().CurrentScene.GetNode<PlayerController>("Player");

        defaultDirection = Direction.Right;
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
                if (waitTimer.TimeLeft == 0)
                {
                    return (int)State.Walk;
                }
                if (playerChecker.IsColliding() && !wallChecker.IsColliding() && floorChecker.IsColliding())
                {
                    return (int)State.Catch;
                }
                break;
            case State.Walk:
                if (playerChecker.IsColliding() && !wallChecker.IsColliding() && floorChecker.IsColliding())
                {
                    return (int)State.Catch;
                }
                if (waitTimer.TimeLeft > 0)
                {
                    return (int)State.Idle;
                }
                break;
            case State.Attack:
                if (!animationPlayer.IsPlaying())
                {
                    return (int)State.Idle;
                }
                break;
            case State.Hurt:
                if (!animationPlayer.IsPlaying())
                {
                    return (int)State.Catch;
                }
                break;
            case State.Catch:
                if (attackChecker.IsColliding())
                {
                    return (int)State.Attack;
                }
                if (wallChecker.IsColliding() || !floorChecker.IsColliding())
                {
                    return (int)State.Walk;
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
                    waitTimer.Start();
                }

                animationPlayer.Play("Idle");
                break;
            case State.Walk:
                animationPlayer.Play("Walk");
                break;
            case State.Hurt:
                animationPlayer.Play("Hit");

                stats.ChangeHealth(-pendingDamage.amount);

                pendingDamage = null;
                break;
            case State.Attack:
                animationPlayer.Play("Attack");
                break;
            case State.Dying:
                animationPlayer.Play("Death");
                break;
            case State.Catch:
                animationPlayer.Play("Walk");
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
                if (wallChecker.IsColliding() || !floorChecker.IsColliding())
                {
                    FilpDirection();
                }
                Move(MaxSpeed, delta);
                break;
            case State.Hurt:
                Move(0f, delta);
                break;
            case State.Attack:
                Move(0f, delta);
                break;
            case State.Dying:
                Move(0f, delta);
                break;
            case State.Catch:
                var movement = ToLocal(navigationAgent.GetNextPathPosition());
                if (Math.Abs(movement.X) < 5)
                {
                    Move(MaxSpeed / 5, delta);
                }
                else if (movement.X != 0)
                {
                    SetDirection((Direction)(movement.X > 0 ? 1 : -1));
                    Move(MaxSpeed, delta);
                }
                break;
        }
    }

    public void OnNavigationTimerTimeOut()
    {
        navigationAgent.TargetPosition = player.Position;
    }
}
