using UnityEngine;
using System;
using System.Collections;
using Game.Player;

public class Damageable : MonoBehaviour, ICanTakeDamage   // ✅ 인터페이스 추가
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

    [Header("Player Options")]
    [SerializeField] private float invincibleTime = 0.6f; // 플레이어 무적시간
    [SerializeField] private GameObject hitEffect;
    private bool isInvincible = false;

    private bool isDead;

    public Action OnDeath;

    private bool isPlayer; // 자동 인식용

    void Start()
    {
        currentHP = maxHP;
        isPlayer = CompareTag("Player");
    }

    // ✅ TrapMap이 호출할 수 있는 인터페이스 함수
    public void ApplyDamage(int amount)
    {
        TakeHit(amount);
    }

    // ✅ 핵심 피격 함수
    public void TakeHit(int damage)
    {
        TakeHit(damage, Vector2.zero, transform.position);
    }

    public void TakeHit(int damage, Vector2 knockback, Vector2 hitPoint)
    {
        if (isDead) return;
        if (isPlayer && isInvincible) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);


        // 피격 연출
        if (animator)
            animator.SetTrigger(hitTrig);

        if (hitEffect)
            Instantiate(hitEffect, hitPoint, Quaternion.identity);

        // 넉백
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        // AI 이벤트
        if (enemyAI is IEnemyAIEvents ai)
            ai.OnHurt();

        // HP 0 이하
        if (currentHP <= 0)
        {
            Die();
        }
        else if (isPlayer)
        {
            StartCoroutine(InvincibleRoutine());
        }

        // ✅ 플레이어 HP UI 반영
        if (isPlayer)
        {
            var stats = GetComponent<PlayerStats>();
            if (stats != null)
                stats.SetHP(currentHP);
        }
    }

    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;

        // 깜빡임 연출
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        for (int i = 0; i < 6; i++)
        {
            if (sr) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(invincibleTime / 12f);
        }
        if (sr) sr.enabled = true;

        yield return new WaitForSeconds(invincibleTime / 2f);
        isInvincible = false;
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

        // ✅ 플레이어는 파괴하지 않음
        if (!isPlayer)
            Destroy(gameObject, 1.5f);
        else
            Debug.Log("플레이어 사망 (게임오버 처리 필요)");
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHP = Mathf.Min(currentHP + amount, maxHP);

        if (isPlayer)
        {
            var stats = GetComponent<PlayerStats>();
            if (stats != null)
                stats.SetHP(currentHP);
        }
    }

    public int GetCurrentHP() => currentHP;
    public bool IsDead() => isDead;
}
