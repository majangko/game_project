using UnityEngine;
using System.Collections;

public class EnemyLanceAI : MonoBehaviour, IEnemyAIEvents
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] Transform visualRoot;
    public Transform player;
    [SerializeField] AttackHitbox attackHitbox;

    [Header("Detect/Attack")]
    [SerializeField] float detectRadius = 6f;
    [SerializeField] float attackRange = 2.5f; // 창은 긴 사거리
    [SerializeField] float attackCooldown = 2f;
    float lastAttackTime;

    [Header("Move")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float dashSpeed = 6f;
    [SerializeField] float dashTime = 0.3f;

    [Header("Animator Params")]
    [SerializeField] string moveBool = "1_Move";
    [SerializeField] string attackTrig = "2_Attack";
    [SerializeField] string hitTrig = "3_Damage";
    [SerializeField] string dieTrig = "4_Death";

    bool isDead;
    public void SetPlayer(Transform t)
    {
        player = t;
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visualRoot) visualRoot = transform;
    }

    void Start()
    {
        // ✅ 플레이어 자동 탐색 (Player 태그 기반)
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
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

        FlipToPlayer();
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

        // 대시 돌진 후 공격 판정
        StartCoroutine(DashAttack());
    }

    IEnumerator DashAttack()
    {
        float start = Time.time;
        Vector2 dir = (player.position - transform.position).normalized;

        while (Time.time < start + dashTime)
        {
            rb.linearVelocity = new Vector2(dir.x * dashSpeed, rb.linearVelocity.y);
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // 히트박스 발동
        attackHitbox?.DoAttack();
    }

    void FlipToPlayer()
    {
        if (!player) return;

        Vector3 scale = visualRoot.localScale;

        if (player.position.x > transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);

        visualRoot.localScale = scale;

        // 공격 히트박스 방향도 같이 반전
        if (attackHitbox)
        {
            Vector3 hbScale = attackHitbox.transform.localScale;
            hbScale.x = Mathf.Sign(scale.x) * Mathf.Abs(hbScale.x);
            attackHitbox.transform.localScale = hbScale;
        }
    }

    // 인터페이스 구현부
    public void OnHurt()
    {
        if (!isDead && animator) animator.SetTrigger(hitTrig);
    }

    public void OnDie()
    {
        if (isDead) return;
        isDead = true;

        animator.SetTrigger(dieTrig);
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        this.enabled = false;
    }
}
