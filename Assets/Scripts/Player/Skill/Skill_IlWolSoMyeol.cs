using UnityEngine;

public class Skill_IlWolSoMyeol : SkillBase
{
    [Header("일월소멸 설정")]
    public int enhancedCount = 3;
    public int explosionDamage = 20;
    public float explosionRadius = 2f;
    public GameObject explosionPrefab;
    public LayerMask enemyMask;

    [Header("이펙트 설정")]
    [Tooltip("스킬 발동 시 캐릭터 주변에 표시할 집중 이펙트")]
    public Vector3 effectOffset = new Vector3(0, 0.8f, 0f); // 캐릭터 머리 위 쯤

    protected override void OnActivate()
    {
        // 🔸 1. 애니메이션 실행
        if (!string.IsNullOrEmpty(animTrigger))
            anim.SetTrigger(animTrigger);

        // 🔸 2. 집중 이펙트 생성
        if (effectPrefab)
        {
            Vector3 spawnPos = transform.position + effectOffset;
            GameObject fx = Instantiate(effectPrefab, spawnPos, Quaternion.identity, transform);
            Destroy(fx, duration); // 스킬 지속시간 후 자동 제거
        }

        // 🔸 3. 강화 평타 설정 (원래 기능 유지)
        if (ctrl != null)
        {
            ctrl.SetEnhancedAttack(enhancedCount, explosionPrefab, explosionDamage, explosionRadius, enemyMask);
        }
    }
}
