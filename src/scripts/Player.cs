using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	public string UserId;

	public bool IsLocalPlayer;

	[Export] Camera2D playercam;


    public override void _Ready()
    {
        base._Ready();

		//playercam.Enabled = IsLocalPlayer;
    }

    public override void _PhysicsProcess(double delta)
	{
		if (!IsLocalPlayer) return;

		Vector2 velocity = Velocity;
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Y = direction.Y * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}


	
}
