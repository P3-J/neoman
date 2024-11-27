using Godot;
using System;
using Nakama;
using System.Linq;
using Godot.Collections;
using System.Net;
using Nakama.TinyJson;
using System.Text;
using Newtonsoft.Json;

public partial class World : Node2D
{

	[Export] Control hud;
	[Export] PackedScene playerscene;
	[Export] Camera2D worldcam;
	[Signal] public delegate void MoveSyncEventHandler(string data);
	public static Client Client;
	public static World ClientNode;
	private ISession Session;
	private static ISocket Socket;
	private static IMatch Match; // current lobby pretty much

	public bool IsHost { get; private set; } // match host p2p

	private Dictionary<string, Player> playerInstances = new Dictionary<string, Player>(); 


	public override void _Ready()
	{
		GD.Print("Hello World");

		if (ClientNode != null)
		{
			QueueFree();
		} else {
			ClientNode = this;
		}

        Client = new Client("http", "127.0.0.1", 7350, "defaultkey");
        Client.Timeout = 500;
	}

	public async void AuthenticateUser(string email, string password, string name)
    {
        try
        {
            Session = await Client.AuthenticateEmailAsync(email, password, name, create:false);
            GD.Print("User authenticated, session ID: ", Session.UserId);
        }
        catch (Exception e)
        {
            GD.PrintErr("Authentication failed: ", e.Message);
			GD.PrintErr("extra: ", e.Data.ToJson());
        }
    }

	private void AddPlayer(string userId, string userName){

		var player = playerscene.Instantiate<Player>();

		player.UserId = userId;
		player.Name = userName;

		if (userId == Session.UserId){
			player.IsLocalPlayer = true;
			worldcam.Enabled = false;
		}
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
		Socket.ReceivedMatchState    += onMatchState;

		if (!Match.Presences.Any()){
			IsHost = true;
		}

		AddPlayer(Session.UserId, Session.Username);
		hud.Visible = false;
	}

    private void onMatchPresence(IMatchPresenceEvent @event)
    {
        foreach(var presence in @event.Joins){

			AddPlayer(presence.UserId, presence.Username);
			SyncUpAllPlayers(presence.UserId);
			GD.Print("new player joined: ", presence.Username);
		}
    }

    private void SyncUpAllPlayers(string userId)
    {
        
		foreach (var pInstance in playerInstances.Values){
			if (pInstance.UserId != userId){

				PlayerSyncData syncdata = new PlayerSyncData(){
					Position = pInstance.GlobalPosition,
					id = pInstance.UserId,
					UserName = pInstance.Name,
				};

				SyncData(JsonConvert.SerializeObject(syncdata), 2);
			}
		}
    }

    public static async void SyncData(string data, int opcode){
		await Socket.SendMatchStateAsync(Match.Id, opcode, data);
	}

	private void onMatchState(IMatchState state){
		string data = Encoding.UTF8.GetString(state.State);
		switch (state.OpCode){

			case 1:
				CallDeferred("EmitPlayerSyncDataSignal", data);
				break;
			case 2:
				PlayerSyncData syncdata = JsonConvert.DeserializeObject<PlayerSyncData>(data);
				CallDeferred("AddPlayer", syncdata.id, syncdata.UserName);
				break;
			case 3:
				BulletCreateSyncData bsyncdata = JsonConvert.DeserializeObject<BulletCreateSyncData>(data);
				CallDeferred("InstanceBullet", bsyncdata.Direction, bsyncdata.GlobalPosition, bsyncdata.id);
				break;
		}
	}

	public void InstanceBullet(Vector2 dir, Vector2 globalpos, string id){
		foreach (var pInstance in playerInstances.Values){
			if (pInstance.UserId == id){
				pInstance._FireBullet(dir, globalpos, id);
				break;
			}      
		}
	}

	private void EmitPlayerSyncDataSignal(string data){
		EmitSignal("MoveSync", data);
	}

}
