using UnityEngine;
using UnityEditor;
using System.IO;

public static class CardDefinitionGenerator
{
    private static readonly Suit[] suits = new[] {
        Suit.Spade, Suit.Heart, Suit.Club, Suit.Diamond
    };

    [MenuItem("Cards/Generate CardDefinitions")]
    public static void GenerateCardDefinitions()
    {
        // 1. Ensure folder exists
        string resourcesPath = "Assets/Resources";
        string cardsFolder  = Path.Combine(resourcesPath, "Cards");
        if (!AssetDatabase.IsValidFolder(resourcesPath))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(cardsFolder))
            AssetDatabase.CreateFolder(resourcesPath, "Cards");

        // 2. Clear out old assets (optional)
        var existing = AssetDatabase.FindAssets("t:CardDefinition", new[] { cardsFolder });
        foreach (var guid in existing)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
        }

        // 3. Create one SO per suit/rank
        foreach (var suit in suits)
        {
            for (int rank = 1; rank <= 13; rank++)
            {
                string assetName = $"{suit}_{rank}";
                string assetPath = Path.Combine(cardsFolder, $"{assetName}.asset");

                // Instantiate and set up
                var cd = ScriptableObject.CreateInstance<CardDefinition>();
                cd.cardName    = assetName;
                cd.suit        = suit;
                cd.rank        = rank;
                // leave sprites null for you to assign manually later
                AssetDatabase.CreateAsset(cd, assetPath);
            }
        }

        // 4. Create two Jokers
        for (int i = 1; i <= 2; i++)
        {
            string assetName = $"Joker_{i}";
            string assetPath = Path.Combine(cardsFolder, $"{assetName}.asset");

            var cd = ScriptableObject.CreateInstance<CardDefinition>();
            cd.cardName = assetName;
            cd.suit     = Suit.Joker;
            cd.rank     = 0;
            AssetDatabase.CreateAsset(cd, assetPath);
        }

        // 5. Save & refresh
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Generated 54 CardDefinition assets in Resources/Cards.");
    }
}
