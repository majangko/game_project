using UnityEngine;
using System.Collections;
using System;

public class ExorcismCombo : SkillBase
{
    [Header("Combo Settings")]
    public Transform hitOrigin;
    public LayerMask enemyMask;
    public float damage = 25f;
    public float knockback = 10f;

    [Header("Teleport Settings")]
    public float dashDistance = 3f;
    public float focusDuration = 0.15f;

    [Header("Hitbox")]
    public Vector2 hitBoxSize = new Vector2(2.5f, 1.2f);
    public Vector2 hitBoxOffset = new Vector2(0.5f, 0.2f);

    [Header("Effects")]
    public GameObject[] slashEffectPrefabs;
    public float effectLifetime = 0.6f;
    public float effectDelay = 0.15f;

    // 쿨다운 HUD 이벤트 (SkillBase에서 지원하지 않는 경우 직접 알림)
    public static event Action<string, float> OnSkillUsed;

    private Rigidbody2D rb;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void OnActivate()
    {
        // 애니메이션 트리거가 등록돼 있다면 실행, 없으면 무시
        if (!string.IsNullOrEmpty(animTrigger))
            TriggerAnimation();

        // HUD 쿨다운 알림 (선택)
        OnSkillUsed?.Invoke(skillName, cooldown);

        // 실제 스킬 로직 실행
        StartCoroutine(DoCombo());
    }

    private IEnumerator DoCombo()
    {
        int dir = ctrl != null ? ctrl.FacingDir : 1;

        // --- 1. 순간이동 ---
        Vector2 dashStart = transform.position;
        Vector2 dashEnd = dashStart + new Vector2(dashDistance * dir, 0f);

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.position = dashEnd;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            transform.position = dashEnd;
        }

        // --- 2. 집중 시간 ---
        yield return new WaitForSeconds(focusDuration);

        // --- 3. 경로를 따라 이펙트 & 타격 ---
        for (int i = 0; i < slashEffectPrefabs.Length; i++)
        {
            GameObject prefab = slashEffectPrefabs[i];
            float t = (i + 1f) / (slashEffectPrefabs.Length + 1f);
            Vector2 spawnPos = Vector2.Lerp(dashStart, dashEnd, t);

            if (prefab)
            {
                GameObject fx = Instantiate(prefab, spawnPos, Quaternion.identity);

                // 방향 반전 적용
                Vector3 scale = fx.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * dir;
                fx.transform.localScale = scale;

                Destroy(fx, effectLifetime);

                // 타격 판정
                PerformSlash(spawnPos, dir);
            }

            yield return new WaitForSeconds(effectDelay);
        }
    }

    private void PerformSlash(Vector2 center, int dir)
    {
        Vector2 boxCenter = center + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, hitBoxSize, 0f, enemyMask);

        foreach (var h in hits)
        {
            Damageable dmg = h.GetComponentInParent<Damageable>();
            if (dmg != null)
            {
                Vector2 knock = new Vector2(dir * knockback, knockback * 0.25f);
                dmg.TakeHit(Mathf.RoundToInt(damage), knock, h.transform.position);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        int dir = Application.isPlaying && ctrl ? ctrl.FacingDir : 1;
        Vector2 center = (Vector2)transform.position + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);
        Gizmos.DrawWireCube(center, hitBoxSize);
    }
#endif
}
