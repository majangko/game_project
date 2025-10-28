using System.Collections.Generic;
using UnityEngine;

public static class ItemCatalog
{
    private static Dictionary<string, ItemBaseSO> map;
    private static bool loaded;

    public static void EnsureLoaded()
    {
        if (loaded) return;
        map = new Dictionary<string, ItemBaseSO>();

        // Resources/Items 폴더에서 모두 로드
        var all = Resources.LoadAll<ItemBaseSO>("Items");
        foreach (var so in all)
        {
            if (so == null || string.IsNullOrEmpty(so.ItemId)) continue;
            map[so.ItemId] = so;
        }
        loaded = true;
#if UNITY_EDITOR
        Debug.Log($"[ItemCatalog] Loaded {map.Count} items from Resources/Items");
#endif
    }

    public static ItemBaseSO Get(string id)
    {
        EnsureLoaded();
        return id != null && map.TryGetValue(id, out var so) ? so : null;
    }
}
