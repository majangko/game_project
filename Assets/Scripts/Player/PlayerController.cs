using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public CharacterData data;                    // 캐릭터 정보 (ScriptableObject)
    public Transform firePoint;                   // 투사체 발사 위치
    public GameObject attackZone;                 // 근접 공격 범위 오브젝트
    public GameObject hitEffectPrefab;            // 히트 이펙트 프리팹
    public GameObject projectilePrefab;           // 원거리 공격 투사체 프리팹

    public float jumpForce = 5f;                  // 점프 힘
    public float projectileSpeed = 10f;           // 투사체 속도
    public Transform groundCheck;                 // 땅 체크용 위치
    public LayerMask groundLayer;                 // Ground 레이어

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.right;

    private bool isAttacking = false;
    private float attackDuration = 0.2f;
    private float attackTimer = 0f;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (attackZone != null)
            attackZone.SetActive(false);
    }

    void Update()
    {
        HandleInput();
        HandleAttack();
        HandleJump();
        HandleShoot();
    }

    void FixedUpdate()
    {
        // 좌우 이동만 적용 (y속도는 유지)
        rb.linearVelocity = new Vector2(moveInput.x * data.moveSpeed, rb.linearVelocity.y);
    }

    void HandleInput()
    {
        moveInput = Vector2.zero;

        if (Input.GetKey(KeyCode.LeftArrow))
            moveInput.x = -1;
        if (Input.GetKey(KeyCode.RightArrow))
            moveInput.x = 1;

        moveInput = moveInput.normalized;

        if (moveInput.x != 0)
        {
            lastMoveDir = new Vector2(moveInput.x, 0); // 좌우 방향만 저장
        }
    }

    void HandleJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        Debug.Log("🧱 isGrounded: " + isGrounded);

        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Debug.Log("⬆ 점프!");
        }
    }

    void HandleShoot()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (projectilePrefab != null && firePoint != null)
            {
                GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                Rigidbody2D rbProj = projectile.GetComponent<Rigidbody2D>();

                if (rbProj != null)
                {
                    Vector2 shootDir = lastMoveDir;
                    if (shootDir == Vector2.zero)
                        shootDir = Vector2.right;

                    rbProj.linearVelocity = shootDir.normalized * projectileSpeed;
                }

                Debug.Log("🎯 원거리 공격!");
            }
            else
            {
                Debug.LogWarning("⚠ projectilePrefab 또는 firePoint가 연결되지 않았습니다!");
            }
        }
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.X) && !isAttacking)
        {
            StartAttack();
        }

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                EndAttack();
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            UseSkill();
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;

        if (attackZone != null)
            attackZone.SetActive(true);

        if (hitEffectPrefab != null)
        {
            Vector3 effectPos = transform.position + (Vector3)lastMoveDir * 0.5f;
            effectPos.z = 0f;
            Instantiate(hitEffectPrefab, effectPos, Quaternion.identity);
        }

        Debug.Log("▶ 근접 공격 시작");
    }

    void EndAttack()
    {
        isAttacking = false;

        if (attackZone != null)
            attackZone.SetActive(false);

        Debug.Log("◼ 근접 공격 종료");
    }

    void UseSkill()
    {
        if (data.skillPrefab != null && firePoint != null)
        {
            GameObject skill = Instantiate(data.skillPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projRb = skill.GetComponent<Rigidbody2D>();

            if (projRb != null)
            {
                projRb.linearVelocity = lastMoveDir.normalized * 10f;
            }

            Debug.Log("🌀 스킬 사용");
        }
    }
}
