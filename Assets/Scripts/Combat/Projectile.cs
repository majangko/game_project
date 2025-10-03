using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int damage;
    private int direction;
    private float knockback;

    [Header("Settings")]
    public float speed = 10f;              // 투사체 이동 속도
    public float lifetime = 3f;            // 자동 파괴 시간
    public GameObject hitEffectPrefab;     // 충돌 시 이펙트
    public bool destroyOnHit = true;       // 맞으면 바로 파괴할지 여부

    [SerializeField] private LayerMask enemyMask;  // ✅ 인스펙터에서 설정 가능

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(int dmg, int dir, float kb, LayerMask mask)
    {
        damage = dmg;
        direction = dir;
        knockback = kb;

        // Inspector에서 설정한 값이 있으면 유지, Init에서 전달된 mask가 우선
        if (mask != 0)
            enemyMask = mask;

        // 발사 방향으로 속도 적용
        rb.linearVelocity = new Vector2(speed * direction, 0f);

        // 방향에 따라 스프라이트 반전
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (direction > 0 ? 1 : -1);
        transform.localScale = -scale;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyMask) != 0)
        {
            var target = other.GetComponent<Damageable>();
            if (target != null)
            {
                Vector2 knock = new Vector2(direction * knockback, knockback * 0.25f);
                target.TakeHit(damage, knock, transform.position);
            }

            if (hitEffectPrefab)
            {
                GameObject fx = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(fx, 0.5f);
            }

            if (destroyOnHit)
                Destroy(gameObject);
        }
    }
}
