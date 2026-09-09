using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CabbooNetworkManager : NetworkManager
{
    private readonly Dictionary<string, Room> _rooms = new Dictionary<string, Room>();
    private readonly Dictionary<NetworkConnectionToClient, string> _connectionToRoom = new Dictionary<NetworkConnectionToClient, string>();
    [SerializeField] private GameObject gameRoomPrefab;
    public GameObject GameRoomPrefab => gameRoomPrefab;
    
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        var room = GetRoomForConnection(conn);
        if (room == null) return;
        GameObject playerObject = Instantiate(playerPrefab);
        NetworkPlayer networkPlayer = playerObject.GetComponent<NetworkPlayer>();
        networkPlayer.SeatIndex = room.connections.IndexOf(conn);
        if (conn == room._hostConnection)
        {
            networkPlayer.IsHost = true;
        }
        NetworkServer.AddPlayerForConnection(conn, playerObject);
    }
    
    public void RegisterConnectionToRoom(NetworkConnectionToClient conn, string inviteCode)
    {
        if (!_rooms.TryGetValue(inviteCode, out Room room))
        {
            room = new Room(conn, inviteCode);
            _rooms.Add(inviteCode, room);
        }
        else
        {
            room.connections.Add(conn);
            
        }
        _connectionToRoom[conn] = inviteCode;
        Debug.Log($"{room.connections.Count} connections to {inviteCode}");
    }

    public Room GetRoomForConnection(NetworkConnectionToClient conn)
    {
        if (_connectionToRoom.TryGetValue(conn, out string inviteCode))
        {
            if (_rooms.TryGetValue(inviteCode, out Room room))
            {
                return room;
            }
            else
            {
                Debug.LogWarning($"Connection mapped to inviteCode {inviteCode}, but room  was not found");
            }
        }
        else
        {
            Debug.LogWarning($"Connection {conn.connectionId} is not registered in any room.");
        }

        return null;
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (!_connectionToRoom.TryGetValue(conn, out var inviteCode))
        {
            base.OnServerDisconnect(conn);
            return;
        }
        _connectionToRoom.Remove(conn);

        if (!_rooms.TryGetValue(inviteCode, out Room room))
        {
            base.OnServerDisconnect(conn);
            return;
        }
        room.connections.Remove(conn);
        if (room.connections.Count == 0)
        {
            if (room.gameRoomInstance != null)
            {
                RoomInterestManagement _roomInterestManagement = FindAnyObjectByType<RoomInterestManagement>();
                _roomInterestManagement.UnregisterRoom(room.gameRoomInstance.GetComponent<NetworkIdentity>());
                NetworkServer.Destroy(room.gameRoomInstance); 
            }
            _rooms.Remove(inviteCode);
        }
        base.OnServerDisconnect(conn);
        Debug.Log($"{_connectionToRoom.Count} connections to {inviteCode}");
    }
}