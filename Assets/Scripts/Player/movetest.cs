// SpumPlatformerController.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SpumPlatformerController : MonoBehaviour
{
    [Header("Move / Jump")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.18f);
    public LayerMask groundMask;

    [Header("Flip Roots")]
    [Tooltip("좌우 반전을 적용할 '빈 부모' Transform (FlipRoot). 없어도 런타임에 자동 생성됩니다.")]
    public Transform flipRoot;
    [Tooltip("실제 애니메이션 본의 루트(보통 UnitRoot/Root). 미지정 시 Animator의 transform 사용.")]
    public Transform visualRoot;

    [Tooltip("원본 리소스가 기본적으로 오른쪽을 보고 있으면 체크. (SPUM 기본은 왼쪽, 그러므로 보통 해제)")]
    public bool spriteFacesRight = false;

    [Header("Attack (basic anim trigger only)")]
    public float attackMoveLock = 0.12f;
    string attackTriggerName = null;

    // ---- Buff multipliers (스킬이 조정) ----
    [HideInInspector] public float moveSpeedMul = 1f;
    [HideInInspector] public float attackPowerMul = 1f;

    // Animator params
    const string P_MOVE_BOOL = "1_Move";
    const string P_IS_GROUNDED = "IsGrounded";
    const string P_VERT_SPEED = "VerticalSpeed";

    Rigidbody2D rb;
    Animator anim;

    float lockUntil;
    int desiredDir = 0; // -1,0,+1
    float baseFlipAbsX = 1f;

    public int FacingDir
    {
        get
        {
            if (!flipRoot) return desiredDir == 0 ? 1 : desiredDir;
            bool scaleRight = flipRoot.localScale.x >= 0f;
            // 스케일의 부호와 '원본이 오른쪽을 보는가'를 대조해 세계 기준 방향 산출
            return (scaleRight == spriteFacesRight) ? +1 : -1;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            if (visualRoot) visualRoot.SetParent(go.transform, true);
            else transform.SetParent(go.transform, true);
        }

        baseFlipAbsX = Mathf.Abs(flipRoot.localScale.x);
        if (baseFlipAbsX < 0.0001f) baseFlipAbsX = 1f;

        // --- 기본 공격 트리거 자동 탐색(없어도 OK) ---
        if (anim)
        {
            string[] candidates = { "Attack", "1_Attack", "2_Attack", "Attack_Trigger", "ATTACK" };
            foreach (var c in candidates)
                if (HasParam(anim, c, AnimatorControllerParameterType.Trigger)) { attackTriggerName = c; break; }
        }
    }

    void Update()
    {
        // 좌우 입력
        float x = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) x = -1f;
        else if (Input.GetKey(KeyCode.RightArrow)) x = 1f;

        if (Time.time < lockUntil) x = 0f;

        float speed = moveSpeed * Mathf.Max(0.1f, moveSpeedMul);
        rb.linearVelocity = new Vector2(x * speed, rb.linearVelocity.y);

        if (Mathf.Abs(x) > 0.01f) desiredDir = x > 0 ? +1 : -1;

        // 접지
        bool grounded = false;
        if (groundCheck)
            grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundMask);

        // 애니메이터 파라미터
        if (anim)
        {
            anim.SetBool(P_MOVE_BOOL, Mathf.Abs(x) > 0.01f);
            if (HasParam(anim, P_IS_GROUNDED, AnimatorControllerParameterType.Bool)) anim.SetBool(P_IS_GROUNDED, grounded);
            if (HasParam(anim, P_VERT_SPEED, AnimatorControllerParameterType.Float)) anim.SetFloat(P_VERT_SPEED, rb.linearVelocity.y);

            // (선택) 기본 공격 애니 트리거
            if (Input.GetKeyDown(KeyCode.Z) && !string.IsNullOrEmpty(attackTriggerName))
            {
                anim.SetTrigger(attackTriggerName);
                lockUntil = Time.time + attackMoveLock;
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

    static bool HasParam(Animator a, string name, AnimatorControllerParameterType type)
    {
        foreach (var p in a.parameters) if (p.name == name && p.type == type) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
