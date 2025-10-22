using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireballBouncer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float bounceDamping = 1f;
    public int damage = 10;

    private Rigidbody2D rb;
    private Vector2 moveDir;

    private Vector2 zoneCenter;
    private Vector2 zoneSize;
    private Damageable bossRef;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 🔸 초기 속도 설정
        moveDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        rb.linearVelocity = moveDir * moveSpeed;
    }

    private void Update()
    {
        // 🔹 보스 사망 시 삭제
        if (bossRef != null && bossRef.IsDead())
        {
            Destroy(gameObject);
            return;
        }

        // 🔹 영역 내에서만 반사
        Vector2 pos = transform.position;
        Vector2 half = zoneSize / 2f;
        Vector2 min = zoneCenter - half;
        Vector2 max = zoneCenter + half;

        if (pos.x <= min.x || pos.x >= max.x)
        {
            moveDir.x *= -1;
            rb.linearVelocity = moveDir * moveSpeed * bounceDamping;
            transform.position = new Vector2(Mathf.Clamp(pos.x, min.x, max.x), pos.y);
        }

        if (pos.y <= min.y || pos.y >= max.y)
        {
            moveDir.y *= -1;
            rb.linearVelocity = moveDir * moveSpeed * bounceDamping;
            transform.position = new Vector2(pos.x, Mathf.Clamp(pos.y, min.y, max.y));
        }
    }

    public void SetZoneArea(Vector2 center, Vector2 size)
    {
        zoneCenter = center;
        zoneSize = size;
    }

    public void SetBossReference(Damageable boss)
    {
        bossRef = boss;
    }

    // ✅ 충돌 감지 (Player만)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Layer로 필터링 (Player만 반응)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Damageable player = collision.collider.GetComponent<Damageable>();
            if (player != null)
            {
                Vector2 hitPoint = transform.position;
                Vector2 knockback = Vector2.zero;
                player.TakeHit(damage, knockback, hitPoint);
                Debug.Log($"[Fireball] Player hit! Damage {damage}");
            }
        }
    }
}
