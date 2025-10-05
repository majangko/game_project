using UnityEngine;

public class Skill_IlWolSoMyeol : SkillBase
{
    [Header("일월소멸 설정")]
    public int enhancedCount = 3;          // 강화 평타 횟수
    public int explosionDamage = 20;       // 폭발 데미지
    public float explosionRadius = 2f;     // 폭발 범위
    public GameObject explosionPrefab;     // 폭발 이펙트
    public LayerMask enemyMask;            // 적 판정용

    [Header("이펙트 설정")]
    [Tooltip("스킬 발동 시 캐릭터 주변에 표시할 집중 이펙트")]
    public Vector3 effectOffset = new Vector3(0, 0.8f, 0f); // 캐릭터 머리 위 위치

    protected override void OnActivate()
    {
        // 1️⃣ 시전 애니메이션 실행
        TriggerAnimation();

        // 2️⃣ 집중 이펙트 생성 (시전 효과)
        if (effectPrefab)
        {
            Vector3 spawnPos = transform.position + effectOffset;
            GameObject fx = Instantiate(effectPrefab, spawnPos, Quaternion.identity, transform);
            Destroy(fx, duration); // 지속시간 후 제거
        }

        // 3️⃣ 평타 강화 적용
        if (ctrl != null)
        {
            ctrl.SetEnhancedAttack(enhancedCount, explosionPrefab, explosionDamage, explosionRadius, enemyMask);
        }

        // ✅ 4️⃣ HUD 쿨타임 알림 (SkillBase의 이벤트 호출)
        NotifySkillUsed();
    }
}
