using UnityEngine;
using System.Collections;

/// <summary>
/// 에드가 리안의 방어 스킬 (Guard Stance)
/// 일정 시간 동안 피해를 감소시키며, 느리게 이동할 수 있는 방어 자세를 유지.
/// 발동 시 짧은 이펙트가 한 번 재생됨.
/// </summary>
public class Skill_GuardStance : SkillBase
{
    [Header("Guard Settings")]
    [Tooltip("방어 지속 시간 (초 단위)")]
    [SerializeField] private float guardDuration = 2.5f;

    [Tooltip("피해 감소 비율 (예: 0.5f = 50%)")]
    [Range(0f, 1f)][SerializeField] private float damageReduction = 0.5f;

    [Tooltip("가드 중 이동 속도 배율 (예: 0.3 = 기본 속도의 30%)")]
    [Range(0f, 1f)][SerializeField] private float guardMoveMultiplier = 0.3f;

    [Header("Effect Settings")]
    [Tooltip("방어 시 표시되는 이펙트 (한 번 재생 후 사라짐)")]
    [SerializeField] private GameObject guardEffectPrefab;

    [Tooltip("이펙트 위치 오프셋 (로컬 기준)")]
    [SerializeField] private Vector3 effectOffset = new Vector3(0f, 1f, 0f); // 인스펙터에서 조정 가능

    private bool isGuarding = false;

    protected void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    protected override void OnActivate()
    {
        if (!isGuarding)
            StartCoroutine(GuardRoutine());
    }

    private IEnumerator GuardRoutine()
    {
        isGuarding = true;
        TriggerAnimation();

        // 이동 속도 감소
        if (ctrl != null)
            ctrl.moveSpeedMul = guardMoveMultiplier;

        // Animator 전환
        if (anim != null)
            anim.SetBool("IsGuarding", true);

        // 짧은 이펙트 재생 (한 번만 표시)
        if (guardEffectPrefab != null)
        {
            Vector3 spawnPos = transform.position + effectOffset;
            GameObject fx = Instantiate(guardEffectPrefab, spawnPos, Quaternion.identity);
            fx.transform.SetParent(transform);
            Destroy(fx, 1.5f); // 1.5초 뒤 자동 제거
        }

        // 피해 감소 적용
        if (TryGetComponent(out Damageable dmg))
            dmg.SetDamageMultiplier(damageReduction);

        // 지속 시간 동안 방어 유지
        float elapsed = 0f;
        while (elapsed < guardDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원상 복귀
        if (ctrl != null)
            ctrl.moveSpeedMul = 1f;

        if (TryGetComponent(out Damageable dmgRestore))
            dmgRestore.SetDamageMultiplier(1f);

        if (anim != null)
            anim.SetBool("IsGuarding", false);

        isGuarding = false;
    }

    // (선택사항) 외부 접근용 Getter
    public GameObject GuardEffectPrefab => guardEffectPrefab;
}
