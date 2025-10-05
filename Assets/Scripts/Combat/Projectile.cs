using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int damage;
    private int direction;
    private float knockback;
    private int enhancedRemaining;
    private GameObject explosionPrefab;
    private int explosionDamage;
    private float explosionRadius;
    private LayerMask explosionMask;
    private SpumPlatformerController owner;

    [Header("Settings")]
    public float speed = 10f;
    public float lifetime = 3f;
    public GameObject hitEffectPrefab;
    public bool destroyOnHit = true;

    [SerializeField] private LayerMask enemyMask;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(int dmg, int dir, float kb, LayerMask mask,
                     int enhancedCount = 0, GameObject explPrefab = null,
                     int explDamage = 0, float explRadius = 0f, LayerMask explMask = default,
                     SpumPlatformerController ctrl = null)
    {
        damage = dmg;
        direction = dir;
        knockback = kb;
        if (mask != 0) enemyMask = mask;

        enhancedRemaining = enhancedCount;
        explosionPrefab = explPrefab;
        explosionDamage = explDamage;
        explosionRadius = explRadius;
        explosionMask = explMask;
        owner = ctrl;

        rb.linearVelocity = new Vector2(speed * direction, 0f);

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (direction > 0 ? 1 : -1);
        transform.localScale = scale;

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

            // 일반 피격 이펙트
            if (hitEffectPrefab)
            {
                GameObject fx = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(fx, 0.5f);
            }

            // 강화 폭발 처리
            if (enhancedRemaining > 0 && explosionPrefab != null)
            {
                owner?.ConsumeEnhanced(); // 남은 횟수 차감

                GameObject expl = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(expl, 0.5f);

                Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, explosionRadius, explosionMask);
                foreach (var c in cols)
                {
                    Damageable d = c.GetComponent<Damageable>();
                    if (d != null)
                        d.TakeHit(explosionDamage, Vector2.zero, transform.position);
                }
            }

            if (destroyOnHit) Destroy(gameObject);
        }
    }
}
