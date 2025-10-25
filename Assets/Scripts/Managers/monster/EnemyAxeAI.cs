using UnityEngine;
using System.Collections;

public class EnemyAxeAI : MonoBehaviour, IEnemyAIEvents
{
    [Header("References")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] Transform visualRoot;
    public Transform player;
    [SerializeField] AttackHitbox attackHitbox;

    [Header("Detect/Attack")]
    [SerializeField] float detectRadius = 6f;
    [SerializeField] float attackRange = 1.8f;
    [SerializeField] float attackCooldown = 3f;
    [SerializeField] float chargeTime = 0.6f;
    [SerializeField] float swingDelay = 0.25f;   // ⚡ 공격 판정 나가는 시점 (애니메이션 타이밍용)
    float lastAttackTime;

    [Header("Move")]
    [SerializeField] float moveSpeed = 2.2f;

    [Header("Animator Params")]
    [SerializeField] string moveBool = "1_Move";
    [SerializeField] string attackTrig = "2_Attack";
    [SerializeField] string hitTrig = "3_Damage";
    [SerializeField] string dieTrig = "4_Death";

    bool isDead;
    public void SetPlayer(Transform t)
    {
        player = t;
        if (player != null)
            Debug.Log($"[EnemyAxeAI] {gameObject.name} → Player 연결 완료: {player.name}");
        else
            Debug.LogWarning($"[EnemyAxeAI] {gameObject.name} → Player 연결 실패!");
    }


    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visualRoot) visualRoot = transform;
    }

    void Start()
    {
        // 🟢 자동으로 플레이어 찾기
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
            else
                Debug.LogWarning("[EnemyAxeAI] No GameObject with tag 'Player' found!");
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

        lastAttackTime = Time.time;
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        animator.SetBool(moveBool, false);
        animator.SetTrigger(attackTrig);

        // ⚡ 애니메이션 시작 후 약간의 준비시간 (chargeTime)
        yield return new WaitForSeconds(chargeTime);

        // ⚡ 휘두르는 순간 타이밍 (swingDelay)
        yield return new WaitForSeconds(swingDelay);

        if (attackHitbox != null)
        {
            Debug.Log("[EnemyAxeAI] 자동 공격 발동!");
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

        if (attackHitbox)
        {
            Vector3 hbScale = attackHitbox.transform.localScale;
            hbScale.x = Mathf.Sign(scale.x) * Mathf.Abs(hbScale.x);
            attackHitbox.transform.localScale = hbScale;
        }
    }

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
