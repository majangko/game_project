using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class ShopCatalog
{
    // ID → SO
    private static readonly Dictionary<string, ShopItemSO> _byId = new();
    private static bool _warmedFromManagers;
    private static bool _warmedFromResources;

    /// <summary>
    /// Resources에서 스캔할 폴더. 반드시 Resources/ 하위 경로여야 함.
    /// 예: "Data/Shop"  →  Assets/Resources/Data/Shop/* 에서만 로드
    /// </summary>
    public static string ResourcesFolder = "Data/Shop";

    /// <summary>
    /// 매니저들에 연결된 SO만 읽어서 카탈로그를 채운다. (안전, 언제 호출해도 OK)
    /// </summary>
    public static void WarmupFromManagers()
    {
        if (_warmedFromManagers) return;
        _warmedFromManagers = true;

        _byId.Clear();

        var managers = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(m => m && m.GetType().Name.IndexOf("ShopManager", StringComparison.OrdinalIgnoreCase) >= 0);

        foreach (var m in managers)
        {
            var t = m.GetType();
            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var f in fields)
            {
                // IEnumerable<ShopItemSO>
                if (typeof(IEnumerable<ShopItemSO>).IsAssignableFrom(f.FieldType))
                {
                    try
                    {
                        var col = f.GetValue(m) as IEnumerable<ShopItemSO>;
                        if (col == null) continue;
                        foreach (var so in col) TryAdd(so);
                    }
                    catch { }
                }
                // 단일 ShopItemSO
                else if (typeof(ShopItemSO).IsAssignableFrom(f.FieldType))
                {
                    try { TryAdd(f.GetValue(m) as ShopItemSO); } catch { }
                }
            }
        }
    }

    /// <summary>
    /// Play 중 한 프레임 이후(직렬화 끝난 뒤) 안전한 타이밍에 호출해야 함.
    /// UseItemPopup 등 UI 코드에서 호출하지 말고, 부트스트랩에서 미리 호출하세요.
    /// </summary>
    public static void WarmupFromResources()
    {
        if (_warmedFromResources) return;
        if (!Application.isPlaying) return; // 에디터 직렬화 타이밍 회피
        _warmedFromResources = true;

        if (string.IsNullOrEmpty(ResourcesFolder))
        {
            Debug.LogWarning("[ShopCatalog] ResourcesFolder가 비어 있습니다. 스캔 생략.");
            return;
        }

        try
        {
            var all = Resources.LoadAll<ShopItemSO>(ResourcesFolder);
            foreach (var so in all) TryAdd(so);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ShopCatalog] Resources 스캔 실패: {e.Message}");
        }
    }

    public static ShopItemSO GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_byId.TryGetValue(id, out var so)) return so;
        return null;
    }

    private static void TryAdd(ShopItemSO so)
    {
        if (!so) return;

        // 흔한 필드명으로 ID 유추
        string id = null;
        var t = so.GetType();

        // property id
        var piId = t.GetProperty("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (piId != null) id = piId.GetValue(so)?.ToString();

        // field id
        if (string.IsNullOrEmpty(id))
        {
            var fiId = t.GetField("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (fiId != null) id = fiId.GetValue(so)?.ToString();
        }

        // 없으면 에셋명 fallback
        if (string.IsNullOrEmpty(id)) id = so.name;

        if (!_byId.ContainsKey(id))
            _byId.Add(id, so);
    }
}
