using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Random = System.Random;

public class TableView : MonoBehaviour
{
    public static TableView Instance { get;  private set; }
    private GameController _gameController;
    [SerializeField] private PlayerGridPanel playerGridPanelPrefab;
    private List<PlayerGridPanel> _playerGridPanels = new List<PlayerGridPanel>();
    [SerializeField] private TurnUIController turnUIController;
    [SerializeField] private CardSlot deckSlot;
    [SerializeField] private CardSlot discardSlot;
    private bool _actionsShown;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void Start()
    {
        SpawnPlayersPanel(CabbooNetworkManager.ConnectedPlayerCount);
        deckSlot.ShowHidden();
    }

    public void SetGameController(GameController controller)
    {
        _gameController = controller;
        turnUIController.Initialize(controller, _playerGridPanels);
    }

    private void SpawnPlayersPanel(int numPlayers)
    {
        for (int i = 0; i < numPlayers; i++)
        {
            Vector3 position = GetSeatPosition(i, numPlayers, radius: 5f);
            PlayerGridPanel panel = Instantiate(playerGridPanelPrefab, position, Quaternion.identity);
            panel.SetLabel($"Player {i+1}");
            _playerGridPanels.Add(panel);
        }
    }

    public void RenderAll(NetworkGameController.BoardSnapshot snapshot)
    {
        int count = Math.Min(_playerGridPanels.Count, snapshot.Players.Length);
        for (int i = 0; i < count; i++)
        {
            _playerGridPanels[i].Render(snapshot.Players[i]);
            _playerGridPanels[i].SetActiveTurn(i == snapshot.CurrentPlayerIndex);
        }
        discardSlot.ShowCard(snapshot.DiscardTop);
        if (snapshot.GamePhases != GamePhase.InitialPeek && !_actionsShown)
        {
            _actionsShown = true;
            turnUIController.ShowActions(); 
        }
    }

    private Vector3 GetSeatPosition(int index, int totalPlayers, float radius)
    {
        float angleStep = 360f / totalPlayers;
        float angleDegrees = -90f + index * angleStep;
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRadians)*radius, Mathf.Sin(angleRadians)*radius, 0f);
    }
}