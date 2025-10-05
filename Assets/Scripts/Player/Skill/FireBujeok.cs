using UnityEngine;
using System.Collections;

public class Skill_JungHwaJin : SkillBase
{
    [Header("정화진 투척 설정")]
    public GameObject projectilePrefab;     // 던질 부적 (Sprite 오브젝트)
    public float projectileSpeed = 8f;      // 투척 속도
    public GameObject areaEffectPrefab;     // 결계 이펙트 (Sprite나 Particle)
    public int damagePerTick = 10;          // 지속 피해
    public float tickInterval = 1f;         // 초당 데미지 주기
    public float radius = 2f;               // 결계 범위
    public LayerMask enemyMask;             // 적 판정용

    [Header("투척 위치 설정")]
    [Tooltip("투척 기준이 될 캐릭터의 특정 파츠 (예: P_Body, PivotFront 등)")]
    public Transform throwOrigin;           // 기준 Transform (없으면 본체 transform 사용)
    [Tooltip("기준점으로부터의 위치 보정 (x는 전방 방향, y는 높이)")]
    public Vector2 throwOffset = new Vector2(0.5f, 0.8f); // 인스펙터에서 조정 가능

    protected override void OnActivate()
    {
        // 🔹 애니메이션 실행
        TriggerAnimation();

        int dir = GetFacingDir();

        // ✅ 기준점 계산
        Vector2 basePos = throwOrigin != null ? (Vector2)throwOrigin.position : (Vector2)transform.position;
        Vector2 spawnPos = basePos + new Vector2(throwOffset.x * dir, throwOffset.y);

        // 🔹 Projectile 생성
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Rigidbody 설정
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = proj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = new Vector2(projectileSpeed * dir, 0f);

        // Collider 설정
        CircleCollider2D col = proj.GetComponent<CircleCollider2D>();
        if (col == null) col = proj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        // 방향에 따라 Sprite 반전
        SpriteRenderer sr = proj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = (dir == -1);

        // 🔹 충돌 이벤트 연결
        proj.AddComponent<MonoHelper>().Init((hitPos) =>
        {
            // 결계 생성
            GameObject area = Instantiate(areaEffectPrefab, hitPos, Quaternion.identity);
            StartCoroutine(DoAreaEffect(area.transform));
            Destroy(area, duration);

            // ✅ HUD 쿨타임 갱신 (실제 스킬 효과 발동 시점)
            NotifySkillUsed();
        });

        Destroy(proj, 3f); // 투사체 3초 후 자동 제거
    }

    private IEnumerator DoAreaEffect(Transform center)
    {
        float timer = 0f;
        while (timer < duration)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(center.position, radius, enemyMask);
            foreach (var c in cols)
            {
                Damageable d = c.GetComponent<Damageable>();
                if (d != null)
                    d.TakeHit(damagePerTick, Vector2.zero, center.position);
            }

            yield return new WaitForSeconds(tickInterval);
            timer += tickInterval;
        }
    }
}

/// 🔸 충돌 감지용 헬퍼 (Projectile용)
public class MonoHelper : MonoBehaviour
{
    private System.Action<Vector2> onHit;

    public void Init(System.Action<Vector2> onHitCallback)
    {
        onHit = onHitCallback;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 적 또는 지면에 닿았을 때 폭발 이펙트 실행
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
            other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            onHit?.Invoke(transform.position);
            Destroy(gameObject);
        }
    }
}
