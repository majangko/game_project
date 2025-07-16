using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public CharacterData data;           // 캐릭터 정보 (ScriptableObject)
    public Transform firePoint;          // 스킬 발사 위치

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;  // 기본 방향 ↓

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Vector2.zero;

        // 방향키 입력만 허용
        if (Input.GetKey(KeyCode.LeftArrow))
            moveInput.x = -1;
        if (Input.GetKey(KeyCode.RightArrow))
            moveInput.x = 1;
        if (Input.GetKey(KeyCode.UpArrow))
            moveInput.y = 1;
        if (Input.GetKey(KeyCode.DownArrow))
            moveInput.y = -1;

        moveInput = moveInput.normalized;

        if (moveInput != Vector2.zero)
        {
            lastMoveDir = moveInput;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseSkill();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * data.moveSpeed;
    }

    void UseSkill()
    {
        if (data.skillPrefab != null && firePoint != null)
        {
            GameObject skill = Instantiate(data.skillPrefab, firePoint.position, Quaternion.identity);

            Rigidbody2D projRb = skill.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = lastMoveDir * 10f;
            }
        }
    }
}
