using UnityEngine;

public class InventoryDebugProbe : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F6))
        {
            var inv = PlayerInventoryExtensions.FindWithItemsOrSingleton();
            var snap = PlayerInventoryExtensions.Snapshot(inv);
            Debug.Log($"[INV] picked={inv?.name ?? "null"}  kinds={(snap?.Count ?? 0)}");
            if (snap != null)
                foreach (var kv in snap)
                    Debug.Log($"[INV]  {kv.Key} x{kv.Value}");
        }
    }
}
