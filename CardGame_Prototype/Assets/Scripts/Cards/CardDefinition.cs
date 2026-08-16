using UnityEngine;

[CreateAssetMenu(fileName = "Card Definition", menuName = "Cabboo/Card Definition")]
public class CardDefinition : ScriptableObject
{
    [SerializeField] private Suit suit;
    [SerializeField] private int rank;
    [SerializeField] private Sprite faceSprite;

    public Suit Suit => suit;
    public int Rank => rank;
    public Sprite FaceSprite => faceSprite;
}