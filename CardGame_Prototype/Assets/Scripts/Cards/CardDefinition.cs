using UnityEngine;

// Assets/Scripts/Cards/CardDefinition.cs
using UnityEngine;

[CreateAssetMenu(menuName="Cards/CardDefinition")]
public class CardDefinition : ScriptableObject {
  public string cardName;
  public Suit suit;       // enum { Spade, Heart, Club, Diamond, Joker }
  public int rank;        // 1–13 (Ace–King, Joker=0)
  public Sprite faceSprite;
  public Sprite backSprite;
  // any flags for special rules: e.g. public bool triggersOnDraw;
}

