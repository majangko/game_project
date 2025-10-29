using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }
    [SerializeField] private ItemDatabase database;

    private readonly Dictionary<string,int> counts = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
    {
        Destroy(gameObject); // 씬에 새로 생긴 빈 인벤토리는 즉시 파괴
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
        if (Instance && Instance!=this){ Destroy(gameObject); return; }
        Instance=this; DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Add(string itemId, int n=1)
    {
        if (string.IsNullOrEmpty(itemId) || n<=0) return;
        if (counts.ContainsKey(itemId)) counts[itemId]+=n; else counts[itemId]=n;
        Save();
    }

    public bool UseOne(string itemId, PlayerContext ctx, StatusEffectManager eff)
    {
        if (!counts.TryGetValue(itemId, out var c) || c<=0) return false;
        var so = database?.Get(itemId) as ConsumableItemSO; if (so==null) return false;

        so.ApplyEffect(ctx, eff);
        counts[itemId] = c-1; if (counts[itemId]<=0) counts.Remove(itemId);
        Save(); return true;
    }

    public IReadOnlyDictionary<string,int> All() => counts;

    public void Save()
    {
        var data = new InventorySaveData();
        foreach (var kv in counts) data.Items.Add(new ItemStack(kv.Key, kv.Value));
        SaveSystem.SaveInventory(data);
    }

    public void Load()
    {
        counts.Clear();
        var data = SaveSystem.LoadInventory();
        foreach (var st in data.Items) counts[st.ItemId]=st.Count;
    }

    public void ResetAll()
    {
        counts.Clear();
        SaveSystem.ClearAll();
    }
}
