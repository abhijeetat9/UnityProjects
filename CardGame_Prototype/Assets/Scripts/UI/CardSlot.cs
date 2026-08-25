using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;


public class CardSlot : MonoBehaviour
{
    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private Sprite cardBackSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public event Action<CardSlot> Clicked;

    public int PlayerId { get; private set; }
    public int SlotIndex { get; private set; }

    public void SetIdentity(int playerId, int slotIndex)
    {
        PlayerId = playerId;
        SlotIndex = slotIndex;
    }

    public void ShowCard(Card card)
    {
        CardDefinition definition = cardDatabase.GetDefinition(card.Suit, card.Rank);

        if (definition != null)
        {
            Sprite sprite = definition.FaceSprite;
            spriteRenderer.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"CardSlot ShowCard: Definition is null for {card}");
        }
        
    }

    public void FlashReveal(Card card, float duration)
    {
        ShowCard(card);
        StartCoroutine(HideAfterDelay(duration));
    }

    private IEnumerator HideAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        ShowHidden();
    }

    public void ShowHidden()
    {
        spriteRenderer.sprite = cardBackSprite;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log($"CardSlot.OnMouseDown: click on Player {PlayerId} Slot {SlotIndex} SWALLOWED (pointer over UI element)");
            return;
        }
        if (Clicked == null)
        {
            Debug.Log($"CardSlot.OnMouseDown: click on Player {PlayerId} Slot {SlotIndex} registered but NO LISTENERS subscribed (nothing will happen)");
        }
        else
        {
            Debug.Log($"CardSlot.OnMouseDown: click on Player {PlayerId} Slot {SlotIndex} -> invoking Clicked");
        }
        Clicked?.Invoke(this);
    }
    
    public void ShowEmpty()
    {
        spriteRenderer.sprite = null;
    }
}

