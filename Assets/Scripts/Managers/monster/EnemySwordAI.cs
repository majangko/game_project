using UnityEngine;
using System.Collections;

public class EnemySwordAI : MonoBehaviour, IEnemyAIEvents
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] Transform visualRoot;
    [SerializeField] Transform player;
    [SerializeField] AttackHitbox attackHitbox;

    [Header("Detect/Attack")]
    [SerializeField] float detectRadius = 6f;      // 플레이어 인식 범위
    [SerializeField] float attackRange = 1.5f;     // 검은 짧은 사거리
    [SerializeField] float attackCooldown = 2f;    // 콤보 쿨타임
    float lastAttackTime;

    [Header("Combo Attack")]
    [SerializeField] int maxCombo = 2;             // 연속 공격 횟수 (2~3 추천)
    [SerializeField] float comboDelay = 0.4f;      // 연속 공격 사이 딜레이
    int currentCombo = 0;

    [Header("Move")]
    [SerializeField] float moveSpeed = 3.5f;       // 빠른 이동 속도

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

        // 콤보 시작
        currentCombo = 0;
        StartCoroutine(ComboAttackRoutine());
        lastAttackTime = Time.time;
    }

    IEnumerator ComboAttackRoutine()
    {
        while (currentCombo < maxCombo)
        {
            animator.SetTrigger(attackTrig);

            // 실제 공격 판정 (애니메이션 이벤트에서 호출)
            yield return new WaitForSeconds(comboDelay);
            currentCombo++;
        }
    }

    // Animation Event에서 호출됨
    public void DoAttack()
    {
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
