using UnityEngine;
using System.Collections;

public class GhostGate : SkillBase
{
    [Header("Buff Settings")]
    public float atkBoost = 1.3f;
    public float speedBoost = 1.2f;
    public float hpDrainPercent = 0.02f; // 초당 2%

    [Header("Effects")]
    public GameObject gateAuraEffect;   // 버프 유지 이펙트
    public Vector3 effectOffset = new Vector3(0, 0.5f, 0); // Inspector에서 조절 가능

    private bool isActive;
    private GameObject auraInstance;
    private DamageableExtended dmg;

    void Start()
    {
        dmg = GetComponent<DamageableExtended>();
    }

    protected override void OnActivate()
    {
        if (!isActive)
        {
            // ✅ 애니메이션 실행
            if (anim != null && !string.IsNullOrEmpty(animTrigger))
                anim.SetTrigger(animTrigger);

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

        // 버프 적용
        ctrl.attackPowerMul *= atkBoost;
        ctrl.moveSpeedMul *= speedBoost;

        // 이펙트 생성 (위치 오프셋 반영)
        if (gateAuraEffect && auraInstance == null)
        {
            Vector3 spawnPos = transform.position + effectOffset;
            auraInstance = Instantiate(gateAuraEffect, spawnPos, Quaternion.identity, transform);
        }

        // HP 소모 루프
        while (isActive)
        {
            if (dmg != null)
                dmg.TakePureDamage(Mathf.RoundToInt(dmg.MaxHPValue * hpDrainPercent));

            yield return new WaitForSeconds(1f);
        }
    }

    private void CloseGate()
    {
        isActive = false;

        // 버프 해제
        ctrl.attackPowerMul /= atkBoost;
        ctrl.moveSpeedMul /= speedBoost;

        if (auraInstance)
            Destroy(auraInstance);
    }
}
