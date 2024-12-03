using Godot;
using System;
using Nakama;
using Newtonsoft.Json;


public partial class World : Node2D
{

    [Export] BoxContainer lobbybox;

    private PackedScene lobbypacked = GD.Load<PackedScene>("res://scenes/lobbyuser.tscn");

    private void AddPlayerToLobby(string userid, string username)
    {

        if (userInstances.ContainsKey(userid)) { return;}

        Lobbyuser listobj = lobbypacked.Instantiate<Lobbyuser>();
        listobj.SetPlayerName(username);
        userInstances[userid] = username;
        lobbybox.CallDeferred("add_child", listobj);
    }

    private void SyncLobby(string userId)
    {

		foreach (var user in userInstances){
			if (user.Key != userId ){
				lobbysyncdata syncdata = new lobbysyncdata(){
					USERID = user.Key,
					USERNAME = user.Value,
				};
				SyncData(JsonConvert.SerializeObject(syncdata), 4);
                GD.Print("sent sync");
			}
		}
    }


    private void _on_readybutton_pressed(){

        if (!IsHost){return;}

        foreach (var user in userInstances)
        {
            AddPlayer(user.Key, user.Value);
            SyncUpAllPlayers(user.Key);
        }
        
    }


}