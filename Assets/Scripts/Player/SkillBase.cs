using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 모든 스킬의 기본 클래스.
/// 공통 속성(쿨타임, 지속시간, 애니메이션, 컨트롤러 등) 관리.
/// SPUM 캐릭터 구조(UnitRoot 안 Animator) 자동 인식 버전.
/// </summary>
public class SkillBase : MonoBehaviour
{
    [Header("Common Settings")]
    [Tooltip("스킬 이름 (디버깅 및 식별용)")]
    public string skillName = "New Skill";

    [Tooltip("쿨타임 (초 단위)")]
    public float cooldown = 1.0f;

    [Tooltip("지속 시간 (정화진 같은 지속형 스킬용)")]
    public float duration = 3.0f;

    [Tooltip("시전 시 발동할 애니메이션 트리거 이름")]
    public string animTrigger;

    [Tooltip("스킬 이펙트 프리팹 (즉시 생성형일 경우 사용)")]
    public GameObject effectPrefab;

    // 🔹 내부 참조
    protected Animator anim;
    protected SpumPlatformerController ctrl;

    // 🔹 상태값
    protected bool isCoolingDown = false;
    private float cooldownTimer = 0f;

    // ✅ 전역 이벤트 (기존용)
    public static event Action<string, float> OnSkillUsed;

    // ✅ 인스턴스 이벤트 (HUDController가 캐릭터별로 구독)
    public event Action<string, float> OnSkillUsedInstance;

    // ============================================================
    // Unity Life Cycle
    // ============================================================
    protected virtual void Start()
    {
        // 1️⃣ SPUM_Prefabs에서 Animator 자동 인식
        var spum = GetComponent<SPUM_Prefabs>();
        if (spum != null && spum.Anim != null)
        {
            anim = spum.Anim;  // UnitRoot의 Animator 연결
        }
        else if (anim == null)
        {
            // 2️⃣ SPUM_Prefabs가 없으면 자식에서 Animator 탐색
            anim = GetComponentInChildren<Animator>();
        }

        // 3️⃣ Controller 연결
        if (ctrl == null)
            ctrl = GetComponent<SpumPlatformerController>();
    }

    protected virtual void Update()
    {
        // 쿨타임 갱신
        if (isCoolingDown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isCoolingDown = false;
                cooldownTimer = 0f;
            }
        }
    }

    // ============================================================
    // 스킬 실행 관련
    // ============================================================
    public virtual void Activate()
    {
        if (isCoolingDown)
        {
            Debug.Log($"{skillName} 쿨타임 남음: {cooldownTimer:F1}s");
            return;
        }

        StartCoroutine(ActivateRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        // 🔸 실제 스킬 발동
        OnActivate();

        // 🔸 쿨타임 시작
        if (cooldown > 0)
        {
            isCoolingDown = true;
            cooldownTimer = cooldown;
            Debug.Log($"[SkillBase] {skillName} → 쿨타임 이벤트 전송 (HUD용)");
            // ✅ HUD에 쿨타임 알림 보내기
            OnSkillUsed?.Invoke(skillName, cooldown);         // 전역
            OnSkillUsedInstance?.Invoke(skillName, cooldown); // 인스턴스
        }

        yield return null;
    }

    /// <summary>
    /// 실제 스킬 로직 구현부 (자식 클래스에서 override)
    /// </summary>
    protected virtual void OnActivate() { }

    // ============================================================
    // 유틸리티
    // ============================================================
    protected int GetFacingDir() => ctrl == null ? 1 : ctrl.FacingDir;

    protected void NotifySkillUsed()
    {
        OnSkillUsed?.Invoke(skillName, cooldown);
        OnSkillUsedInstance?.Invoke(skillName, cooldown);
    }

    protected void TriggerAnimation()
    {
        if (anim == null)
        {
            Debug.LogWarning($"[{skillName}] Animator not found on {name}");
            return;
        }

        if (!string.IsNullOrEmpty(animTrigger))
            anim.SetTrigger(animTrigger);
    }
}
