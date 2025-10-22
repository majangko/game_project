using UnityEngine;
using System.Reflection;

public class DamageableExtended : Damageable
{
    private FieldInfo _hpField;
    private FieldInfo _maxHpField;
    private MethodInfo _dieMethod;
    private PlayerStats _stats;

    void Awake()
    {
        // ✅ Damageable 내부 변수명에 맞게 변경
        _hpField = typeof(Damageable).GetField("currentHP", BindingFlags.NonPublic | BindingFlags.Instance);
        _maxHpField = typeof(Damageable).GetField("maxHP", BindingFlags.Public | BindingFlags.Instance);
        _dieMethod = typeof(Damageable).GetMethod("Die", BindingFlags.NonPublic | BindingFlags.Instance);

        _stats = GetComponentInParent<PlayerStats>();
    }

    public int CurrentHPValue => (int)(_hpField?.GetValue(this) ?? 0);
    public int MaxHPValue => (int)(_maxHpField?.GetValue(this) ?? 1);
    public float HPRatio => (float)CurrentHPValue / Mathf.Max(1, MaxHPValue);

    public void TakePureDamage(int damage)
    {
        if (_hpField == null)
        {
            Debug.LogWarning("[DamageableExtended] hpField not found!");
            return;
        }

        int curHP = CurrentHPValue;
        int newHP = Mathf.Max(curHP - Mathf.Abs(damage), 0);

        // ✅ 내부 Damageable 체력 갱신
        _hpField.SetValue(this, newHP);

        // ✅ PlayerStats HUD 동기화
        if (_stats != null)
        {
            _stats.SetHP(newHP);
        }

        // ✅ 사망 처리
        if (newHP <= 0 && _dieMethod != null)
        {
            _dieMethod.Invoke(this, null);
        }
    }
}
