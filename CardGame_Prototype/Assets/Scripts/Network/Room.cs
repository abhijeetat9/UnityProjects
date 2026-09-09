using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Room
{
    public string _inviteCode;
    public NetworkConnectionToClient _hostConnection;
    public string _hostUserId;
    
    public List<NetworkConnectionToClient> connections = new List<NetworkConnectionToClient>();

    public GameObject gameRoomInstance;

    public Room(NetworkConnectionToClient hostConn, string inviteCode)
    {
        _hostConnection = hostConn;
        _inviteCode = inviteCode;
        connections.Add(hostConn);
    }

}