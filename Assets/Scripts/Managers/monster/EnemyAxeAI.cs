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
    [SerializeField] float detectRadius = 6f;
    [SerializeField] float attackRange = 1.8f;
    [SerializeField] float attackCooldown = 3f;
    [SerializeField] float chargeTime = 0.6f;
    float lastAttackTime;

    [Header("Move")]
    [SerializeField] float moveSpeed = 2.2f;

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

    void Start()
    {
        // 🟢 "Player" 태그로 자동 탐색
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
        StartCoroutine(ChargeAndAttack());
    }

    IEnumerator ChargeAndAttack()
    {
        animator.SetBool(moveBool, false);
        yield return new WaitForSeconds(chargeTime);
        animator.SetTrigger(attackTrig);
    }

    public void DoAttack()
    {
        if (attackHitbox != null)
            attackHitbox.DoAttack();
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
