using Godot;
using System;
using System.Diagnostics.Contracts;

public partial class Goblin : Enemy
{
	[Export]
	public NavigationAgent2D navigationAgent;
	[Export]
	public Timer navigationTimer;
	[Export]
	public RayCast2D wallChecker;
	[Export]
	public RayCast2D floorChecker;

	public PlayerController player;

	public int findRange = 180;

	public enum State
	{
		Idle,
		Run,
		Hurt,
		Attack,
		Dying
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

		var movement = ToLocal(navigationAgent.GetNextPathPosition());

		switch (state)
		{
			case State.Idle:
				if (ToLocal(player.Position).Length() < 30)
				{
					return (int)State.Attack;
				}
				if (Mathf.Abs(movement.X) >= 3 && Mathf.Abs(movement.X) <= findRange)
				{
					if (movement.X > 0)
					{
						SetDirection(Direction.Right);
						if (wallChecker.IsColliding() || !floorChecker.IsColliding())
						{
							return (int)StateMachine.keepCurrent;
						}
					}
					if (movement.X < 0)
					{
						SetDirection(Direction.Left);
						if (wallChecker.IsColliding() || !floorChecker.IsColliding())
						{
							return (int)StateMachine.keepCurrent;
						}
					}
					if (movement.X == 0)
					{
						return (int)StateMachine.keepCurrent;
					}
					return (int)State.Run;
				}
				break;
			case State.Run:
				if (ToLocal(player.Position).Length() < 30)
				{
					return (int)State.Attack;
				}
				if (Mathf.Abs(movement.X) < 3 || Mathf.Abs(movement.X) > findRange)
				{
					return (int)State.Idle;
				}
				if (wallChecker.IsColliding() || !floorChecker.IsColliding())
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
				animationPlayer.Play("Idle");
				break;
			case State.Run:
				animationPlayer.Play("Run");
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
		}
	}

	public void TickPhysics(State state, double delta)
	{
		switch (state)
		{
			case State.Idle:
				Move(0f, delta);
				break;
			case State.Run:
				var movement = ToLocal(navigationAgent.GetNextPathPosition());
				if (movement.Length() != 0)
					SetDirection((Direction)(movement.X > 0 ? 1 : -1));
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
		}
	}

	public void OnNavigationTimerTimeOut()
	{
		navigationAgent.TargetPosition = player.Position;
	}
}
