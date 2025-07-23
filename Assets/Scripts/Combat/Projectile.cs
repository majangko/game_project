using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public GameObject hitEffectPrefab;

    private Vector2 moveDirection;
    private Rigidbody2D rb;

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 화면 벗어나면 삭제
        if (Mathf.Abs(transform.position.x) > 30f || Mathf.Abs(transform.position.y) > 20f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 이펙트 출력
            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

            // 적 피격 처리 로직이 있으면 여기에 작성

            Destroy(gameObject); // 투사체 파괴
        }
    }
}
