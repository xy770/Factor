using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;

public partial class PlayerController : CharacterBody2D
{
	public enum Direction
	{
		Left = -1,
		Right = 1
	}
	public enum State
	{
		Idle,
		Runing,
		Jump,
		Fall,
		WallSliding,
		Attack,
		Hurt,
		Dying
	}

	// Player运动常量
	[Export]
	public const float Speed = 6000.0f;
	public const float acceleration = Speed / 0.2f;
	[Export]
	public const float MaxFallSpeed = 8000.0f;
	[Export]
	public const float JumpVelocity = 170.0f;
	[Export]
	public Vector2 wallJumpVelocity = new Vector2(-4000.0f, 1700.0f);
	[Export]
	public const float ExtraJumpVelocity = 120.0f;
	[Export]
	public const float JumpIntervalTime = 0.1f;
	public const int MaxJumpTime = 2;
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	public int knockbackAmount;
	public List<Interactable> interactableWith = new();

	// Player运动状态
	[Export]
	public Vector2 velocity = Vector2.Zero;
	private int JumpTime = 0;
	private bool CanJump = true;
	private bool canLanding = false;
	[Export]
	Direction direction;
	[Export]
	public Weapon currentWeapon;

	// 其他
	private Timer JumpInterval; //控制跳跃间隔
	[Export]
	public Timer TeleportationTimer;
	[Export]
	Timer InvincibleTimer;
	[Export]
	public RayCast2D FloorChecker;
	[Export]
	public RayCast2D handChecker;
	[Export]
	public RayCast2D footChecker;
	[Export]
	public AnimationPlayer animationPlayer;
	[Export]
	public Node2D playerSprite;
	public Damage pendingDamage;
	[Export]
	public Node2D graphics;
	[Export]
	public Stats stats;
	[Export]
	public AnimatedSprite2D interactionIcon;
	public Game game;
	[Export]
	public StateMachine stateMachine;
	[Export]
	public pause_screen pause_Screen;

	public override void _Ready()
	{
		game = GetNode<Game>("/root/Game");
		stats = game.playerStats;
		JumpInterval = new Timer
		{
			WaitTime = JumpIntervalTime
		};

		JumpInterval.OneShot = true;
		JumpInterval.Connect("timeout", new Callable(this, "OnJumpIntervalTimeout"));

		AddChild(JumpInterval);

		stateMachine.GetNextstate = new Callable(this, MethodName.GetNextState);
		stateMachine.TransitionState = new Callable(this, MethodName.TransitionState);
		stateMachine.TickPhsics = new Callable(this, MethodName.TickPhysics);
	}

	public void OnJumpIntervalTimeout()
	{
		CanJump = true;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("pause"))
		{
			pause_Screen.Visible = true;
		}
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

		Vector2 movement = new Vector2(Input.GetAxis("move_left", "move_right"), 0);
		bool isStill = movement.X == 0;

		if (Input.IsActionJustPressed("jump"))
		{
			if (JumpTime < MaxJumpTime && CanJump)
			{
				if (state == State.Jump)
				{
					return StateMachine.keepCurrent;
				}
				return (int)State.Jump;
			}
			if (state == State.WallSliding && CanJump)
			{
				JumpTime = -1;
				return (int)State.Jump;
			}
		}

