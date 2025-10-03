using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField] int maxHP = 50;

    [Header("Animator")]
    [SerializeField] Animator animator;          // UnitRoot(Animator)
    [SerializeField] string hitTrig = "3_Damage";
    [SerializeField] string dieTrig = "4_Death";

    [Header("Optional")]
    [SerializeField] Rigidbody2D rb;

    // EnemyAI 뿐만 아니라 EnemyLanceAI, EnemySwordAI 등도 받을 수 있도록 변경
    [SerializeField] MonoBehaviour enemyAI;

    int hp;
    bool isDead = false;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        hp = maxHP;
    }

    public void TakeHit(int damage, Vector2 knock, Vector2 hitPoint)
    {
        if (isDead) return;

        hp -= damage;

        // 피격 연출
        TryCallOnHurt();
        if (!string.IsNullOrEmpty(hitTrig)) animator?.SetTrigger(hitTrig);
        if (rb) rb.AddForce(knock * 100f, ForceMode2D.Force);

        if (hp <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        TryCallOnDie();
        if (!string.IsNullOrEmpty(dieTrig)) animator?.SetTrigger(dieTrig);

        // 콜라이더 비활성화 (다시는 충돌 안 하게)
        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        // 애니메이션 길이에 맞춰 지연 후 삭제
        Destroy(gameObject, 0.7f);  // Death 애니메이션이 약 0.7초라면
    }

    // 공통 메서드 호출 (Reflection 대신 as-cast 사용)
    void TryCallOnHurt()
    {
        var ai = enemyAI as IEnemyAIEvents;
        ai?.OnHurt();
    }

    void TryCallOnDie()
    {
        var ai = enemyAI as IEnemyAIEvents;
        ai?.OnDie();
    }
}

// 모든 EnemyAI 계열이 구현해야 하는 인터페이스
public interface IEnemyAIEvents
{
    void OnHurt();
    void OnDie();
}
