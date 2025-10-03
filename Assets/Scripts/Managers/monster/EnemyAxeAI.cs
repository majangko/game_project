using UnityEngine;
using System.Collections;

public class EnemyAxeAI : MonoBehaviour, IEnemyAIEvents
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] Transform visualRoot;
    [SerializeField] Transform player;
    [SerializeField] AttackHitbox attackHitbox;

    [Header("Detect/Attack")]
    [SerializeField] float detectRadius = 6f;    // 플레이어 인식 범위
    [SerializeField] float attackRange = 1.8f;   // 도끼는 약간 긴 근접 범위
    [SerializeField] float attackCooldown = 3f;  // 공격 쿨타임 길게
    [SerializeField] float chargeTime = 0.6f;    // 내려찍기 전에 차징 시간
    float lastAttackTime;

    [Header("Move")]
    [SerializeField] float moveSpeed = 2.2f;     // 검병보다 느리게

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

        lastAttackTime = Time.time;
        StartCoroutine(ChargeAndAttack());
    }

    IEnumerator ChargeAndAttack()
    {
        // 차징 모션 (그냥 대기, 애니메이션도 대기 동작 가능)
        animator.SetBool(moveBool, false);

        // 차징 시간 대기
        yield return new WaitForSeconds(chargeTime);

        // 공격 모션 발동
        animator.SetTrigger(attackTrig);

        // DoAttack()은 애니메이션 이벤트에서 호출됨
    }

    // Animation Event에서 호출
    public void DoAttack()
    {
        if (attackHitbox != null)
        {
            // 넉백 값은 AttackHitbox Inspector에서 직접 설정
            attackHitbox.DoAttack();
        }
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

        // 히트박스 방향 반전
        if (attackHitbox)
        {
            Vector3 hbScale = attackHitbox.transform.localScale;
            hbScale.x = Mathf.Sign(scale.x) * Mathf.Abs(hbScale.x);
            attackHitbox.transform.localScale = hbScale;
        }
    }

    // 인터페이스 구현
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
        rb.bodyType = RigidbodyType2D.Kinematic;

        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        this.enabled = false;
    }
}
