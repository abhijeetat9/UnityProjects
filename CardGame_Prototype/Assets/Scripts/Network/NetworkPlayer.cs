using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar] public int SeatIndex = -1;
    [SyncVar (hook = nameof(OnPlayerNameChanged))] public string PlayerName;
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"I am seat {SeatIndex}");
        CmdSetPlayerName($"Player {SeatIndex + 1}");
    }
    
    [Command]
    private void CmdSetPlayerName(string name)
    {
        PlayerName = name;
    }

    private void OnPlayerNameChanged(string oldName, string newName)
    {
        Debug.Log($"Seat {SeatIndex} name synced {newName}");
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.RefreshUI();
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.RegisterPlayer(this);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.UnRegisterPlayer(this);
        }
    }
}
