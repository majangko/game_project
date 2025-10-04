using UnityEngine;

public class EnemyArcherAI : MonoBehaviour, IEnemyAIEvents
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform player;
    [SerializeField] private Transform arrowSpawnPoint;  // 화살 생성 위치
    [SerializeField] private GameObject arrowPrefab;     // 화살 프리팹

    [Header("Detect/Attack")]
    [SerializeField] private float detectRadius = 8f;   // 인식 범위
    [SerializeField] private float attackRange = 6f;    // 사거리
    [SerializeField] private float attackCooldown = 2f; // 공격 쿨타임
    [SerializeField] private float shootDelay = 0.3f;   // 화살 발사 딜레이
    private float lastAttackTime;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("Animator Params")]
    [SerializeField] private string moveBool = "1_Move";
    [SerializeField] private string attackTrig = "2_Attack";
    [SerializeField] private string hitTrig = "3_Damage";
    [SerializeField] private string dieTrig = "4_Death";

    private bool isDead;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visualRoot) visualRoot = transform;
    }

    void Start()
    {
        // ✅ Player 태그 기반 자동 탐색
        if (!player)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
                player = foundPlayer.transform;
            else
                Debug.LogWarning("[EnemyArcherAI] No GameObject with tag 'Player' found!");
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

        // 🔹 화살 발사 딜레이 후 실행
        Invoke(nameof(ShootArrow), shootDelay);

        lastAttackTime = Time.time;
    }

    void ShootArrow()
    {
        if (arrowPrefab != null && arrowSpawnPoint != null && player != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);

            // 방향 계산
            Vector2 dir = (player.position - arrowSpawnPoint.position).normalized;

            // Arrow.cs에 방향 전달
            Arrow arrowScript = arrow.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                arrowScript.SetDirection(dir);
            }
        }
    }

    void FlipToPlayer()
    {
        if (!player) return;

        Vector3 scale = visualRoot.localScale;

        if (player.position.x > transform.position.x)
            scale.x = -Mathf.Abs(scale.x); // 오른쪽 바라봄
        else
            scale.x = Mathf.Abs(scale.x);  // 왼쪽 바라봄

        visualRoot.localScale = scale;
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
