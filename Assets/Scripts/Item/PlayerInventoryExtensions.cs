using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class PlayerInventoryExtensions
{
    /// <summary>
    /// 씬 전체에서 가장 "아이템을 실제로 보유"한 PlayerInventory를 찾아 반환.
    /// 없으면 Singleton, 그것도 없으면 첫 번째 발견 인스턴스를 반환.
    /// </summary>
    public static PlayerInventory FindWithItemsOrSingleton()
    {
        var all = UnityEngine.Object.FindObjectsByType<PlayerInventory>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        PlayerInventory best = null;
        int bestCount = -1;

        foreach (var inv in all)
        {
            var snap = Snapshot(inv);
            int c = snap?.Count ?? 0;
            if (c > bestCount)
            {
                best = inv;
                bestCount = c;
            }
        }

        if (best != null) return best;
        if (PlayerInventory.Instance != null) return PlayerInventory.Instance;
        return all.FirstOrDefault();
    }

    /// <summary>
    /// PlayerInventory 내부 상태를 가능한 한 정확히 사본으로 뽑아낸다.
    /// 우선순위: All() → 내부 Dictionary<string,int> → 빈 딕셔너리
    /// </summary>
    public static Dictionary<string, int> Snapshot(PlayerInventory inv)
    {
        if (inv == null) return new Dictionary<string, int>();

        try
        {
            // 1) All() 메서드가 있으면 사용
            var miAll = inv.GetType().GetMethod("All",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (miAll != null)
            {
                var ret = miAll.Invoke(inv, null) as IDictionary<string, int>;
                if (ret != null) return new Dictionary<string, int>(ret);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[InventoryExt] All() 호출 실패: {e.Message}");
        }

        try
        {
            // 2) 내부 Dictionary<string,int> 필드 직접 읽기
            var fi = inv.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(f => typeof(Dictionary<string, int>).IsAssignableFrom(f.FieldType));
            if (fi != null)
            {
                var dict = fi.GetValue(inv) as Dictionary<string, int>;
                if (dict != null) return new Dictionary<string, int>(dict);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[InventoryExt] 내부 Dictionary 읽기 실패: {e.Message}");
        }

        return new Dictionary<string, int>();
    }

    /// <summary>
    /// 인벤토리에서 해당 아이템을 count만큼 소모. (리플렉션 기반, 안전모드)
    /// 우선순위: Consume/Remove/Use/Decrease/ Add(-count) → 내부 Dictionary 조작
    /// </summary>
    public static bool TryConsume(PlayerInventory inv, string itemId, int count)
    {
        if (inv == null || string.IsNullOrEmpty(itemId) || count <= 0) return false;

        var t = inv.GetType();

        // 1) 공개/비공개 메서드 우선 시도
        var methodNames = new[] { "Consume", "Remove", "Use", "Decrease", "Add" };
        foreach (var name in methodNames)
        {
            var mi = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                 null, new Type[] { typeof(string), typeof(int) }, null);
            if (mi != null)
            {
                try
                {
                    int arg = name.Equals("Add", StringComparison.OrdinalIgnoreCase) ? -count : count;
                    var result = mi.Invoke(inv, new object[] { itemId, arg });
                    return result as bool? ?? true;
                }
                catch { /* 다음 경로 시도 */ }
            }
        }

        // 2) 내부 Dictionary 직접 수정
        var fiDict = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                      .FirstOrDefault(f => typeof(Dictionary<string, int>).IsAssignableFrom(f.FieldType));
        if (fiDict != null)
        {
            try
            {
                var dict = fiDict.GetValue(inv) as Dictionary<string, int>;
                if (dict != null && dict.TryGetValue(itemId, out var cur) && cur > 0)
                {
                    var newVal = Mathf.Max(0, cur - count);
                    if (newVal == 0) dict.Remove(itemId);
                    else dict[itemId] = newVal;
                    return true;
                }
            }
            catch { }
        }

        Debug.LogWarning("[InventoryExt] 소비 처리 경로를 찾지 못했습니다.");
        return false;
    }
}
