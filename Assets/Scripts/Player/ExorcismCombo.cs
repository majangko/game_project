using UnityEngine;

public class ExorcismCombo : SkillBase
{
    [Header("Combo Settings")]
    public Transform hitOrigin;
    public LayerMask enemyMask;
    public float damage = 25;
    public float knockback = 10f;

    [Header("Teleport Settings")]
    public float dashDistance = 3f;       // 순간이동 거리
    public float focusDuration = 0.15f;   // 잠깐 집중 모션 유지 시간

    [Header("Hitbox")]
    public Vector2 hitBoxSize = new Vector2(2.5f, 1.2f);
    public Vector2 hitBoxOffset = new Vector2(0.5f, 0.2f);

    [Header("Effects")]
    public GameObject[] slashEffectPrefabs; // 3개 넣을 수 있음
    public float effectLifetime = 0.6f;
    public float effectDelay = 0.15f;       // 각 이펙트 간격

    protected override void OnActivate()
    {
        if (anim) anim.SetTrigger("9_Dash");
        StartCoroutine(DoCombo());
    }

    private System.Collections.IEnumerator DoCombo()
    {
        int dir = ctrl ? ctrl.FacingDir : 1;

        // --- 1. 순간이동 ---
        Vector2 dashStart = transform.position;
        Vector2 dashEnd = dashStart + new Vector2(dashDistance * dir, 0f);

        rb.position = dashEnd; // 순간이동 느낌

        // --- 2. 집중 모션 유지 ---
        yield return new WaitForSeconds(focusDuration);

        // --- 3. 시작점~도착점 경로를 따라 이펙트 3개 배치 ---
        for (int i = 0; i < slashEffectPrefabs.Length; i++)
        {
            GameObject prefab = slashEffectPrefabs[i];

            // 경로를 (i+1)/(총개수+1) 비율로 분할해서 배치
            float t = (i + 1f) / (slashEffectPrefabs.Length + 1f);
            Vector2 spawnPos = Vector2.Lerp(dashStart, dashEnd, t);

            if (prefab)
            {
                // 방향에 따라 좌우 반전
                GameObject fx = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
                if (dir == -1)
                {
                    Vector3 scale = fx.transform.localScale;
                    scale.x *= -1f;
                    fx.transform.localScale = scale;
                }

                Object.Destroy(fx, effectLifetime);

                // 타격 판정
                PerformSlash(spawnPos, dir);
            }

            yield return new WaitForSeconds(effectDelay);
        }
    }

    private void PerformSlash(Vector2 center, int dir)
    {
        Vector2 boxCenter = center + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);

        var hits = Physics2D.OverlapBoxAll(boxCenter, hitBoxSize, 0f, enemyMask);

        foreach (var h in hits)
        {
            var dmg = h.GetComponentInParent<Damageable>();
            if (dmg != null)
            {
                Vector2 knock = new Vector2(dir * knockback, knockback * 0.25f);
                dmg.TakeHit(Mathf.RoundToInt(damage), knock, h.transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        int dir = ctrl ? ctrl.FacingDir : 1;
        Vector2 center = (Vector2)transform.position + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);
        Gizmos.DrawWireCube(center, hitBoxSize);
    }
}
