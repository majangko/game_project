using UnityEngine;
using System.Collections;

public class GhostGate : SkillBase
{
    [Header("Buff Settings")]
    [Tooltip("공격력 배율 (1.3 = 30% 증가)")]
    public float atkBoost = 1.3f;

    [Tooltip("이동 속도 배율 (1.2 = 20% 증가)")]
    public float speedBoost = 1.2f;

    [Tooltip("초당 HP 소모 비율 (0.02 = 초당 최대체력의 2%)")]
    public float hpDrainPercent = 0.02f;

    [Header("Effects")]
    [Tooltip("버프 유지 중 캐릭터 주위 이펙트")]
    public GameObject gateAuraEffect;

    [Tooltip("이펙트 위치 오프셋 (기본은 약간 위)")]
    public Vector3 effectOffset = new Vector3(0, 0.5f, 0);

    private bool isActive = false;
    private GameObject auraInstance;
    private DamageableExtended dmg;

    // SkillBase의 Start()를 덮어씌움
    protected override void Start()
    {
        base.Start();
        dmg = GetComponent<DamageableExtended>();
    }

    protected override void OnActivate()
    {
        // 버프 상태에 따라 On/Off 전환
        if (!isActive)
        {
            // ✅ 애니메이션 실행
            TriggerAnimation();
            StartCoroutine(OpenGate());
        }
        else
        {
            CloseGate();
        }
    }

    private IEnumerator OpenGate()
    {
        isActive = true;

        // 🔸 버프 적용
        if (ctrl != null)
        {
            ctrl.attackPowerMul *= atkBoost;
            ctrl.moveSpeedMul *= speedBoost;
        }

        // 🔸 오라 이펙트 생성 (없을 때만)
        if (gateAuraEffect && auraInstance == null)
        {
            Vector3 spawnPos = transform.position + effectOffset;
            auraInstance = Instantiate(gateAuraEffect, spawnPos, Quaternion.identity, transform);
        }

        // 🔸 HP 지속 소모 루프
        while (isActive)
        {
            if (dmg != null)
            {
                int drainAmount = Mathf.RoundToInt(dmg.MaxHPValue * hpDrainPercent);
                dmg.TakePureDamage(drainAmount);
            }

            // 이펙트 위치 유지
            if (auraInstance != null)
                auraInstance.transform.position = transform.position + effectOffset;

            yield return new WaitForSeconds(1f);
        }
    }

    private void CloseGate()
    {
        isActive = false;

        // 🔸 버프 해제
        if (ctrl != null)
        {
            ctrl.attackPowerMul /= atkBoost;
            ctrl.moveSpeedMul /= speedBoost;
        }

        // 🔸 오라 이펙트 제거
        if (auraInstance)
            Destroy(auraInstance);
    }

    private void OnDisable()
    {
        // 캐릭터가 비활성화되면 자동으로 버프 종료
        if (isActive)
            CloseGate();
    }
}
