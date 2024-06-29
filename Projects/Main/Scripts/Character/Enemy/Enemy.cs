using Godot;
using System;
using System.Threading;

[GlobalClass]
public partial class Enemy : CharacterBody2D
{
    public enum Direction
    {
        Left = -1,
        Right = 1
    }

    [Export]
    private Direction direction = Direction.Left;
    [Export]
    public Direction defaultDirection;

    [Export]
    public float MaxSpeed = 180f;
    [Export]
    public float Acceleration = 2000f;
    [Export]
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    [Export]
    public int damageAmount;
    [Export]
    public int knockbackAmount;

    public Vector2 velocity;

    [Export]
    public Node2D graphics;
    [Export]
    public AnimationPlayer animationPlayer;
    [Export]
    public StateMachine stateMachine;
    [Export]
    public Stats stats;

    public Damage pendingDamage;

    public override void _Ready()
    {
        graphics = GetNode<Node2D>("Graphics");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        stateMachine = GetNode<StateMachine>("StateMachine");

        stats = GetNode<Stats>("Stats");

        AddToGroup("Enemies");
    }

    public void Move(float Speed, double delta)
    {
        velocity.X = Speed * (float)GetDirection();
        if (IsOnFloor())
        {
            velocity.Y = 0;
        }
        else
        {
            velocity.Y += gravity * (float)delta;
        }

        Velocity = velocity;

        MoveAndSlide();
    }

    public void Fly(Vector2 vel, double delta)
    {
        velocity.X = vel.X * GetDirectionInt();
        velocity.Y = vel.Y;

        Velocity = velocity;

        MoveAndSlide();
    }

    public void SetDirection(Direction d)
    {
        if (direction != d)
        {
            direction = d;
            graphics.Scale = new Vector2((int)defaultDirection * (float)direction, graphics.Scale.Y);
        }
    }

    public void FilpDirection()
    {
        if (GetDirection() == Direction.Left)
        {
            SetDirection(Direction.Right);
        }
        else
        {
            SetDirection(Direction.Left);
        }
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
        else
        {
            return 1;
        }
    }

    public void Die()
    {
        QueueFree();
    }

    public void OnHurtBoxHurt(Hitbox hit)
    {
        pendingDamage = new Damage();
        pendingDamage.source = hit.GetOwner<PlayerController>();
        pendingDamage.amount = hit.GetOwner<PlayerController>().currentWeapon.baseHurt;
    }

    public EnemyInfo GetEnemyInfo()
    {
        return new EnemyInfo
        {
            name = Name,
            statInfo = stats.GetStatInfo(),
            direction = GetDirectionInt(),
            posx = GlobalPosition.X,
            posy = GlobalPosition.Y
        };
    }

    public async void SetEnemyInfo(EnemyInfo info)
    {
        if(info.statInfo.health == 0)
        {
            QueueFree();
            return;
        }
        
        await ToSignal(this, "ready");
        
        stats.SetStatsInfo(info.statInfo);
        SetDirection((Direction)info.direction);
        GlobalPosition = new Vector2(info.posx, info.posy);
    }
}

public class EnemyInfo
{
    public string name{ get; set; }
    public StatsInfo statInfo{ get; set; }
    public int direction{ get; set; }
    public float posx{ get; set; }
    public float posy{ get; set; }
}