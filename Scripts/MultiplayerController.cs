using Godot;
using System;

//THE HOLY GRAIL: https://www.youtube.com/watch?v=MOJVjFngOs4&t=4829s
// IN ALL ITS GLORY
public partial class MultiplayerController : Control
{
	[Export]
	private int port = 8910;

	[Export]
	private string address = "127.0.0.1";

	[Export]
	public PackedScene startScene;

	[Export]
	private LineEdit nameInput;

	private ENetMultiplayerPeer peer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
	}

    private void ConnectionFailed()
    {
        GD.Print("Server Connection Failed :(");
    }

    private void ConnectedToServer()
    {
        GD.Print("Server Connection Established :)");
		RpcId(1,nameof(sendPlayerInfo), nameInput.Text, Multiplayer.GetUniqueId());
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Player Disconnected: " + id.ToString());
    }

	private void PeerConnected(long id)
    {
        GD.Print("Player Connected: " + id.ToString());
    }

	public void _on_host_button_down(){ 
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, 4);
		if (error != Error.Ok) {
			GD.Print("Hosting has failed: " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Success! Waiting For Players <3");
		sendPlayerInfo(nameInput.Text, 1);
	}

	public void _on_join_button_down(){ 
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(address, port);
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Success! Joining game...");
	}

	public void _on_start_button_down(){ 
		Rpc(nameof(StartGame));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true,TransferMode =MultiplayerPeer.TransferModeEnum.Reliable)]
	private void StartGame() {
		foreach (var item in GameManager.Players) {
			GD.Print(item.Name + " joined the game!");
		}
		var scene = startScene.Instantiate<Node3D>();
		GetTree().Root.AddChild(scene);
		this.Hide();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void sendPlayerInfo(string name, int id){
		PlayerInfo pInfo = new PlayerInfo(){
			Name = name,
			Id = id
		};
		if (!GameManager.Players.Contains(pInfo)) {
			GameManager.Players.Add(pInfo);
		}

		if (Multiplayer.IsServer()) {
			foreach (var item in GameManager.Players) {
				Rpc(nameof(sendPlayerInfo), item.Name, item.Id);
			}
		}
	}
}