		switch (state)
		{
			case State.Idle:
				if (!IsOnFloor())
				{
					return (int)State.Fall;
				}
				if (Input.IsActionJustPressed("attack"))
				{
					return (int)State.Attack;
				}
				if (!isStill)
				{
					return (int)State.Runing;
				}
				break;
			case State.Runing:
				if (!IsOnFloor() && isStill)
				{
					return (int)State.Fall;
				}
				if (Input.IsActionJustPressed("attack"))
				{
					return (int)State.Attack;
				}
				if (IsOnFloor() && isStill)
				{
					return (int)State.Idle;
				}
				break;
			case State.Jump:
				if (velocity.Y >= 0 || IsOnCeiling())
				{
					return (int)State.Fall;
				}
				if (Input.IsActionJustPressed("attack"))
				{
					return (int)State.Attack;
				}
				break;
			case State.Fall:
				if (IsOnWall() && handChecker.IsColliding() && footChecker.IsColliding())
				{
					return (int)State.WallSliding;
				}
				if (IsOnFloor())
				{
					return (int)State.Idle;
				}
				if (Input.IsActionJustPressed("attack"))
				{
					return (int)State.Attack;
				}
				if (!isStill)
				{
					return (int)State.Runing;
				}
				break;
			case State.WallSliding:
				if (IsOnFloor())
				{
					return (int)State.Idle;
				}
				if (!IsOnWall())
				{
					return (int)State.Fall;
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
					return (int)State.Idle;
				}
				break;
		}
		return (int)StateMachine.keepCurrent;
	}

	public void TransitionState(State from, State to)
	{
		if (from == State.Attack)
		{
			GetNode<Sprite2D>("Graphics/CompositeSprites/Sword/SwordEnergy").Visible = false;
			GetNode<CollisionShape2D>("Hitbox/CollisionShape2D").Disabled = true;
		}

		switch (to)
		{
			case State.Idle:
				animationPlayer.Play("Idle");
				break;
			case State.Runing:
				animationPlayer.Play("Runing");
				break;
			case State.Jump:
				animationPlayer.Play("Jump");
				if (JumpTime == 0)
				{
					velocity.Y = -JumpVelocity;
				}
				else if (JumpTime == -1)
				{
					velocity.Y = JumpVelocity / 2;
				}
				else
				{
					velocity.Y = -ExtraJumpVelocity;
				}

				JumpTime++;
				CanJump = false;
				JumpInterval.Start();
				break;
			case State.Fall:
				canLanding = true;
				velocity.Y = 0;
				if (velocity.X == 0)
					animationPlayer.Play("Landing");
				break;
			case State.WallSliding:
				animationPlayer.Play("Wall_Sliding");
				CanJump = true;
				break;
			case State.Attack:
				GetNode<Sprite2D>("Graphics/CompositeSprites/Sword/SwordEnergy").Visible = true;
				GetNode<CollisionShape2D>("Hitbox/CollisionShape2D").Disabled = false;
				animationPlayer.Play("Attack");

				Velocity = Vector2.Zero;

				break;
			case State.Hurt:
				game.ShakeCamera(3);

				Invincible(2, true);

				animationPlayer.Play("Hurt");

				var dir = pendingDamage.source.GlobalPosition.DirectionTo(GlobalPosition);
				velocity = dir.Normalized() * knockbackAmount;
				velocity.Y /= 3;

				stats.ChangeHealth(-pendingDamage.amount);

				pendingDamage = null;

				break;
			case State.Dying:
				interactableWith.Clear();

				InvincibleTimer.Stop();
				animationPlayer.Play("Die");

				pendingDamage = null;

				break;
		}
	}

	public void TickPhysics(State state, double delta)
	{
		interactionIcon.Visible = interactableWith.Count != 0;

		if (Input.IsActionJustPressed("interact") && interactableWith.Count != 0)
		{
			interactableWith.Last().Interact();
		}

		if (InvincibleTimer.TimeLeft > 0 && (bool)InvincibleTimer.GetMeta("effect"))
		{
			graphics.Modulate = new Color(graphics.Modulate.R, graphics.Modulate.B, graphics.Modulate.B, Mathf.Sin(Time.GetTicksMsec() / 30) * 0.5f + 0.5f);
		}
		else
		{
			graphics.Modulate = new Color(graphics.Modulate.R, graphics.Modulate.B, graphics.Modulate.B, 1);
		}

		switch (state)
		{
			case State.Idle:
				Move(gravity, delta);
				break;
			case State.Runing:
				Move(gravity, delta);
				break;
			case State.Jump:
				Move(gravity, delta);
				break;
			case State.Fall:
				if (canLanding && FloorChecker.IsColliding())
				{
					animationPlayer.Play("Landing");
					canLanding = false;
				}
				Move(gravity, delta);
				break;
			case State.WallSliding:
				Move(gravity / 3, delta);
				break;
			case State.Attack:
				GD.Print(Velocity);
				MoveAndSlide();
				break;
			case State.Hurt:
				Velocity = velocity;
				MoveAndSlide();
				break;
			case State.Dying:
				HandleGravity(gravity, delta);
				break;
		}
	}

	public void OnHurtBoxHurt(Hitbox hit)
	{
		if (InvincibleTimer.TimeLeft > 0)
		{
			return;
		}
		pendingDamage = new Damage();
		pendingDamage.source = hit.GetOwner<Node2D>();
		pendingDamage.amount = hit.damageAmount;
		knockbackAmount = hit.knockbackAmount;
	}

	public void Move(float gravity, double delta)
	{
		if (!IsOnFloor())
		{
			velocity.Y += gravity * (float)delta;

			if (velocity.Y > MaxFallSpeed * (float)delta)
			{
				velocity.Y = MaxFallSpeed * (float)delta;
			}
		}
		else if (velocity.Y > 0)
		{
			velocity.Y = 0;

			JumpTime = 0;
		}

		Vector2 movement = new Vector2(Input.GetAxis("move_left", "move_right"), 0);

		velocity.X = Game.MoveToward(velocity.X, movement.X * Speed * (float)delta, acceleration * (float)delta);

		Velocity = velocity;

		if (movement.X != 0)
		{
			SetDirection(movement.X > 0 ? 1 : -1);
		}

		MoveAndSlide();
	}

	private void HandleGravity(float gravity, double delta)
	{
		if (!IsOnFloor())
		{
			velocity.Y += gravity * (float)delta;

			if (velocity.Y > MaxFallSpeed * (float)delta)
			{
				velocity.Y = MaxFallSpeed * (float)delta;
			}
		}
		else if (velocity.Y > 0)
		{
			velocity.Y = 0;
		}

		velocity.X = 0;

		Velocity = velocity;

		MoveAndSlide();
	}

	public void RegisterInteractable(Interactable interactable)
	{
		if (interactableWith.Contains(interactable))
		{
			return;
		}
		interactableWith.Add(interactable);
	}

	public void UnregisterInteractable(Interactable interactable)
	{
		interactableWith.Remove(interactable);
	}

	public void Die()
	{
		game.BackToTitle();
	}

	public void SetDirection(int dir)
	{
		playerSprite.Scale = new Vector2(dir, 1);
		direction = (Direction)dir;
	}

	public Direction GetDirection()
	{
		return direction;
	}

	public int GetDirectionInt()
	{
		if (direction == Direction.Left)
		{
			return -1;
		}
		return 1;
	}

	public void Teleportation(float speed, float time)
	{
		velocity.X = speed * GetDirectionInt();
		velocity.Y = 0;
		Velocity = velocity;

		TeleportationTimer.WaitTime = time;
		TeleportationTimer.OneShot = true;

		TeleportationTimer.Start();
	}

	public void TeleportationTimeout()
	{
		velocity = Vector2.Zero;
		Velocity = velocity;
	}

	public void Invincible(float time, bool effect)
	{
		if (InvincibleTimer.TimeLeft > time)
		{
			return;
		}
		InvincibleTimer.WaitTime = time;
		InvincibleTimer.OneShot = true;
		InvincibleTimer.SetMeta("effect", effect);
		InvincibleTimer.Start();
	}

	public PlayerInfo GetPlayerInfo()
	{
		return new PlayerInfo
		{
			name = Name,
			statInfo = stats.GetStatInfo(),
			direction = GetDirectionInt(),
			posx = GlobalPosition.X,
			posy = GlobalPosition.Y
		};
	}

	public async void SetPlayerInfo(PlayerInfo info)
	{
		await ToSignal(this, "ready");

		stats.SetStatsInfo(info.statInfo);
		SetDirection(info.direction);
		GlobalPosition = new Vector2(info.posx, info.posy);
	}
}

public class PlayerInfo
{
	public string name { get; set; }
	public StatsInfo statInfo { get; set; }
	public int direction { get; set; }
	public float posx { get; set; }
	public float posy { get; set; }
}