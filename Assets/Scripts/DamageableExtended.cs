using UnityEngine;

public class DamageableExtended : Damageable
{
    // 체력 접근자
    public int CurrentHP => typeof(Damageable)
        .GetField("hp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        .GetValue(this) is int value ? value : 0;

    public int MaxHPValue => typeof(Damageable)
        .GetField("maxHP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        .GetValue(this) is int value ? value : 1;

    public float HPRatio => (float)CurrentHP / Mathf.Max(1, MaxHPValue);

    // 체력 소모 메서드 (넉백 없이 순수 HP 감소)
    public void TakePureDamage(int damage)
    {
        int newHP = Mathf.Max(CurrentHP - damage, 0);

        // 내부 hp 값 강제로 반영
        typeof(Damageable)
            .GetField("hp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(this, newHP);

        if (newHP <= 0)
        {
            // Die() 호출
            typeof(Damageable)
                .GetMethod("Die", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(this, null);
        }
    }
}
