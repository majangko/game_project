using UnityEngine;

/// <summary>
/// 하르발드의 패시브 스킬: 피의 광란 (Blood Rage)
/// 체력이 낮을수록 공격력과 이동속도가 증가.
/// </summary>
public class Skill_BloodRage : SkillBase
{
    [Header("Blood Rage Settings")]
    [Tooltip("공격력 최대 배율 (HP 0%일 때)")]
    [SerializeField] private float maxAttackMultiplier = 1.5f;

    [Tooltip("이동속도 최대 배율 (HP 0%일 때)")]
    [SerializeField] private float maxSpeedMultiplier = 1.3f;

    private PlayerStats stats;
    private SpumPlatformerController ctrl;

    protected override void Start()
    {
        base.Start();

        stats = GetComponent<PlayerStats>();
        ctrl = GetComponent<SpumPlatformerController>();

        displayName = "피의 광란 (Blood Rage)";
        skillDescription = "체력이 낮을수록 공격력과 이동속도가 증가한다.\n" +
                           "하르발드의 분노는 죽음이 다가올수록 강해진다.";
        cooldown = 0; // 패시브이므로 쿨타임 없음
    }

    protected override void OnActivate() { } // 패시브는 수동 발동 없음

    private void Update()
    {
        if (stats == null || ctrl == null) return;

        float hpRatio = Mathf.Clamp01((float)stats.HP / stats.maxHP);
        float intensity = 1f - hpRatio; // HP가 낮을수록 커짐

        // HP비례 공격/이속 배율 계산
        float atkMul = Mathf.Lerp(1f, maxAttackMultiplier, intensity);
        float spdMul = Mathf.Lerp(1f, maxSpeedMultiplier, intensity);

        // ✅ PlayerStats 이벤트 기반으로 공격력 자동 반영
        stats.SetAttackMultiplier(atkMul);

        // 이동속도는 직접 반영
        ctrl.moveSpeedMul = spdMul;
    }
}
