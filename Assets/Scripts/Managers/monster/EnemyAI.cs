using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody2D rb;                    // Enemy_Wrapper의 Rigidbody2D
    public Animator animator;                 // UnitRoot(Animator)
    public Transform visualRoot;              // UnitRoot Transform (좌우 반전용)
    public Transform groundCheck;             // 바닥 체크 기준점
    public LayerMask groundMask;              // Ground 레이어
    public LayerMask playerMask;              // (인스펙터 표시용 유지)
    public Transform attackOrigin;            // 공격 기준점(선택)
    public AttackHitbox attackHitbox;         // AttackHitbox 컴포넌트

    [Header("Move/Patrol")]
    public float patrolSpeed = 0.8f;
    public float chaseSpeed = 2f;
    public float edgeCheckDistance = 0.2f;
    public float wallCheckDistance = 0.2f;
    public Transform leftBound;               // Patrol Left
    public Transform rightBound;              // Patrol Right

    [Header("Detect/Attack")]
    public float detectRadius = 6f;
    public float attackRange = 1.6f;
    public float loseRadius = 8f;
    public float attackCooldown = 1f;

    [Header("Animator Param Names")]
    public string moveBool = "1_Move";
    public string attackTrig = "2_Attack";
    public string hitTrig = "3_Damage";
    public string dieTrig = "4_Death";

    // --- 내부 상태 ---
    Transform target;
    float cooldownTimer;
    float baseScaleX = 1f;
    float lastDir = 1f;           // 마지막 이동 방향 기억
    float lastFlipTime = -999f;   // 마지막 Flip 시각

    [Header("Flip Settings")]
    public float flipCooldown = 0.3f; // 너무 빠른 좌우 전환 방지

    void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visualRoot) visualRoot = animator ? animator.transform : transform;
        baseScaleX = Mathf.Abs(visualRoot.localScale.x);
    }

    void Update()
    {
        AcquireTarget();

        if (target == null) Patrol();
        else
        {
            float distX = Mathf.Abs(target.position.x - transform.position.x);
            if (distX <= attackRange) { StopX(); TryAttack(); }
            else MoveToTarget();
        }

        animator?.SetBool(moveBool, Mathf.Abs(rb.linearVelocity.x) > 0.01f);
        cooldownTimer -= Time.deltaTime;
    }

    void AcquireTarget()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO)
        {
            float d = Vector2.Distance(transform.position, playerGO.transform.position);
            if (d <= detectRadius) target = playerGO.transform;
            else if (target && d > loseRadius) target = null;
        }
        else target = null;
    }

    void Patrol()
    {
        float dir = lastDir;

        if (rightBound && leftBound)
        {
            if (transform.position.x > rightBound.position.x) dir = -1f;
            else if (transform.position.x < leftBound.position.x) dir = 1f;
        }

        if (!IsGroundAhead(dir) || IsWallAhead(dir)) dir *= -1f;

        lastDir = dir; // 방향 기억
        rb.linearVelocity = new Vector2(dir * patrolSpeed, rb.linearVelocity.y);
        Face(dir);
    }

    void MoveToTarget()
    {
        float dir = Mathf.Sign(target.position.x - transform.position.x);

        if (!IsGroundAhead(dir) || IsWallAhead(dir)) { StopX(); return; }

        lastDir = dir; // 추적 시 방향 기억
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
        Face(dir);
    }

    void StopX() => rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

    bool IsGroundAhead(float dir)
    {
        Vector3 origin = groundCheck ? groundCheck.position : transform.position + Vector3.down * 0.1f;
        Vector2 castDir = (new Vector2(dir, -1f)).normalized;
        var hit = Physics2D.Raycast(origin, castDir, edgeCheckDistance, groundMask);
        Debug.DrawRay(origin, castDir * edgeCheckDistance, Color.green);
        return hit.collider != null;
    }

    bool IsWallAhead(float dir)
    {
        Vector3 origin = groundCheck ? groundCheck.position : transform.position;
        var hit = Physics2D.Raycast(origin, Vector2.right * dir, wallCheckDistance, groundMask);
        Debug.DrawRay(origin, Vector2.right * dir * wallCheckDistance, Color.red);
        return hit.collider != null;
    }

    void Face(float dir)
    {
        if (!visualRoot) return;

        float sign = dir >= 0 ? 1f : -1f;

        // 이미 같은 방향이면 무시
        if (Mathf.Sign(visualRoot.localScale.x) == sign) return;

        // 쿨타임 체크
        if (Time.time - lastFlipTime < flipCooldown) return;

        lastFlipTime = Time.time;

        var s = visualRoot.localScale;
        s.x = baseScaleX * sign;
        visualRoot.localScale = s;
    }

    void TryAttack()
    {
        if (cooldownTimer > 0f) return;

        animator?.SetTrigger(attackTrig);
        if (attackHitbox) attackHitbox.EnableOnce(0.12f);
        cooldownTimer = attackCooldown;
    }

    public void OnHurt()
    {
        if (!string.IsNullOrEmpty(hitTrig))
            animator?.SetTrigger(hitTrig);
    }

    public void OnDie()
    {
        StopX();
        if (!string.IsNullOrEmpty(dieTrig))
            animator?.SetTrigger(dieTrig);

        enabled = false;
    }
}
