using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private GameObject startGameButton;

    private readonly List<NetworkPlayer> _connectedPlayers = new List<NetworkPlayer>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterPlayer(NetworkPlayer player)
    {
        _connectedPlayers.Add(player);
        RefreshUI();
    }
    
    public void UnRegisterPlayer(NetworkPlayer player)
    {
        _connectedPlayers.Remove(player);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (playerListText == null || startGameButton == null) return;
        StringBuilder sb = new StringBuilder();
        foreach (var player in _connectedPlayers)
        {
            sb.AppendLine($"Seat {player.SeatIndex}: {player.PlayerName}");
        }
        playerListText.text = sb.ToString();
        startGameButton.SetActive(Mirror.NetworkServer.active);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}