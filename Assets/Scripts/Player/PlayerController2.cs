using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2 : MonoBehaviour
{
    public CharacterData data;           // 캐릭터 정보 (ScriptableObject)
    public Transform firePoint;          // 스킬 발사 위치

    [Header("Fallback (data 없을 때 사용)")]
    public float defaultMoveSpeed = 6f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.right;  // 기본 방향 →

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb) rb.freezeRotation = true;   // 회전 고정
    }

    void Update()
    {
        // 플랫폼러: 좌우만 입력
        float x = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))  x = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) x =  1f;
        moveInput = new Vector2(x, 0f).normalized;

        if (moveInput.sqrMagnitude > 0f)
            lastMoveDir = moveInput;

        if (Input.GetKeyDown(KeyCode.Space))
            UseSkill();
    }

    void FixedUpdate()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        float speed = (data != null) ? data.moveSpeed : defaultMoveSpeed;

        // y속도는 중력/점프 유지
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    void UseSkill()
    {
        if (data == null || data.skillPrefab == null || firePoint == null) return;

        GameObject skill = Instantiate(data.skillPrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D projRb = skill.GetComponent<Rigidbody2D>();
        if (projRb != null)
            projRb.linearVelocity = lastMoveDir * 10f;
    }
}
