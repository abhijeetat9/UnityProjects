using TMPro;
using UnityEngine;

public class PlayerGridPanel : MonoBehaviour
{
    [SerializeField] private CardSlot[] cardSlots;
    public CardSlot[] CardSlots => cardSlots;
    [SerializeField] private TextMeshPro label;

    public void Render(PlayerState player, bool[] revealSlot)
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            bool shouldReveal = revealSlot != null && i < revealSlot.Length && revealSlot[i];
            cardSlots[i].SetIdentity(player.PlayerId, i);
            if (shouldReveal)
            {
                cardSlots[i].ShowCard(player.Slots[i]);
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