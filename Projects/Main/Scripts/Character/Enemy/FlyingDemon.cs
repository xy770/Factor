using Godot;
using Godot.NativeInterop;
using System;

public partial class FlyingDemon : Enemy
{
	public enum State
	{
		Idle,
		Flying,
		Attack,
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
	public Timer attackTimer;

	[Export]
	public Node2D projectilePos;

	public float flyingSpeed = 200f;

	private Random random = new();
	PackedScene bullet = GD.Load<PackedScene>("res://Prefabs/Character/Enemy/Projectile/DemonFire.tscn");

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
				if (attackTimer.TimeLeft == 0)
				{
					return (int)State.Attack;
				}
				if(floorChecker.IsColliding())
				{
					return (int)State.Flying;
				}
				break;
			case State.Flying:
				if (attackTimer.TimeLeft == 0)
				{
					return (int)State.Attack;
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
					return (int)State.Attack;
				}
				break;
		}

		return (int)StateMachine.keepCurrent;
	}

	public void TransitionState(State from, State to)
	{
		if (from == State.Attack)
		{
			FilpDirection();
		}

		switch (to)
		{
			case State.Idle:
				animationPlayer.Play("Idle");
				break;
			case State.Flying:
				animationPlayer.Play("Flying");
				break;
			case State.Attack:
				attackTimer.Start(random.Next(3, 7));
				animationPlayer.Play("Attack");
				break;
			case State.Hurt:
				animationPlayer.Play("Hit");

				stats.ChangeHealth(-pendingDamage.amount);

				pendingDamage = null;
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
				Fly(new Vector2(0f, 0f), delta);
				break;
			case State.Flying:
				if(IsOnCeiling())
				{
					flyingSpeed = Math.Abs(flyingSpeed);
				}
				if(IsOnFloor())
				{
					flyingSpeed = -Math.Abs(flyingSpeed);
				}
				
				Fly(new Vector2(MaxSpeed, flyingSpeed), delta);
				break;
			case State.Attack:
				Fly(new Vector2(0f, 0f), delta);
				break;
			case State.Hurt:
				Move(0f, delta);
				break;
			case State.Dying:
				Move(0f, delta);
				break;
		}
	}

	public void Shoot()
	{
		var instance = bullet.Instantiate<Projectile>();
		GetTree().CurrentScene.AddChild(instance);

		instance.GlobalPosition = projectilePos.GlobalPosition;
		instance.Track(GetTree().CurrentScene.GetNode<Node2D>("Player"));
	}
}
