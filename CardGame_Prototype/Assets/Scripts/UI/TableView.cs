using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class TableView : MonoBehaviour
{
    private GameController _gameController;
    private bool [][] _revealState = new bool[4][];
    [SerializeField] private PlayerGridPanel playerGridPanelPrefab;
    private List<PlayerGridPanel> playerGridPanels = new List<PlayerGridPanel>();
    [SerializeField] private int numPlayers;
    [SerializeField] private TurnUIController turnUIController;
    [SerializeField] private CardSlot deckSlot;
    [SerializeField] private CardSlot discardSlot;
    
    public void Start()
    {
        GameState state = RoundSetup.CreateGameState(numPlayers);
        SpawnPlayersPanel(numPlayers);
        _gameController = new GameController(state);
        turnUIController.Initialize(_gameController, playerGridPanels);
        deckSlot.ShowHidden();
        int playerCount = _gameController.State.Players.Count;
        _revealState = new bool[playerCount][];
        for (int i = 0; i < playerCount; i++)
        {
            _revealState[i] = new bool[4];
        }
        _gameController.StateChanged += RenderAll;
        _gameController.NewRoundStarted += () =>
        {
            StartCoroutine(RunInitialPeekPhase());
        };
        RenderAll();
        StartCoroutine(RunInitialPeekPhase());
    }
    // private GameState SetupTestGame(int numPlayers)
    // {
    //     Deck deck = new Deck();
    //     deck.BuildStandardDeck();
    //     deck.Shuffle(new Random());
    //     
    //     List<PlayerState> players = Enumerable.Range(0, numPlayers)
    //         .Select(i => new PlayerState(i , $"Player {i}"))
    //         .ToList();
    //     
    //     foreach (var player in players)
    //     {
    //         for (int slot = 0; slot < 4; slot++)
    //         {
    //             player.Slots[slot] = deck.DrawCard();
    //         }
    //     }
    //
    //     deck.DiscardCard(deck.DrawCard());
    //     return new GameState()
    //     {
    //         Deck = deck,
    //         Players = players,
    //         GamePhases = GamePhase.InitialPeek,
    //         CurrentPlayerIndex = 0
    //     };
    // }
    
    private void SpawnPlayersPanel(int numPlayers)
    {
        for (int i = 0; i < numPlayers; i++)
        {
            Vector3 position = GetSeatPosition(i, numPlayers, radius: 5f);
            PlayerGridPanel panel = Instantiate(playerGridPanelPrefab, position, Quaternion.identity);
            panel.SetLabel($"Player {i+1}");
            playerGridPanels.Add(panel);
        }
    }

    private void RenderAll()
    {
        int count = Math.Min(playerGridPanels.Count, _gameController.State.Players.Count);
        for (int i = 0; i < count; i++)
        {
            playerGridPanels[i].Render(_gameController.State.Players[i], _revealState[i]);
            playerGridPanels[i].SetActiveTurn(i == _gameController.State.CurrentPlayerIndex);
        }

        if (_gameController.State.Deck.HasDiscardTop)
        {
            discardSlot.ShowCard(_gameController.State.Deck.GetDiscardTop());
        }
        else
        {
            discardSlot.ShowEmpty();
        }
    }

    private IEnumerator RunInitialPeekPhase()
    {
        for (int i = 0; i < _gameController.State.Players.Count; i++)
        {
            var peek2 = new PeekOwnCardCommand { SlotIndex = 2 };
            _gameController.TryExecute(peek2,  _gameController.State.Players[i].PlayerId);
            _revealState[i][2] = true;
            var peek3 = new PeekOwnCardCommand { SlotIndex = 3 };
            _gameController.TryExecute(peek3,  _gameController.State.Players[i].PlayerId);
            _revealState[i][3] = true;
        }
        RenderAll();
        yield return new WaitForSeconds(5f);
        for (int i = 0; i < _gameController.State.Players.Count; i++)
        {
            _revealState[i][2] = false;
            _revealState[i][3] = false;
            
        }
        turnUIController.ShowActions();
        RenderAll(); 
    }

    private Vector3 GetSeatPosition(int index, int totalPlayers, float radius)
    {
        float angleStep = 360f / totalPlayers;
        float angleDegrees = -90f + index * angleStep;
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRadians)*radius, Mathf.Sin(angleRadians)*radius, 0f);
    }
}