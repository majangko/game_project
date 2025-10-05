using UnityEngine;

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
    [SerializeField] private LayerMask tilemapMask; // ✅ 추가: 타일맵 인식용

    [Header("Flip Roots")]
    [SerializeField] private Transform flipRoot;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private bool spriteFacesRight = false;

    [Header("Attack (Common Settings)")]
    [SerializeField] private float attackMoveLock = 0.12f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackKnockback = 5f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Melee Attack Settings")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.2f, 0.8f);
    [SerializeField] private Vector2 attackBoxOffset = new Vector2(1f, 0.1f);
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Ranged Attack Settings")]
    [SerializeField] private bool isRanged = false;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private Transform projectileSpawnPoint;

    [HideInInspector] public float moveSpeedMul = 1f;
    [HideInInspector] public float attackPowerMul = 1f;

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

    private float lockUntil;
    private int desiredDir = 0;
    private float baseFlipAbsX = 1f;
    private string attackTriggerName = "2_Attack";

    public int FacingDir
    {
        get
        {
            if (!flipRoot) return desiredDir == 0 ? 1 : desiredDir;
            bool scaleRight = flipRoot.localScale.x >= 0f;
            return (scaleRight == spriteFacesRight) ? +1 : -1;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var spum = GetComponent<SPUM_Prefabs>();
        if (spum != null && spum.Anim)
            anim = spum.Anim;
        else
            anim = GetComponentInChildren<Animator>();

        rb.freezeRotation = true;

        if (!visualRoot && anim) visualRoot = anim.transform;

        if (!flipRoot)
        {
            var go = new GameObject("FlipRoot_Auto");
            flipRoot = go.transform;
            if (visualRoot) go.transform.SetParent(visualRoot.parent, true);
            else go.transform.SetParent(transform.parent, true);

            go.transform.position = transform.position;
            go.transform.localScale = Vector3.one;

            if (visualRoot) visualRoot.SetParent(go.transform, true);
            else transform.SetParent(go.transform, true);
        }

        baseFlipAbsX = Mathf.Abs(flipRoot.localScale.x);
        if (baseFlipAbsX < 0.0001f) baseFlipAbsX = 1f;
    }

    void Update()
    {
        // 이동
        float x = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) x = -1f;
        else if (Input.GetKey(KeyCode.RightArrow)) x = 1f;

        if (Time.time < lockUntil) x = 0f;

        float speed = moveSpeed * Mathf.Max(0.1f, moveSpeedMul);
        rb.linearVelocity = new Vector2(x * speed, rb.linearVelocity.y);

        if (Mathf.Abs(x) > 0.01f) desiredDir = x > 0 ? +1 : -1;

        // ✅ Ground + Tilemap 체크
        bool grounded = false;
        if (groundCheck)
        {
            int combinedMask = groundMask | tilemapMask;
            grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, combinedMask);
        }

        // Animator 파라미터
        if (anim)
        {
            anim.SetBool(P_MOVE_BOOL, Mathf.Abs(x) > 0.01f);
            if (HasParam(anim, P_IS_GROUNDED, AnimatorControllerParameterType.Bool))
                anim.SetBool(P_IS_GROUNDED, grounded);
            if (HasParam(anim, P_VERT_SPEED, AnimatorControllerParameterType.Float))
                anim.SetFloat(P_VERT_SPEED, rb.linearVelocity.y);

            // 공격
            if (Input.GetKeyDown(KeyCode.Z) && Time.time >= lockUntil)
            {
                anim.SetTrigger(attackTriggerName);
                lockUntil = Time.time + attackMoveLock;
                DoBasicAttack();
            }
        }

        // 점프
        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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
        float finalDamage = attackDamage * Mathf.Max(0.1f, attackPowerMul);

        foreach (var h in hits)
        {
            if (h.attachedRigidbody && h.attachedRigidbody.gameObject == this.gameObject) continue;

            var dmg = h.GetComponentInParent<Damageable>();
            if (dmg != null)
            {
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
        float finalDamage = attackDamage * Mathf.Max(0.1f, attackPowerMul);

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
        if (enhancedRemaining > 0) enhancedRemaining--;
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
