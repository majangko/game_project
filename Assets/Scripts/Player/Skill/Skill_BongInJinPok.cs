using UnityEngine;
using System.Collections;

public class Skill_BongInJinPok : SkillBase
{
    [Header("봉인진폭 설정")]
    public GameObject sealCirclePrefab;  // 결계 이펙트
    public GameObject explosionPrefab;   // 폭발 이펙트
    public float radius = 3.5f;          // 결계 범위
    public float delayTime = 2f;         // 결계 유지 시간
    public int damage = 80;              // 폭발 피해량
    public LayerMask enemyMask;          // 적 판정용
    public Vector2 offset = new Vector2(0f, -0.5f); // 결계 중심 오프셋

    protected override void OnActivate()
    {
        // 🔹 애니메이션 실행
        if (anim && !string.IsNullOrEmpty(animTrigger))
            anim.SetTrigger(animTrigger);

        // 🔹 캐릭터 위치 기준으로 결계 생성
        Vector2 spawnPos = (Vector2)transform.position + offset;

        StartCoroutine(PerformSeal(spawnPos));
    }

    private IEnumerator PerformSeal(Vector2 center)
    {
        // 1️⃣ 결계 생성
        GameObject seal = null;
        if (sealCirclePrefab)
            seal = Instantiate(sealCirclePrefab, center, Quaternion.identity);

        // 2️⃣ 2초간 유지 (에너지 충전)
        yield return new WaitForSeconds(delayTime);

        // 3️⃣ 폭발 이펙트
        if (explosionPrefab)
        {
            var explosion = Instantiate(explosionPrefab, center, Quaternion.identity);
            Destroy(explosion, 1.5f);
        }

        // 4️⃣ 폭발 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyMask);
        foreach (var h in hits)
        {
            var dmg = h.GetComponent<Damageable>();
            if (dmg != null)
                dmg.TakeHit(damage, Vector2.zero, center);
        }

        // 5️⃣ 결계 제거
        if (seal)
            Destroy(seal);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.3f, 0.5f);
        Vector2 center = (Vector2)transform.position + offset;
        Gizmos.DrawWireSphere(center, radius);
    }
}
