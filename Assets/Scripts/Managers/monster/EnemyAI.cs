using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] Transform visualRoot;
    [SerializeField] Transform player; // 추적할 플레이어

    [Header("Detect/Attack")]
    [SerializeField] float detectRadius = 6f;   // 탐지 범위
    [SerializeField] float attackRange = 1.6f;  // 공격 범위
    [SerializeField] float attackCooldown = 1f; // 공격 쿨타임
    float lastAttackTime;

    [Header("Move")]
    [SerializeField] float moveSpeed = 2f;

    [Header("Animator Params")]
    [SerializeField] string moveBool = "1_Move";
    [SerializeField] string attackTrig = "2_Attack";
    [SerializeField] string hitTrig = "3_Damage";
    [SerializeField] string dieTrig = "4_Death";

    bool isDead;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visualRoot) visualRoot = transform;
    }

    void Update()
    {
        if (isDead || !player) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            StopMoving();
            TryAttack();
        }
        else if (distance <= detectRadius)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }

        FlipToPlayer(); // 항상 시선 보정
    }

    void ChasePlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);
        animator.SetBool(moveBool, true);
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool(moveBool, false);
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        animator.SetTrigger(attackTrig);
        lastAttackTime = Time.time;
    }

    void FlipToPlayer()
    {
        if (!player) return;

        Vector3 scale = visualRoot.localScale;

        // ✅ SPUM 방향 반대 보정
        if (player.position.x > transform.position.x)
            scale.x = -Mathf.Abs(scale.x); // 오른쪽
        else
            scale.x = Mathf.Abs(scale.x);  // 왼쪽

        visualRoot.localScale = scale;
    }

    // --- Damageable 에서 호출 ---
    public void OnHurt()
    {
        animator.SetTrigger(hitTrig);
    }

    public void OnDie()
    {
        isDead = true;
        animator.SetTrigger(dieTrig);
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        this.enabled = false;
    }
}
