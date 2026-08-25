using System.Collections;
using System.Linq;
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
        StartCoroutine(WaitForTableViewAndSetLocalPlayer());
    }
    
    [Command]
    private void CmdSetPlayerName(string name)
    {
        PlayerName = name;
    }

    [Command]
    public void CmdDraw()
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        bool success = gameController.TryDrawCard(SeatIndex);
        TargetActionResult("Draw", success);
    }

    [Command]
    public void CmdDiscard()
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        bool success = gameController.TryExecute(new DiscardDrawnCommand(), SeatIndex);
        TargetActionResult("Discard", success);
    }

    [Command]
    public void CmdSwap(int slotIndex)
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        bool success = gameController.TryExecute(new SwapDrawnCommand {SlotIndex =  slotIndex}, SeatIndex);
        TargetActionResult($"Swap(slot={slotIndex})", success);
    }

    [Command]
    public void CmdSwapFromDiscard(int slotIndex)
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        bool success = gameController.TryExecute(new SwapFromDiscardCommand {SlotIndex =  slotIndex}, SeatIndex);
        TargetActionResult($"SwapFromDiscard(slot={slotIndex})", success);
    }

    [Command]
    public void CmdSkipNext()
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        bool success = gameController.TryExecute(new UseSpecialPowerCommand {PowerCommand = new SkipNextCommand() },  SeatIndex);
        TargetActionResult("SkipNext", success);
    }

    [Command]
    public void CmdPeekOwn(int slotIndex)
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        PeekOwnCardCommand peekOwnCommand = new PeekOwnCardCommand { SlotIndex = slotIndex };
        UseSpecialPowerCommand peekOwnPower = new UseSpecialPowerCommand { PowerCommand = peekOwnCommand };
        bool success = gameController.TryExecute(peekOwnPower, SeatIndex);
        TargetActionResult($"PeekOwn(slot={slotIndex})", success);
        if (success)
        {
            networkGameController.RevealTemporarily(SeatIndex,slotIndex,5f);
        }
    }

    [Command]
    public void CmdPeekTarget(int slotIndex,  int targetPlayerId)
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        PeekOpponentCardCommand peekOpponentCardCommand = new PeekOpponentCardCommand() { SlotIndex = slotIndex, TargetPlayerId =  targetPlayerId };
        UseSpecialPowerCommand peekTargetPower = new UseSpecialPowerCommand { PowerCommand = peekOpponentCardCommand };
        bool success = gameController.TryExecute(peekTargetPower, SeatIndex);
        Debug.Log($"CmdPeekTarget: seat={SeatIndex} target={targetPlayerId} slot={slotIndex} success={success}");
        TargetActionResult($"PeekTarget(target={targetPlayerId}, slot={slotIndex})", success);
        if (success)
        {
            networkGameController.RevealTemporarily(targetPlayerId, slotIndex,5f);
        }
    }

    [Command]
    public void CmdBlindSwap(int slotIndex, int targetPlayerId, int targetSlotIndex)
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        SwapWithPlayerCommand swapWithPlayerCommand;
        UseSpecialPowerCommand swapwithPlayerPower;

        swapWithPlayerCommand = new SwapWithPlayerCommand {OwnSlotIndex= slotIndex, TargetPlayerId = targetPlayerId, TargetSlotIndex = targetSlotIndex};
        swapwithPlayerPower = new UseSpecialPowerCommand { PowerCommand = swapWithPlayerCommand };
        bool success = gameController.TryExecute(swapwithPlayerPower, SeatIndex);
        Debug.Log($"CmdBlindSwap: seat={SeatIndex} ownSlot={slotIndex} target={targetPlayerId} targetSlot={targetSlotIndex} success={success}");
        TargetActionResult($"BlindSwap(ownSlot={slotIndex}, target={targetPlayerId}, targetSlot={targetSlotIndex})", success);
    }

    [Command]
    public void CmdLookAndSwapPeekOwn(int slotIndex)
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        PeekOwnCardCommand peekOwnCommand = new PeekOwnCardCommand { SlotIndex = slotIndex };
        bool success = gameController.TryExecute(peekOwnCommand, SeatIndex);
        Debug.Log($"CmdLookAndSwapPeekOwn: seat={SeatIndex} slot={slotIndex} success={success}");
        TargetActionResult($"LookAndSwapPeekOwn(slot={slotIndex})", success);
        if (success)
        {
            networkGameController.RevealTemporarily(SeatIndex,slotIndex,5f);
        }
    }

    [Command]
    public void CmdLookAndSwapPeekTarget(int slotIndex, int targetPlayerId)
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        RedKingLookCommand redKingLookCommand = new RedKingLookCommand { LookCommand = new PeekOpponentCardCommand {TargetPlayerId = targetPlayerId,  SlotIndex = slotIndex } };
        bool success = gameController.TryExecute(redKingLookCommand, SeatIndex);
        Debug.Log($"CmdLookAndSwapPeekTarget: seat={SeatIndex} target={targetPlayerId} slot={slotIndex} success={success}");
        TargetActionResult($"LookAndSwapPeekTarget(target={targetPlayerId}, slot={slotIndex})", success);
        if (success)
        {
            networkGameController.RevealTemporarily(targetPlayerId, slotIndex,5f);
        }
    }

    [Command]
    public void CmdRedKingDecision(bool shouldSwap, int ownSlotIndex)
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        RedKingDecisionCommand redKingDecisionCommand = new RedKingDecisionCommand { ShouldSwap =  shouldSwap, OwnSlotIndex =  ownSlotIndex };
        bool success = gameController.TryExecute(redKingDecisionCommand, SeatIndex);
        Debug.Log($"CmdRedKingDecision: seat={SeatIndex} shouldSwap={shouldSwap} ownSlot={ownSlotIndex} success={success}");
        TargetActionResult($"RedKingDecision(shouldSwap={shouldSwap}, ownSlot={ownSlotIndex})", success);
    }

    [Command]
    public void CmdCallCabboo()
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        var gameController = networkGameController.GameController;
        bool success = gameController.TryExecute(new CallCabbooCommand(), SeatIndex);
        TargetActionResult("CallCabboo", success);
    }

    // Sent from server back to specifically the client that issued the Cmd,
    // so failures show up on the acting client's own console instead of only
    // the host's. Purely diagnostic - doesn't drive any UI state.
    [TargetRpc]
    private void TargetActionResult(string actionName, bool success)
    {
        if (success)
        {
            Debug.Log($"[ActionResult] {actionName}: SUCCESS");
        }
        else
        {
            Debug.LogWarning($"[ActionResult] {actionName}: FAILED (server rejected - check CanExecute conditions: is it your turn? valid slot? pending card present?)");
        }
    }

    [Command]
    public void CmdNewRound()
    {
        var networkGameController = FindAnyObjectByType<NetworkGameController>();
        networkGameController.BeginNewRound(NetworkServer.connections.Count);
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
    
    private IEnumerator WaitForTableViewAndSetLocalPlayer()
    {
        yield return new WaitUntil(() => TableView.Instance != null);
        TableView.Instance.SetLocalPlayer(this);
    }
}
