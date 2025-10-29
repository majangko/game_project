using UnityEngine;
using System;
using System.Collections;
using Game.Player;

public class Damageable : MonoBehaviour, ICanTakeDamage
{
    [Header("Settings")]
    public int maxHP = 50;
    private int currentHP;
    [Header("Boss Settings")]
    public bool isBoss = false;
    public bool isFinalBoss = false; // ✅ 마지막 보스만 true

    [Header("Animation")]
    public Animator animator;
    public string hitTrig = "3_Damage";
    public string dieTrig = "4_Death";

    [Header("Optional")]
    public Rigidbody2D rb;
    public MonoBehaviour enemyAI; // IEnemyAIEvents를 구현한 스크립트 연결

    [Header("Player Options")]
    [SerializeField] private float invincibleTime = 0.6f; // 피격 무적 시간
    [SerializeField] private GameObject hitEffect;
    private bool isInvincible = false;

    private bool isDead;
    private bool isPlayer; // 자동 인식용
    private float damageMultiplier = 1f; // ✅ 추가: 피해량 배율 제어 (1 = 기본)

    public Action OnDeath;

    // ✅ 추가: 보스 사망 시 띄울 GameClear UI Prefab (Inspector 연결)
    [Header("Game Clear UI")]
    public GameObject gameClearUIPrefab;


    void Start()
    {
        currentHP = maxHP;
        isPlayer = CompareTag("Player");
    }

    // ✅ 외부에서 피해 배율 조정 (예: 가드 스킬, 버프 등)
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0f, multiplier); // 0 = 완전 무적
    }

    // ✅ 현재 피해 배율 가져오기
    public float GetDamageMultiplier() => damageMultiplier;

    // ✅ 외부(TrapMap, TagManager 등)가 호출할 수 있는 인터페이스 함수
    public void ApplyDamage(int amount)
    {
        TakeHit(amount);
    }

    // ✅ 핵심 피격 함수 (float 입력)
    public void TakeHit(float damage)
    {
        TakeHit(Mathf.RoundToInt(damage), Vector2.zero, transform.position);
    }

    // ✅ 실제 피해 처리
    public void TakeHit(int damage, Vector2 knockback, Vector2 hitPoint)
    {
        if (isDead) return;
        if (isPlayer && isInvincible) return;

        // ✅ 피해량 배율 반영
        int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

        currentHP -= finalDamage;
        Debug.Log($"[Damageable] {gameObject.name} took {finalDamage} damage → HP: {currentHP}/{maxHP}");

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

        // 💰 골드 지급
        if (!isPlayer && GoldManager.Instance != null)
        {
            int goldReward = isBoss ? 50 : 5;
            GoldManager.Instance.AddGold(goldReward);
        }

        // 💀 포탈 스폰 (기존 기능)
        var bossPortalSpawner = GetComponent<BossDeathPortalSpawner>();
        if (bossPortalSpawner != null)
            bossPortalSpawner.OnBossDeath();

        // 💀 보스 사망 시 GameClear UI 표시
        if (isBoss && gameClearUIPrefab != null)
        {
            StartCoroutine(ShowClearUIAfterDelay(1.5f)); // 약간의 여유시간 후 표시
        }

        // 💀 플레이어는 파괴하지 않음
        if (!isPlayer)
            Destroy(gameObject, 1.5f);
        else
            Debug.Log("플레이어 사망 (게임오버 처리 필요)");
    }

    // ✅ 이 코루틴은 Die() 바깥, Damageable 클래스 내부에 따로 둡니다
    private IEnumerator ShowClearUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Game Clear UI 프리팹 생성
        GameObject uiObj = Instantiate(gameClearUIPrefab);
        Debug.Log("[Damageable] Boss defeated → Game Clear UI displayed ✅");

        // 보스 종류에 따라 이동 여부 설정
        var manager = uiObj.GetComponent<GameClearSceneManager>();
        if (manager != null)
            manager.isFinalStage = isFinalBoss; // 마지막 보스만 StartIsland로 이동
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

    // ✅ 외부 제어용 무적 함수 (TagManager 등에서 호출)
    public void SetInvincible(bool value)
    {
        isInvincible = value;

        // 시각적 표시 (선택 사항)
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr)
        {
            Color c = sr.color;
            c.a = value ? 0.6f : 1f; // 무적일 때 반투명 처리
            sr.color = c;
        }
    }

    public int GetCurrentHP() => currentHP;
    public bool IsDead() => isDead;
}