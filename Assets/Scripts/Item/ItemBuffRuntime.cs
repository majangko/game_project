using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ItemBuffRuntime : MonoBehaviour
{
    public static ItemBuffRuntime Instance { get; private set; }

    [Header("Target")]
    [SerializeField] private PlayerStats player; // 비워두면 자동 탐색

    // 누적 버프(있는 멤버에만 반영)
    private float speedMultiplier = 1f;
    private int   attackBonus = 0;

    // 리플렉션 캐시
    private FieldInfo fi_hpPriv;         // "_hp" / "currentHP" / "hp" 등
    private PropertyInfo pi_hp;          // "HP" 같은 프로퍼티가 있다면
    private MethodInfo mi_setHP;         // "SetHP(int)"
    private FieldInfo fi_maxHP;          // "maxHP"
    private FieldInfo fi_moveSpeed;
    private PropertyInfo pi_moveSpeed;
    private FieldInfo fi_attack;
    private PropertyInfo pi_attack;

    private float baseSpeed;
    private int   baseAttack;
    private bool  baseCaptured;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!player) player = FindAnyObjectByType<PlayerStats>(FindObjectsInactive.Include);
        if (player) CacheReflection(player);
    }

    private void CacheReflection(PlayerStats p)
    {
        var t = p.GetType();

        // HP
        mi_setHP = t.GetMethod("SetHP", BindingFlags.Instance | BindingFlags.Public);
        pi_hp    = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => string.Equals(x.Name, "HP", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(x.Name, "CurrentHP", StringComparison.OrdinalIgnoreCase));
        fi_hpPriv = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .FirstOrDefault(x => string.Equals(x.Name, "_hp", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(x.Name, "currentHP", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(x.Name, "hp", StringComparison.OrdinalIgnoreCase));
        fi_maxHP = t.GetField("maxHP", BindingFlags.Instance | BindingFlags.Public);

        // Speed
        pi_moveSpeed = t.GetProperty("moveSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        fi_moveSpeed = t.GetField("moveSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Attack
        pi_attack = t.GetProperty("attackDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        fi_attack = t.GetField("attackDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        CaptureBaseValues();
    }

    private void CaptureBaseValues()
    {
        if (!player || baseCaptured) return;

        baseSpeed  = TryGetFloat(player, pi_moveSpeed, fi_moveSpeed, 0f);
        baseAttack = TryGetInt(player, pi_attack, fi_attack, 0);

        baseCaptured = true;
    }

    // 공개 API ----------------------------------------------------

    public void ApplyByItemId(string itemId)
    {
        if (!player)
        {
            player = FindAnyObjectByType<PlayerStats>(FindObjectsInactive.Include);
            if (player) CacheReflection(player);
        }
        if (!player) { Debug.LogWarning("[ItemBuffRuntime] PlayerStats 없음"); return; }

        if (itemId.StartsWith("HP", StringComparison.OrdinalIgnoreCase))
        {
            int amount = ParseInt(itemId, "HP");
            Heal(amount);
            return;
        }

        if (itemId.StartsWith("SPD", StringComparison.OrdinalIgnoreCase))
        {
            float m = ParseSpeedMultiplier(itemId); // 1.015 / 1.02 / 1.03
            speedMultiplier *= m;
            ApplyAggregates();
            Debug.Log($"[ItemBuffRuntime] SPD x{m:F3} (누적 x{speedMultiplier:F3})");
            return;
        }

        if (itemId.StartsWith("ATK", StringComparison.OrdinalIgnoreCase))
        {
            int plus = ParseInt(itemId, "ATK");
            attackBonus += plus;
            ApplyAggregates();
            Debug.Log($"[ItemBuffRuntime] ATK +{plus} (누적 +{attackBonus})");
            return;
        }

        Debug.Log($"[ItemBuffRuntime] 알 수 없는 아이템 ID: {itemId}");
    }

    public void ResetAll()
    {
        if (!player) return;
        // 원복(멤버가 있을 때만)
        TrySetFloat(player, pi_moveSpeed, fi_moveSpeed, baseSpeed);
        TrySetInt(player,   pi_attack,    fi_attack,    baseAttack);

        speedMultiplier = 1f;
        attackBonus = 0;
        Debug.Log("[ItemBuffRuntime] 버프 초기화 완료");
    }

    // 내부 로직 ---------------------------------------------------

    private void Heal(int amount)
    {
        int curHP = GetCurrentHP();
        int max   = fi_maxHP != null ? (int)fi_maxHP.GetValue(player) : 99999;
        int newHP = Mathf.Clamp(curHP + amount, 0, max);

        if (mi_setHP != null)          mi_setHP.Invoke(player, new object[] { newHP });
        else if (pi_hp != null && pi_hp.CanWrite) pi_hp.SetValue(player, newHP);
        else if (fi_hpPriv != null)    fi_hpPriv.SetValue(player, newHP);
        else                           Debug.LogWarning("[ItemBuffRuntime] HP 설정 수단을 찾지 못함");
    }

    private int GetCurrentHP()
    {
        if (pi_hp != null && pi_hp.CanRead)      return (int)pi_hp.GetValue(player);
        if (fi_hpPriv != null)                   return (int)fi_hpPriv.GetValue(player);
        // 마지막 수단: 0 반환하고 SetHP로 바로 세팅
        return 0;
    }

    private void ApplyAggregates()
    {
        if (!player) return;

        // moveSpeed가 있을 때만 적용
        if (pi_moveSpeed != null || fi_moveSpeed != null)
        {
            float newSpeed = baseSpeed * Mathf.Max(0.01f, speedMultiplier);
            TrySetFloat(player, pi_moveSpeed, fi_moveSpeed, newSpeed);
        }

        // attackDamage가 있을 때만 적용
        if (pi_attack != null || fi_attack != null)
        {
            int newAtk = baseAttack + attackBonus;
            TrySetInt(player, pi_attack, fi_attack, newAtk);
        }
    }

    // 유틸 --------------------------------------------------------

    private static int   ParseInt(string id, string prefix) => int.TryParse(id.Substring(prefix.Length), out var v) ? v : 0;

    private static float ParseSpeedMultiplier(string id)
    {
        // SPD1.5 → 1.015,  SPD2 → 1.02,  SPD3 → 1.03
        var num = id.Substring(3);
        if (float.TryParse(num, out var v)) return 1f + (v / 100f);
        return 1f;
    }

    private static float TryGetFloat(object obj, PropertyInfo pi, FieldInfo fi, float def)
    {
        try
        {
            if (pi != null && pi.CanRead) return Convert.ToSingle(pi.GetValue(obj));
            if (fi != null)               return Convert.ToSingle(fi.GetValue(obj));
        }
        catch { }
        return def;
    }

    private static int TryGetInt(object obj, PropertyInfo pi, FieldInfo fi, int def)
    {
        try
        {
            if (pi != null && pi.CanRead) return Convert.ToInt32(pi.GetValue(obj));
            if (fi != null)               return Convert.ToInt32(fi.GetValue(obj));
        }
        catch { }
        return def;
    }

    private static void TrySetFloat(object obj, PropertyInfo pi, FieldInfo fi, float value)
    {
        try
        {
            if (pi != null && pi.CanWrite) { pi.SetValue(obj, value); return; }
            if (fi != null)                { fi.SetValue(obj, value); return; }
        }
        catch { }
    }

    private static void TrySetInt(object obj, PropertyInfo pi, FieldInfo fi, int value)
    {
        try
        {
            if (pi != null && pi.CanWrite) { pi.SetValue(obj, value); return; }
            if (fi != null)                { fi.SetValue(obj, value); return; }
        }
        catch { }
    }
}
