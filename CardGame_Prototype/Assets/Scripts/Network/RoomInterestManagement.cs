using System.Collections.Generic;
using Mirror;

public class RoomInterestManagement : InterestManagement
{
    private readonly Dictionary<NetworkIdentity, Room> _identityToRoom = new Dictionary<NetworkIdentity, Room>();

    public void RegisterRoom(NetworkIdentity identity, Room room)
    {
        if (!_identityToRoom.ContainsKey(identity))
        {
            _identityToRoom.Add(identity, room);
        }
    }

    public void UnregisterRoom(NetworkIdentity identity)
    {
        _identityToRoom.Remove(identity);
    }

    public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnectionToClient newObserver)
    {
        if (_identityToRoom.TryGetValue(identity, out Room room))
        {
            return room.connections.Contains(newObserver);
        }
        return false;
    }

    public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnectionToClient> newObservers)
    {
        if (_identityToRoom.TryGetValue(identity, out Room room))
        {
            foreach (NetworkConnectionToClient conn in room.connections)
            {
                newObservers.Add(conn);
            }
        }
    }
}