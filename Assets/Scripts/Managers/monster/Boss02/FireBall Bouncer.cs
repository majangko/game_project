using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireballBouncer : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;        // 이동 속도
    public float bounceDamping = 1f;    // 반사 감쇠 (1 = 완벽 반사)
    public int damage = 10;             // 피해량

    private Rigidbody2D rb;
    private Vector2 moveDir;

    // 🔹 Fireball Zone이 전달하는 가상 벽 영역 정보
    private Vector2 zoneCenter;
    private Vector2 zoneSize;
    private Damageable bossRef;         // 보스 Damageable 참조

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 🔸 랜덤한 방향으로 초기 속도 설정
        moveDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        rb.linearVelocity = moveDir * moveSpeed;
    }

    private void Update()
    {
        // 🔹 보스가 죽으면 Fireball 제거
        if (bossRef != null && bossRef.IsDead())
        {
            Destroy(gameObject);
            return;
        }

        // 🔸 현재 위치와 가상 벽 경계 계산
        Vector2 pos = transform.position;
        Vector2 halfSize = zoneSize / 2f;
        Vector2 min = zoneCenter - halfSize;
        Vector2 max = zoneCenter + halfSize;

        // X축 경계 반사
        if (pos.x <= min.x || pos.x >= max.x)
        {
            moveDir.x *= -1;
            rb.linearVelocity = moveDir * moveSpeed * bounceDamping;
            transform.position = new Vector2(Mathf.Clamp(pos.x, min.x, max.x), pos.y);
        }

        // Y축 경계 반사
        if (pos.y <= min.y || pos.y >= max.y)
        {
            moveDir.y *= -1;
            rb.linearVelocity = moveDir * moveSpeed * bounceDamping;
            transform.position = new Vector2(pos.x, Mathf.Clamp(pos.y, min.y, max.y));
        }
    }

    // 🔹 FireballZone이 영역 정보 전달
    public void SetZoneArea(Vector2 center, Vector2 size)
    {
        zoneCenter = center;
        zoneSize = size;
    }

    // 🔹 보스 Damageable 전달
    public void SetBossReference(Damageable boss)
    {
        bossRef = boss;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어 충돌 시 데미지 적용
        if (collision.collider.CompareTag("Player"))
        {
            Damageable playerDamage = collision.collider.GetComponent<Damageable>();
            if (playerDamage != null)
            {
                playerDamage.TakeHit(damage);
            }

            // Fireball은 부서지지 않음 — 계속 튕김 유지
        }
    }
}
