using Godot;
using System;

public partial class Lobbyuser : Label
{
	[Export] Sprite2D readycheck;

    public bool ReadyToStart { get; set; } = false;

    public override void _Ready()
    {
        base._Ready();
		readycheck.Visible = ReadyToStart;
    }

	public void ToggleReady(){
		readycheck.Visible = !ReadyToStart;
	}
	
	public void SetPlayerName(string text){
		Text = text;
	}

}
