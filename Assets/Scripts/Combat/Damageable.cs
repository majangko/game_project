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
    [SerializeField] EnemyAI enemyAI;            // 적이면 연결, 플레이어면 없어도 OK

    int hp;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!enemyAI) enemyAI = GetComponent<EnemyAI>();
        hp = maxHP;
    }

    public void TakeHit(int damage, Vector2 knock, Vector2 hitPoint)
    {
        hp -= damage;

        // 피격 연출
        enemyAI?.OnHurt();
        if (!string.IsNullOrEmpty(hitTrig)) animator?.SetTrigger(hitTrig);
        if (rb) rb.AddForce(knock * 100f, ForceMode2D.Force);

        if (hp <= 0) Die();
    }

    void Die()
    {
        enemyAI?.OnDie();
        if (!string.IsNullOrEmpty(dieTrig)) animator?.SetTrigger(dieTrig);

        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        enabled = false;
    }
}
