using UnityEngine;
using System.Collections;

/// <summary>
/// 구마 B 루트: 귀문(鬼門) 스킬
/// 일정 시간 동안 공격력/이속 증가, 체력 지속 소모.
/// HP 30 이하일 땐 더 이상 소모되지 않음.
/// </summary>
public class GhostGate : SkillBase
{
    [Header("Buff Settings")]
    [Tooltip("공격력 배율 (1.3 = 30% 증가)")]
    public float atkBoost = 1.3f;

    [Tooltip("이동 속도 배율 (1.2 = 20% 증가)")]
    public float speedBoost = 1.2f;

    [Tooltip("초당 HP 소모 비율 (0.02 = 초당 최대체력의 2%)")]
    public float hpDrainPercent = 0.02f;

    [Tooltip("HP가 이 수치 이하로 내려가면 더 이상 HP 소모 중단")]
    public int minHPThreshold = 30;

    [Header("Effects")]
    public GameObject gateAuraEffect;
    public Vector3 effectOffset = new Vector3(0, 0.5f, 0);

    private bool isActive = false;
    private GameObject auraInstance;
    private DamageableExtended dmg;
    private PlayerStats stats;
    private SpumPlatformerController ctrl;

    private void Awake()
    {
        dmg = GetComponent<DamageableExtended>();
        if (dmg == null)
            dmg = GetComponentInParent<DamageableExtended>();

        stats = GetComponent<PlayerStats>();
        ctrl = GetComponent<SpumPlatformerController>();

        if (dmg == null)
            Debug.LogWarning("[GhostGate] DamageableExtended not found!");
    }

    protected override void OnActivate()
    {
        if (!isActive)
        {
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

        // ✅ PlayerStats 이벤트 기반 버프
        if (stats != null)
            stats.SetAttackMultiplier(stats.attackMultiplier * atkBoost);

        if (ctrl != null)
            ctrl.moveSpeedMul *= speedBoost;

        // 이펙트 생성
        if (gateAuraEffect && auraInstance == null)
        {
            Vector3 spawnPos = transform.position + effectOffset;
            auraInstance = Instantiate(gateAuraEffect, spawnPos, Quaternion.identity, transform);
        }

        // HP 지속 소모 루프
        while (isActive)
        {
            if (dmg != null)
            {
                int curHP = dmg.CurrentHPValue;

                if (curHP <= minHPThreshold)
                {
                    Debug.Log($"[GhostGate] HP at {curHP}, drain stopped (threshold: {minHPThreshold})");
                }
                else
                {
                    int drainAmount = Mathf.RoundToInt(dmg.MaxHPValue * hpDrainPercent);
                    dmg.TakePureDamage(drainAmount);
                }
            }
            else
            {
                Debug.LogWarning("[GhostGate] dmg reference lost!");
                yield break;
            }

            if (auraInstance != null)
                auraInstance.transform.position = transform.position + effectOffset;

            yield return new WaitForSeconds(1f);
        }
    }

    private void CloseGate()
    {
        isActive = false;

        // ✅ 버프 해제
        if (stats != null)
            stats.SetAttackMultiplier(stats.attackMultiplier / atkBoost);

        if (ctrl != null)
            ctrl.moveSpeedMul /= speedBoost;

        if (auraInstance)
            Destroy(auraInstance);
    }

    private void OnDisable()
    {
        if (isActive)
            CloseGate();
    }
}
