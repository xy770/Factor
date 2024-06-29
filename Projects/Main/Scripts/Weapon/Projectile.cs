using Godot;
using System;

[GlobalClass]
public partial class Projectile : Node2D
{
	public Vector2 velocity = new Vector2();
	[Export]
	public float speed;
	public float distance;
	[Export]
	public int rotationSpeed;

	[Export]
	public AnimationPlayer animationPlayer;
	[Export]
	public Sprite2D projectileSprite;

	private bool isShoted = false;
	private bool isTracked = false;

	private Vector2 acceleration = Vector2.Zero;
	private Node2D target;

	public override void _Process(double delta)
	{
		if (isShoted)
		{
			Move((float)delta);
		}

		if (isTracked)
		{
			MoveByTarget((float)delta);
		}
	}

	public void Shoot()
	{
		isShoted = true;
		animationPlayer.Play("Move");
		Rotation = ((int)GetMeta("SpriteDirection") * velocity).Angle();
	}

	public void Track(Node2D t)
	{
		target = t;
		isTracked = true;
		animationPlayer.Play("Move");
	}

	private void MoveByTarget(float delta)
	{
		var lineVelocity = Vector2.Zero;
		lineVelocity = (target.GlobalPosition - GlobalPosition).Normalized() * speed;
		var rv = (lineVelocity - velocity).Normalized() * rotationSpeed;
		acceleration += rv;
		velocity += acceleration * delta;
		velocity = velocity.Normalized() * speed;
		Rotation = ((int)GetMeta("SpriteDirection") * velocity).Angle();

		Move((float)delta);
	}

	private void Move(float delta)
	{
		GD.Print(Name + velocity);
		GlobalPosition += velocity * delta;
		distance += velocity.Length() * delta;
	}

	public void OnHitboxHit(Hurtbox hit)
	{
		Explode();
	}

	public void BodyEntered(Rid rid, Node2D body, int bodyIndex, int localIndexShape)
	{
		Explode();
	}

	public void Explode()
	{
		animationPlayer.Play("Explode");
		isShoted = false;
		isTracked = false;
	}

	public void Clean()
	{
		QueueFree();
	}
}
