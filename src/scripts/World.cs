using Godot;
using System;
using Nakama;
using System.Linq;
using Godot.Collections;

public partial class World : Node2D
{

	[Export] Control hud;
	[Export] PackedScene playerscene;

	public Client Client;
	private ISession Session;
	private static ISocket Socket;
	private static IMatch Match; // current lobby pretty much

	public bool IsHost { get; private set; } // match host p2p

	private Dictionary<string, Player> playerInstances = new Dictionary<string, Player>(); 


	public override void _Ready()
	{
		GD.Print("Hello World");

		if (Client != null)
		{
			QueueFree();
		}

        Client = new Client("http", "127.0.0.1", 7350, "defaultkey");
        Client.Timeout = 500;
	}

	public async void AuthenticateUser(string email, string password, string name)
    {
        try
        {
            Session = await Client.AuthenticateEmailAsync(email, password, name);
            GD.Print("User authenticated, session ID: ", Session.UserId);
        }
        catch (Exception e)
        {
            GD.PrintErr("Authentication failed: ", e.Message);
        }
    }

	private void AddPlayer(string userId, string userName){

		var player = playerscene.Instantiate<Player>();

		player.UserId = userId;
		player.Name = userName;
		player.IsLocalPlayer = userId == Session.UserId;
		playerInstances[userId] = player;

		CallDeferred("add_child", player);

	}

	private void _on_p_1_pressed(){
		AuthenticateUser("user@user.com", "Password123!", "tester");
	}

	private void _on_p_2_pressed(){
		AuthenticateUser("user1@user.com", "Password123!", "tester2");
	}

	private async void _on_join_pressed(){
		Socket = Nakama.Socket.From(Client);
		await Socket.ConnectAsync(Session);

		Match = await Socket.CreateMatchAsync("servertest");
		GD.Print("Joined " , Match.Id);

		Socket.ReceivedMatchPresence += onMatchPresence;

		if (!Match.Presences.Any()){
			IsHost = true;
		}

		AddPlayer(Session.UserId, Session.UserId);
		hud.Visible = false;
	}

    private void onMatchPresence(IMatchPresenceEvent @event)
    {
        foreach(var presence in @event.Joins){

			AddPlayer(presence.UserId, presence.UserId);
			GD.Print("new player joined");
		}
    }

}
