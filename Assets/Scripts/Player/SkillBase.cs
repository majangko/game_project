using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 모든 스킬의 기본 클래스.
/// 공통 속성(쿨타임, 지속시간, 애니메이션, 사운드, 설명, 컨트롤러 등) 관리.
/// SPUM 캐릭터 구조(UnitRoot 안 Animator) 자동 인식 버전.
/// + 버프 자동 적용 시스템 통합
/// </summary>
public class SkillBase : MonoBehaviour
{
    [Header("Common Settings")]
    [Tooltip("스킬 이름 (디버깅 및 식별용)")]
    public string skillName = "New Skill";

    [Tooltip("UI 등에 표시될 한글 이름")]
    public string displayName = "새로운 스킬";

    [Tooltip("쿨타임 (초 단위)")]
    public float cooldown = 1.0f;

    [Tooltip("지속 시간 (정화진 같은 지속형 스킬용)")]
    public float duration = 3.0f;

    [Tooltip("시전 시 발동할 애니메이션 트리거 이름")]
    public string animTrigger;

    [Tooltip("스킬 이펙트 프리팹 (즉시 생성형일 경우 사용)")]
    public GameObject effectPrefab;

    [Header("Description Settings")]
    [Tooltip("HUD, 툴팁 등에 표시될 스킬 설명")]
    [TextArea(2, 4)]
    public string skillDescription = "이 스킬의 설명을 여기에 작성하세요.";

    [Header("Audio Settings")]
    [Tooltip("스킬 발동 사운드 (없으면 무음)")]
    [SerializeField] private AudioClip skillSound;

    // ============================================================
    // 🔹 버프 자동 적용 설정
    // ============================================================
    public enum BuffType
    {
        None,
        AttackSpeed,
        MoveSpeed,
        Regen,
        DefenseUp
    }

    [Header("Buff Auto Settings")]
    [Tooltip("스킬 사용 시 자동 적용할 버프 타입 (없으면 None)")]
    public BuffType buffType = BuffType.None;

    [Tooltip("버프 지속 시간 (초 단위)")]
    public float buffDuration = 0f;

    [Tooltip("버프 배율 (1.5 = 50% 증가 등)")]
    public float buffMultiplier = 1f;

    // ============================================================
    // 내부 참조
    // ============================================================
    protected Animator anim;
    protected SpumPlatformerController ctrl;

    // 상태값
    protected bool isCoolingDown = false;
    private float cooldownTimer = 0f;

    // ✅ 전역 이벤트 (HUD 등에서 쿨타임 표시용)
    public static event Action<string, float> OnSkillUsed;
    // ✅ 인스턴스 이벤트 (캐릭터별로 독립 구독 가능)
    public event Action<string, float> OnSkillUsedInstance;

    // ============================================================
    // Unity Life Cycle
    // ============================================================
    protected virtual void Start()
    {
        // Animator 자동 인식
        var spum = GetComponent<SPUM_Prefabs>();
        if (spum != null && spum.Anim != null)
            anim = spum.Anim;
        else
            anim = GetComponentInChildren<Animator>();

        // Controller 연결
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
    // 스킬 실행
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
        // 실제 스킬 발동
        OnActivate();

        // 자동 버프 적용 (설정된 경우)
        TryApplyAutoBuff();

        // 사운드 재생
        PlaySkillSound();

        // 쿨타임 시작
        if (cooldown > 0)
        {
            isCoolingDown = true;
            cooldownTimer = cooldown;
            Debug.Log($"[SkillBase] {skillName} → 쿨타임 이벤트 전송 (HUD용)");
            OnSkillUsed?.Invoke(skillName, cooldown);
            OnSkillUsedInstance?.Invoke(skillName, cooldown);
        }

        yield return null;
    }

    /// <summary>
    /// 실제 스킬 로직 구현부 (자식 클래스에서 override)
    /// </summary>
    protected virtual void OnActivate() { }

    // ============================================================
    // 🔹 자동 버프 적용 로직
    // ============================================================
    protected void TryApplyAutoBuff()
    {
        if (buffType == BuffType.None || ctrl == null) return;
        if (buffDuration <= 0f) return;

        switch (buffType)
        {
            case BuffType.AttackSpeed:
                ctrl.ApplyAttackSpeedBuff(buffMultiplier, buffDuration);
                break;
            case BuffType.MoveSpeed:
                ctrl.ApplyMoveSpeedBuff(buffMultiplier, buffDuration);
                break;
            case BuffType.Regen:
                ctrl.ApplyRegenBuff(buffMultiplier, buffDuration);
                break;
            case BuffType.DefenseUp:
                ctrl.ApplyDefenseBuff(buffMultiplier, buffDuration);
                break;
        }

        Debug.Log($"[SkillBase] {skillName} 자동 버프 적용: {buffType} ×{buffMultiplier} for {buffDuration}s");
    }

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

    /// <summary>
    /// 스킬 사운드 재생 (SoundManager를 통해)
    /// </summary>
    protected void PlaySkillSound()
    {
        if (skillSound == null)
            return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(skillSound);
        else
            Debug.LogWarning($"[SkillBase] SoundManager not found — '{skillName}' 사운드 재생 불가");
    }

    // ============================================================
    // HUD/Tooltip용 정보 제공
    // ============================================================
    public string GetSkillDisplayName() => string.IsNullOrEmpty(displayName) ? skillName : displayName;
    public string GetSkillDescription() => skillDescription;
    public float GetCooldown() => cooldown;
}
