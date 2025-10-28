using UnityEngine; using System.IO;

public static class SaveSystem
{
    private static string InvPath => Path.Combine(Application.persistentDataPath, "inventory.json");
    public static void SaveInventory(InventorySaveData d){ System.IO.File.WriteAllText(InvPath, JsonUtility.ToJson(d)); }
    public static InventorySaveData LoadInventory(){
        if(!File.Exists(InvPath)) return new InventorySaveData();
        var json = File.ReadAllText(InvPath);
        return JsonUtility.FromJson<InventorySaveData>(json) ?? new InventorySaveData();
    }
    public static void ClearAll(){ if(File.Exists(InvPath)) File.Delete(InvPath); }
}
