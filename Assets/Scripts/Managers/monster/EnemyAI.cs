using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] Transform visualRoot;
    [SerializeField] Transform player; // 추적할 플레이어

    [Header("Detect/Attack")]
    [SerializeField] float detectRadius = 6f;
    [SerializeField] float attackRange = 1.6f;
    [SerializeField] float attackCooldown = 1f;
    float lastAttackTime;

    [Header("Move")]
    [SerializeField] float moveSpeed = 2f;
    private float originalSpeed;
    private bool isLocked = false; // 완전 고정 여부
    private bool isSlowed = false; // 슬로우 상태 여부
    private float slowRatio = 1f;

    [Header("Boss Settings")]
    public bool isBoss = false; // 보스 판정용

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
        originalSpeed = moveSpeed;
    }

    void Update()
    {
        if (isDead || !player) return;

        if (isLocked)
        {
            StopMoving();
            return;
        }

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
        rb.linearVelocity = new Vector2(dir.x * (moveSpeed * slowRatio), rb.linearVelocity.y);
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

        if (player.position.x > transform.position.x)
            scale.x = -Mathf.Abs(scale.x); // 오른쪽
        else
            scale.x = Mathf.Abs(scale.x);  // 왼쪽

        visualRoot.localScale = scale;
    }

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
        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;
        this.enabled = false;
    }

    // ----------------------------------------------------
    // 🔒 결계 제어용 함수 추가
    // ----------------------------------------------------

    /// <summary>
    /// 일정 시간 동안 이동 완전 정지 (일반 몬스터용)
    /// </summary>
    public void LockMovement(float duration)
    {
        if (isLocked) return;
        isLocked = true;
        StopMoving();
        animator.SetBool(moveBool, false);
        Invoke(nameof(UnlockMovement), duration);
    }

    private void UnlockMovement()
    {
        isLocked = false;
    }

    /// <summary>
    /// 일정 시간 동안 이동속도 감소 (보스용)
    /// </summary>
    public void ApplySlow(float ratio, float duration)
    {
        if (isSlowed) return;
        isSlowed = true;
        slowRatio = Mathf.Clamp01(ratio);
        Invoke(nameof(RemoveSlow), duration);
    }

    private void RemoveSlow()
    {
        isSlowed = false;
        slowRatio = 1f;
    }
}
