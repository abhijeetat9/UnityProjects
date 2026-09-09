using System;
using System.Collections.Generic;
using System.Text;
using Mirror;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject lobbyEntryPanel;
    [SerializeField] private GameObject startGamePanel;
    
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
        if (NetworkClient.localPlayer != null)
        {
            if (NetworkClient.localPlayer.GetComponent<NetworkPlayer>().IsHost)
            {
                startGameButton.SetActive(true);
            }
            else
            {
                startGameButton.SetActive(false);
            }
        }
        else
        {
            startGameButton.SetActive(false);
        }
    }

    public void OnStartGameClicked()
    {
        if (NetworkClient.localPlayer == null)
        {
            Debug.Log("OnStartGameClicked: no local player found yet");
            return;
        }
        NetworkClient.localPlayer.GetComponent<NetworkPlayer>().CmdStartGame();
    }
    
    public void HideLobbyUI()
    {
        if (lobbyEntryPanel != null) lobbyEntryPanel.SetActive(false);
        if (startGamePanel != null) startGamePanel.SetActive(false);
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}