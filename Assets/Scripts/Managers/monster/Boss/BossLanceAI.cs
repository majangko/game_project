using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossLanceAI : MonoBehaviour, IEnemyAIEvents
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Transform player;
    [SerializeField] private AttackHitbox attackHitbox;
    [SerializeField] private Damageable bossHealth;

    [Header("Detect/Attack")]
    [SerializeField] private float detectRadius = 7f;
    [SerializeField] private float attackRange = 2.8f;
    [SerializeField] private float attackCooldown = 2f;
    private float lastAttackTime;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float dashSpeed = 6f;
    [SerializeField] private float dashTime = 0.3f;

    [Header("Boss Pattern Settings")]
    [Tooltip("돌진 기마병 무리 패턴 쿨타임 (초)")]
    [SerializeField] private float patternCooldown = 10f;
    private float patternTimer = 0f;

    [Tooltip("돌진 기마병 그룹 프리팹")]
    [SerializeField] private GameObject chargePrefab;

    [Tooltip("기마병 생성 위치 (왼쪽/오른쪽)")]
    [SerializeField] private Transform[] chargeSpawnPoints;

    [Tooltip("패턴 발동 전 경고 이펙트 프리팹")]
    [SerializeField] private GameObject warningEffectPrefab;

    [Header("Rage Mode Settings")]
    [Tooltip("체력 절반 이하 시 발동")]
    [SerializeField] private float rageMultiplier = 1.5f;
    private bool isRaged = false;

    [Header("Animator Params")]
    [SerializeField] private string moveBool = "1_Move";
    [SerializeField] private string attackTrig = "2_Attack";
    [SerializeField] private string hitTrig = "3_Damage";
    [SerializeField] private string dieTrig = "4_Death";

    private bool isDead = false;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visualRoot) visualRoot = transform;
        if (!bossHealth) bossHealth = GetComponent<Damageable>();
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
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
        else StopMoving();

        FlipToPlayer();

        // 패턴 타이머
        patternTimer += Time.deltaTime;
        if (patternTimer >= patternCooldown)
        {
            patternTimer = 0f;
            StartCoroutine(PerformChargePattern());
        }

        // 광폭화 조건 (체력 50% 이하)
        if (!isRaged && bossHealth != null)
        {
            int current = bossHealth.GetCurrentHP();
            int max = bossHealth.maxHP;
            if (current <= max * 0.5f)
                EnterRageMode();
        }
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
        attackHitbox?.DoAttack();
    }

    void FlipToPlayer()
    {
        if (!player) return;
        Vector3 scale = visualRoot.localScale;
        scale.x = (player.position.x > transform.position.x) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        visualRoot.localScale = scale;
    }

    // 💥 돌진 패턴 (한쪽 랜덤, 방향 및 높이 보정, 수명 연장)
    IEnumerator PerformChargePattern()
    {
        // 한쪽만 랜덤으로 선택
        int side = Random.Range(0, chargeSpawnPoints.Length);
        Transform point = chargeSpawnPoints[side];

        // 1️⃣ 경고 이펙트
        if (warningEffectPrefab != null)
        {
            GameObject warn = Instantiate(warningEffectPrefab, point.position, Quaternion.identity);
            Destroy(warn, 1.5f);
        }

        yield return new WaitForSeconds(1.5f);

        // 2️⃣ 기마병 무리 생성
        GameObject horseGroup = Instantiate(chargePrefab, point.position + new Vector3(0, 1.5f, 0), Quaternion.identity); // Y +1.5 높이 보정

        // 왼쪽 포인트 → 오른쪽으로 / 오른쪽 포인트 → 왼쪽으로 이동
        float moveDir = (point.position.x < transform.position.x) ? 1f : -1f;

        // 프리팹 방향 보정
        Vector3 scale = horseGroup.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (moveDir > 0 ? 1 : -1);
        horseGroup.transform.localScale = scale;

        // Rigidbody2D 이동
        Rigidbody2D horseRb = horseGroup.GetComponent<Rigidbody2D>();
        if (horseRb != null)
            horseRb.linearVelocity = new Vector2(moveDir * 8f, 0);

        // 수명 연장 (5초 → 8초)
        Destroy(horseGroup, 8f);

        animator.SetTrigger("2_Attack");
    }

    void EnterRageMode()
    {
        isRaged = true;
        moveSpeed *= rageMultiplier;
        dashSpeed *= rageMultiplier;
        attackCooldown /= rageMultiplier;
        Debug.Log("보스 광폭화 상태 진입!");
    }

    // 인터페이스 구현
    public void OnHurt() => animator.SetTrigger(hitTrig);
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
