using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card Database", menuName = "Cabboo/Card Database")]
public class CardDatabase : ScriptableObject
{
    [SerializeField]private List<CardDefinition> cardDefinitions =  new List<CardDefinition>();

    public CardDefinition GetDefinition(Suit suit, int rank)
    {
        return cardDefinitions.Find(d => d.Suit == suit && d.Rank == rank);
    }
}