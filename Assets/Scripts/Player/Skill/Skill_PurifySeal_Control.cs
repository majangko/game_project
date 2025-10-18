using UnityEngine;

public class Skill_PurifySeal_Control : SkillBase
{
    [Header("Prefabs")]
    public GameObject projectilePrefab;
    public GameObject sealPrefab;

    [Header("Settings")]
    public float projectileSpeed = 8f;
    public float sealDuration = 2.5f;
    public Transform throwOrigin;
    public Vector2 throwOffset = new Vector2(0.5f, 0.8f);

    protected override void OnActivate()
    {
        if (projectilePrefab == null || sealPrefab == null)
        {
            Debug.LogError($"[{skillName}] Prefabs not assigned!");
            return;
        }

        // ✅ 방향 계산 (왼쪽:-1 / 오른쪽:1)
        int dir = GetFacingDir();

        // ✅ 발사 위치 계산
        Vector2 basePos = throwOrigin != null ? (Vector2)throwOrigin.position : (Vector2)transform.position;
        Vector2 spawnPos = basePos + new Vector2(throwOffset.x * dir, throwOffset.y);

        // ✅ 발사체 생성
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // ✅ Rigidbody 설정
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = proj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = new Vector2(projectileSpeed * dir, 0f);

        // ✅ Collider 설정
        Collider2D col = proj.GetComponent<Collider2D>();
        if (col == null) col = proj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        // ✅ 스프라이트 반전 (왼쪽: 원본 / 오른쪽: flip)
        SpriteRenderer sr = proj.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = proj.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.flipX = (dir == 1);

        // ✅ SealProjectile 초기화
        var projectile = proj.GetComponent<SealProjectile>();
        if (projectile != null)
            projectile.Init(projectileSpeed * dir, sealPrefab, sealDuration, gameObject);

        // ✅ 애니메이션 + 쿨타임 처리
        TriggerAnimation();
        NotifySkillUsed();

        // ✅ 일정 시간 후 제거
        Destroy(proj, 3f);
    }
}
