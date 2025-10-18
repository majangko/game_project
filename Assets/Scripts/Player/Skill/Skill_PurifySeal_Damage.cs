using UnityEngine;

public class Skill_PurifySeal_Damage : SkillBase
{
    [Header("Prefabs")]
    public GameObject projectilePrefab;
    public GameObject sealPrefab;

    [Header("Settings")]
    public float projectileSpeed = 8f;
    public float sealDuration = 3f;
    public Transform throwOrigin;
    public Vector2 throwOffset = new Vector2(0.5f, 0.8f);

    protected override void OnActivate()
    {
        if (projectilePrefab == null || sealPrefab == null)
        {
            Debug.LogError($"[{skillName}] Prefabs not assigned!");
            return;
        }

        // ▶ 현재 캐릭터의 바라보는 방향 (왼쪽:-1 / 오른쪽:1)
        int dir = GetFacingDir();

        // ▶ 부적 생성 위치 계산
        Vector2 basePos = throwOrigin != null ? (Vector2)throwOrigin.position : (Vector2)transform.position;
        Vector2 spawnPos = basePos + new Vector2(throwOffset.x * dir, throwOffset.y);

        // ▶ 발사체 생성
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // ▶ Rigidbody 설정
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = proj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = new Vector2(projectileSpeed * dir, 0f);

        // ▶ Collider 설정
        Collider2D col = proj.GetComponent<Collider2D>();
        if (col == null) col = proj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        // ▶ 스프라이트 반전 처리 (왼쪽: 원본 / 오른쪽: flip)
        SpriteRenderer sr = proj.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = proj.GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            // dir == 1이면 오른쪽 → 반전된 부적
            // dir == -1이면 왼쪽 → 원본 부적
            sr.flipX = (dir == 1);
        }

        // ▶ SealProjectile 초기화
        var projectile = proj.GetComponent<SealProjectile>();
        if (projectile != null)
            projectile.Init(projectileSpeed * dir, sealPrefab, sealDuration, gameObject);

        // ▶ 애니메이션 트리거 및 스킬 사용 처리
        TriggerAnimation();
        NotifySkillUsed();

        // ▶ 부적 자동 파괴 (3초 후)
        Destroy(proj, 3f);
    }
}
