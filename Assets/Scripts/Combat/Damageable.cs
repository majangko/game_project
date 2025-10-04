using UnityEngine;
using System;

public class Damageable : MonoBehaviour
{
    [Header("Settings")]
    public int maxHP = 50;
    private int currentHP;

    [Header("Animation")]
    public Animator animator;
    public string hitTrig = "3_Damage";
    public string dieTrig = "4_Death";

    [Header("Optional")]
    public Rigidbody2D rb;
    public MonoBehaviour enemyAI; // IEnemyAIEvents를 구현한 스크립트 연결

    private bool isDead;

    public Action OnDeath;

    void Start()
    {
        currentHP = maxHP;
    }

    // ✅ 핵심 피격 함수
    public void TakeHit(int damage)
    {
        if (isDead) return;

        currentHP -= damage;

        if (animator)
            animator.SetTrigger(hitTrig);

        if (enemyAI is IEnemyAIEvents ai)
            ai.OnHurt();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // ✅ 오류 방지를 위한 오버로드 (호출은 받아서 무시)
    public void TakeHit(int damage, Vector2 knockback, Vector2 hitPoint)
    {
        TakeHit(damage);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator)
            animator.SetTrigger(dieTrig);

        if (enemyAI is IEnemyAIEvents ai)
            ai.OnDie();

        OnDeath?.Invoke();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        this.enabled = false;
        Destroy(gameObject, 1.5f);
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    public int GetCurrentHP() => currentHP;
    public bool IsDead() => isDead;
}
