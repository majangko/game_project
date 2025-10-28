using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SpumPlatformerController : MonoBehaviour
{
    [Header("Move / Jump")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.18f);
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask tilemapMask;

    [Header("Flip Roots")]
    [SerializeField] private Transform flipRoot;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private bool spriteFacesRight = false;

    [Header("Attack (Common Settings)")]
    [SerializeField] private float attackMoveLock = 0.12f;
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackKnockback = 5f;
    [Header("Attack Settings")]
    public LayerMask enemyMask;
    public LayerMask EnemyMask => enemyMask;

    [Header("Melee Attack Settings")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.2f, 0.8f);
    [SerializeField] private Vector2 attackBoxOffset = new Vector2(1f, 0.1f);
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Attack Sounds")]
    [SerializeField] private AudioClip swingSound;
    [SerializeField] private AudioClip hitSound;

    [Header("Ranged Attack Settings")]
    [SerializeField] private bool isRanged = false;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private Transform projectileSpawnPoint;

    [HideInInspector] public float moveSpeedMul = 1f;
    [HideInInspector] public float attackPowerMul = 1f;
    [HideInInspector] public float attackSpeedMul = 1f;
    [HideInInspector] public float defenseMul = 1f;
    [HideInInspector] public float regenRate = 0f;

    [HideInInspector] public bool canMove = true;

    private int enhancedRemaining = 0;
    private GameObject enhancedExplosionPrefab;
    private int enhancedExplosionDamage;
    private float enhancedExplosionRadius;
    private LayerMask enhancedMask;

    private const string P_MOVE_BOOL = "1_Move";
    private const string P_IS_GROUNDED = "IsGrounded";
    private const string P_VERT_SPEED = "VerticalSpeed";

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerStats stats; // ✅ PlayerStats 캐싱

    private float lockUntil;
    private int desiredDir = 0;
    private float baseFlipAbsX = 1f;
    private string attackTriggerName = "2_Attack";
    private float nextAttackTime = 0f;

    public int FacingDir
    {
        get
        {
            if (!flipRoot) return desiredDir == 0 ? 1 : desiredDir;
            bool scaleRight = flipRoot.localScale.x >= 0f;
            return (scaleRight == spriteFacesRight) ? +1 : -1;
        }
    }

    // ============================================================
    // Awake
    // ============================================================
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        var spum = GetComponent<SPUM_Prefabs>();
        anim = spum != null && spum.Anim ? spum.Anim : GetComponentInChildren<Animator>();
        rb.freezeRotation = true;

        // ✅ PlayerStats 자동 탐색 및 이벤트 구독
        if (TryGetComponent(out stats))
        {
            // 공격력 배율이 바뀌면 자동 반영
            stats.OnAttackMultiplierChanged += (mult) =>
            {
                attackPowerMul = mult;
            };
        }

        // 루트 설정
        if (!flipRoot)
        {
            if (anim != null)
                flipRoot = anim.transform.parent;
            else
                flipRoot = transform;
        }

        if (!visualRoot && anim)
            visualRoot = anim.transform;

        baseFlipAbsX = Mathf.Abs(flipRoot.localScale.x);
        if (baseFlipAbsX < 0.0001f) baseFlipAbsX = 1f;
    }

    // ============================================================
    // Update
    // ============================================================
    void Update()
    {
        if (!canMove)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (anim)
                anim.SetBool(P_MOVE_BOOL, false);
            return;
        }

        float x = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) x = -1f;
        else if (Input.GetKey(KeyCode.RightArrow)) x = 1f;
        if (Time.time < lockUntil) x = 0f;

        // ✅ [ADD] PlayerStats의 런타임 속도 배율을 곱해준다.
        float playerSpeedMul = 1f;
        if (stats != null)
            playerSpeedMul = stats.CurrentSpeedMultiplier; // (= 1 + 누적 속도%)

        float speed = moveSpeed * Mathf.Max(0.1f, moveSpeedMul) * playerSpeedMul; // ✅ [CHANGED]
        rb.linearVelocity = new Vector2(x * speed, rb.linearVelocity.y);
        if (Mathf.Abs(x) > 0.01f) desiredDir = x > 0 ? +1 : -1;

        bool grounded = false;
        if (groundCheck)
        {
            int combinedMask = groundMask | tilemapMask;
            grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, combinedMask);
        }

        // Animator
        if (anim)
        {
            anim.SetBool(P_MOVE_BOOL, Mathf.Abs(x) > 0.01f);
            if (HasParam(anim, P_IS_GROUNDED, AnimatorControllerParameterType.Bool))
                anim.SetBool(P_IS_GROUNDED, grounded);
            if (HasParam(anim, P_VERT_SPEED, AnimatorControllerParameterType.Float))
                anim.SetFloat(P_VERT_SPEED, rb.linearVelocity.y);

            if (Input.GetKeyDown(KeyCode.Z) && Time.time >= nextAttackTime)
            {
                anim.SetTrigger(attackTriggerName);
                lockUntil = Time.time + attackMoveLock;
                float realCooldown = attackCooldown / Mathf.Max(0.1f, attackSpeedMul);
                nextAttackTime = Time.time + realCooldown;
                DoBasicAttack();
            }
        }

        // 점프
        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // ✅ 초당 회복 버프
        if (regenRate > 0 && stats != null)
        {
            stats.Heal(Mathf.RoundToInt(regenRate * Time.deltaTime));
        }
    }

    void LateUpdate()
    {
        if (!flipRoot) return;
        int want = 0;
        if (desiredDir != 0)
        {
            bool moveRight = desiredDir > 0;
            want = (spriteFacesRight == moveRight) ? +1 : -1;
        }

        var s = flipRoot.localScale;
        float sign = (want == 0) ? Mathf.Sign(s.x) : want;
        flipRoot.localScale = new Vector3(baseFlipAbsX * sign, s.y, s.z);
    }

    // ============================================================
    // 기본 공격 처리
    // ============================================================
    void DoBasicAttack()
    {
        if (isRanged) DoRangedAttack();
        else DoMeleeAttack();
    }

    void DoMeleeAttack()
    {
        int dir = FacingDir;
        Vector2 center = (Vector2)(attackOrigin ? attackOrigin.position : transform.position)
                         + new Vector2(attackBoxOffset.x * dir, attackBoxOffset.y);
        var hits = Physics2D.OverlapBoxAll(center, attackBoxSize, 0f, enemyMask);

        if (SoundManager.Instance && swingSound)
            SoundManager.Instance.PlaySFX(swingSound);

        // ✅ PlayerStats 기반 공격력 계산
        float finalDamage = attackDamage;
        if (stats != null)
            finalDamage = stats.GetAttackPower() * Mathf.Max(0.1f, attackPowerMul);

        foreach (var h in hits)
        {
            var dmg = h.GetComponentInParent<Damageable>();
            if (dmg != null)
            {
                if (SoundManager.Instance && hitSound)
                    SoundManager.Instance.PlaySFX(hitSound);

                Vector2 knock = new Vector2(dir * attackKnockback, attackKnockback * 0.25f);
                dmg.TakeHit(Mathf.RoundToInt(finalDamage), knock, h.transform.position);

                if (hitEffectPrefab)
                {
                    var hitFx = Instantiate(hitEffectPrefab, h.transform.position, Quaternion.identity);
                    Destroy(hitFx, 0.3f);
                }
            }
        }
    }

    void DoRangedAttack()
    {
        int dir = FacingDir;

        // ✅ PlayerStats 기반 공격력 계산
        float finalDamage = attackDamage;
        if (stats != null)
            finalDamage = stats.GetAttackPower() * Mathf.Max(0.1f, attackPowerMul);

        if (SoundManager.Instance && swingSound)
            SoundManager.Instance.PlaySFX(swingSound);

        if (projectilePrefab && projectileSpawnPoint)
        {
            GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            SpriteRenderer sr = proj.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.flipX = (dir == -1);

            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
            {
                p.Init(Mathf.RoundToInt(finalDamage), dir, attackKnockback, enemyMask,
                       enhancedRemaining, enhancedExplosionPrefab, enhancedExplosionDamage,
                       enhancedExplosionRadius, enhancedMask, this);
            }
        }
    }

    // ============================================================
    // 강화 공격 관련
    // ============================================================
    public void SetEnhancedAttack(int count, GameObject prefab, int dmg, float radius, LayerMask mask)
    {
        enhancedRemaining = count;
        enhancedExplosionPrefab = prefab;
        enhancedExplosionDamage = dmg;
        enhancedExplosionRadius = radius;
        enhancedMask = mask;
    }

    public void ConsumeEnhanced()
    {
        if (enhancedRemaining > 0)
            enhancedRemaining--;
    }

    // ============================================================
    // 버프 코루틴 (공통)
    // ============================================================
    public void ApplyAttackSpeedBuff(float multiplier, float duration)
        => StartCoroutine(ApplyBuffCoroutine(BuffType.AttackSpeed, multiplier, duration));

    public void ApplyMoveSpeedBuff(float multiplier, float duration)
        => StartCoroutine(ApplyBuffCoroutine(BuffType.MoveSpeed, multiplier, duration));

    public void ApplyDefenseBuff(float multiplier, float duration)
        => StartCoroutine(ApplyBuffCoroutine(BuffType.DefenseUp, multiplier, duration));

    public void ApplyRegenBuff(float regenPerSecond, float duration)
        => StartCoroutine(RegenBuffCoroutine(regenPerSecond, duration));

    private IEnumerator ApplyBuffCoroutine(BuffType type, float multiplier, float duration)
    {
        var buffUI = FindObjectOfType<BuffUIController>();
        if (buffUI)
        {
            BuffData data = new BuffData
            {
                type = type,
                duration = duration,
                multiplier = multiplier,
                icon = Resources.Load<Sprite>($"UI/Buffs/icon_{type.ToString().ToLower()}")
            };
            buffUI.ShowBuff(data);
        }

        switch (type)
        {
            case BuffType.AttackSpeed: attackSpeedMul *= multiplier; break;
            case BuffType.MoveSpeed: moveSpeedMul *= multiplier; break;
            case BuffType.DefenseUp: defenseMul *= multiplier; break;
        }

        yield return new WaitForSeconds(duration);

        switch (type)
        {
            case BuffType.AttackSpeed: attackSpeedMul /= multiplier; break;
            case BuffType.MoveSpeed: moveSpeedMul /= multiplier; break;
            case BuffType.DefenseUp: defenseMul /= multiplier; break;
        }

        if (buffUI) buffUI.HideBuff(type);
    }

    private IEnumerator RegenBuffCoroutine(float regenPerSecond, float duration)
    {
        var buffUI = FindObjectOfType<BuffUIController>();
        if (buffUI)
        {
            BuffData data = new BuffData
            {
                type = BuffType.Regen,
                duration = duration,
                multiplier = regenPerSecond,
                icon = Resources.Load<Sprite>("UI/Buffs/icon_regen")
            };
            buffUI.ShowBuff(data);
        }

        regenRate += regenPerSecond;
        yield return new WaitForSeconds(duration);
        regenRate -= regenPerSecond;

        if (buffUI) buffUI.HideBuff(BuffType.Regen);
    }

    static bool HasParam(Animator a, string name, AnimatorControllerParameterType type)
    {
        foreach (var p in a.parameters)
            if (p.name == name && p.type == type) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        int dir = Application.isPlaying ? FacingDir : 1;
        Vector2 center = (Vector2)(attackOrigin ? attackOrigin.position : transform.position)
                         + new Vector2(attackBoxOffset.x * dir, attackBoxOffset.y);

        if (!isRanged)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            Gizmos.DrawCube(center, attackBoxSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, attackBoxSize);
        }
        else
        {
            if (projectileSpawnPoint)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(projectileSpawnPoint.position, 0.15f);
            }
        }
    }
}
