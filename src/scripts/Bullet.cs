using Godot;
using System;

public partial class Bullet : Area2D
{
	public Vector2 dir;
	public int speed = 150;

	public override void _Process(double delta)
	{
		GlobalPosition += dir * speed * (float)delta;
	}
}
