using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private int damage = 10;          // 화살 데미지
    [SerializeField] private float speed = 8f;         // 화살 속도
    [SerializeField] private float lifeTime = 3f;      // 화살 지속 시간
    [SerializeField] private Vector2 knockback = new Vector2(2f, 1f); // 피격시 넉백

    [Header("Target Settings")]
    [SerializeField] private LayerMask targetMask;     // 맞을 대상 (예: Player)

    private Rigidbody2D rb;
    private Vector2 shootDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // 화살은 lifeTime 이후 자동 파괴
        Destroy(gameObject, lifeTime);

        // 초기 속도 적용 (이미 shootDir이 설정돼 있으면 사용)
        if (shootDir != Vector2.zero)
        {
            rb.linearVelocity = shootDir * speed;
            RotateToDirection(shootDir);
        }
    }

    /// <summary>
    /// 발사 방향을 외부에서 지정
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        shootDir = dir.normalized;

        if (rb != null)
        {
            rb.linearVelocity = shootDir * speed;
            RotateToDirection(shootDir);
        }
    }

    void RotateToDirection(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 타겟 마스크에 포함된 경우에만 충돌 처리
        if (((1 << collision.gameObject.layer) & targetMask) != 0)
        {
            Damageable dmg = collision.GetComponent<Damageable>();
            if (dmg != null)
            {
                dmg.TakeHit(damage, knockback, transform.position);
            }

            Destroy(gameObject);
        }
    }
}
