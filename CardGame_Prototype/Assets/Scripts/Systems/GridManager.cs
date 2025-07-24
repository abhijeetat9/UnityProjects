using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridManager : MonoBehaviour {
    [Tooltip("Drag in your CardSlot prefab here")]
    public GameObject slotPrefab;
    [Tooltip("Should equal constraint count in GridLayoutGroup")]
    public int totalSlots = 4;

    void Awake() {
        var grid = GetComponent<GridLayoutGroup>();
        // sanity check: grid.constraintCount should be 2, constraint = FixedColumnCount
        for (int i = 0; i < totalSlots; i++) {
            var slot = Instantiate(slotPrefab, transform);
            slot.name = $"Slot_{i}";
            Debug.Log($"Instantiated {slot.name} at position {i}");
        }
    }
}
