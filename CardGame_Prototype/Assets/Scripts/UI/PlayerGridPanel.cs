using TMPro;
using UnityEngine;

public class PlayerGridPanel : MonoBehaviour
{
    [SerializeField] private CardSlot[] cardSlots;
    public CardSlot[] CardSlots => cardSlots;
    [SerializeField] private TextMeshPro label;

    public void Render(NetworkGameController.PlayerView player)
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            cardSlots[i].SetIdentity(player.PlayerId, i);
            if (player.Slots[i].Card != null)
            {
                cardSlots[i].ShowCard(player.Slots[i].Card.Value);
            }
            else
            {
                cardSlots[i].ShowHidden();
            }
        }
    }

    public void SetLabel(string text)
    {
        label.text = text;
    }

    public void SetActiveTurn(bool isCurrentTurn)
    {
        label.color = isCurrentTurn ? Color.red : Color.white;
    }
}