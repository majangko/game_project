using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public CharacterData data;
    public Transform firePoint;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Vector2.zero;
        if (Input.GetKey(KeyCode.LeftArrow))  moveInput.x = -1;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput.x = 1;
        if (Input.GetKey(KeyCode.UpArrow))    moveInput.y = 1;
        if (Input.GetKey(KeyCode.DownArrow))  moveInput.y = -1;
        moveInput = moveInput.normalized;

        if (moveInput != Vector2.zero) lastMoveDir = moveInput;

        if (Input.GetKeyDown(KeyCode.Space)) UseSkill();
    }

    void FixedUpdate()
    {
        #if UNITY_2022_2_OR_NEWER
        rb.linearVelocity = moveInput * data.moveSpeed;
        #else
        rb.velocity = moveInput * data.moveSpeed;
        #endif
    }

    void UseSkill()
    {
        if (data.skillPrefab != null && firePoint != null)
        {
            GameObject skill = Instantiate(data.skillPrefab, firePoint.position, Quaternion.identity);

            Rigidbody2D projRb = skill.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                #if UNITY_2022_2_OR_NEWER
                projRb.linearVelocity = lastMoveDir * 10f;
                #else
                projRb.velocity = lastMoveDir * 10f;
                #endif
            }
        }
    }
}
