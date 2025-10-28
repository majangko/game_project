using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemBaseSO> items;
    private Dictionary<string, ItemBaseSO> map;

    public void Init()
    {
        if (map != null) return;
        map = new Dictionary<string, ItemBaseSO>();
        foreach (var it in items)
        {
            if (it == null || string.IsNullOrEmpty(it.ItemId)) continue;
            map[it.ItemId] = it;
        }
    }

    public ItemBaseSO Get(string id)
    {
        Init();
        return map != null && map.TryGetValue(id, out var so) ? so : null;
    }

    public IEnumerable<ItemBaseSO> All()
    {
        Init();
        return map?.Values;
    }
}
