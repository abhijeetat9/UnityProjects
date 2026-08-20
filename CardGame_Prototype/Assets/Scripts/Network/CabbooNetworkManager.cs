using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CabbooNetworkManager : NetworkManager
{
    private int _nextSeatIndex = 0;
    public static int ConnectedPlayerCount;
    private readonly Dictionary<int, int> _connectionSeats = new Dictionary<int, int>();
    
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject playerObject = Instantiate(playerPrefab);
        NetworkPlayer networkPlayer = playerObject.GetComponent<NetworkPlayer>();
        networkPlayer.SeatIndex = _nextSeatIndex;
        if (_connectionSeats.TryGetValue(conn.connectionId, out int existingSeat))
        {
            networkPlayer.SeatIndex = existingSeat;
        }
        else
        {
            networkPlayer.SeatIndex = _nextSeatIndex;
            _connectionSeats[conn.connectionId] = _nextSeatIndex;
            Debug.Log($"OnServerAddPlayer: connection {conn.connectionId} -> seat {_nextSeatIndex}");
            _nextSeatIndex++;
        }
        NetworkServer.AddPlayerForConnection(conn, playerObject);
    }

    public void StartGame()
    {
        if(!NetworkServer.active)return;
        ConnectedPlayerCount = NetworkServer.connections.Count;
        ServerChangeScene("MainScene");
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName == "MainScene")
        {
            GameController gameController = new GameController(RoundSetup.CreateGameState(ConnectedPlayerCount)
            );
            NetworkGameController networkGameController = FindAnyObjectByType<NetworkGameController>();
            networkGameController.Initialize(gameController); 
        }
    }
}