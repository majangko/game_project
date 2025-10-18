using UnityEngine;
using System.Collections;

/// <summary>
/// 하르발드 (Harvald)의 스킬: 피의 폭풍 (Bloodstorm)
/// 전진하며 도끼를 크게 휘둘러 적에게 피해를 주고,
/// 피를 흡수해 공격력을 일시적으로 강화.
/// </summary>
public class Skill_Bloodstorm : SkillBase
{
    [Header("Bloodstorm Settings")]
    [Tooltip("돌진 거리")]
    [SerializeField] private float dashDistance = 4f;

    [Tooltip("돌진 속도")]
    [SerializeField] private float dashSpeed = 10f;

    [Tooltip("공격력 강화 배율 (예: 1.5 = +50%)")]
    [SerializeField] private float attackBuffMultiplier = 1.5f;

    [Tooltip("공격력 강화 지속 시간 (초)")]
    [SerializeField] private float buffDuration = 3f;

    [Tooltip("피해 반경 (광역 범위)")]
    [SerializeField] private float attackRadius = 1.5f;

    [Tooltip("기본 피해량")]
    [SerializeField] private float baseDamage = 25f;

    [Tooltip("공격 넉백 힘")]
    [SerializeField] private float knockbackPower = 5f;

    [Tooltip("피의 폭풍 이펙트 프리팹")]
    [SerializeField] private GameObject bloodstormEffectPrefab;

    [Tooltip("이펙트 위치 오프셋")]
    [SerializeField] private Vector3 effectOffset = new Vector3(0f, 1f, 0f);

    private bool isActive = false;
    private Coroutine buffCoroutine; // 공격력 버프 코루틴 핸들

    protected override void OnActivate()
    {
        if (!isActive)
            StartCoroutine(BloodstormRoutine());
    }

    private IEnumerator BloodstormRoutine()
    {
        isActive = true;
        TriggerAnimation();

        // 🔹 돌진 중 이동 제한
        if (ctrl != null)
            ctrl.canMove = false;

        // 🔹 돌진 실행
        int dir = GetFacingDir();
        float moved = 0f;
        while (moved < dashDistance)
        {
            float step = dashSpeed * Time.deltaTime;
            transform.position += Vector3.right * step * dir;
            moved += step;
            yield return null;
        }

        // 🔹 돌진 종료 → 이동 복구
        if (ctrl != null)
            ctrl.canMove = true;

        // 🔹 이펙트 생성
        if (bloodstormEffectPrefab)
        {
            var fx = Instantiate(bloodstormEffectPrefab, transform.position + effectOffset, Quaternion.identity);
            fx.transform.SetParent(transform);
            Destroy(fx, 5f);
        }

        // 🔹 공격 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, ctrl ? ctrl.EnemyMask : 0);
        foreach (var hit in hits)
        {
            var dmg = hit.GetComponentInParent<Damageable>();
            if (dmg != null)
            {
                float finalDamage = baseDamage;

                // PlayerStats 공격력 적용
                if (TryGetComponent(out PlayerStats stats))
                    finalDamage = stats.GetAttackPower();

                Vector2 knock = new Vector2(dir * knockbackPower, knockbackPower * 0.25f);
                dmg.TakeHit(Mathf.RoundToInt(finalDamage), knock, hit.transform.position);
            }
        }

        // 🔹 공격력 버프 시작 (별도 코루틴)
        if (TryGetComponent(out PlayerStats playerStats))
        {
            if (buffCoroutine != null)
                StopCoroutine(buffCoroutine);
            buffCoroutine = StartCoroutine(AttackBuffRoutine(playerStats));
        }

        isActive = false;
    }

    /// <summary>
    /// 공격력 증가 버프 처리 (돌진과 독립적으로 동작)
    /// </summary>
    private IEnumerator AttackBuffRoutine(PlayerStats stats)
    {
        stats.SetAttackMultiplier(attackBuffMultiplier);
        yield return new WaitForSeconds(buffDuration);
        stats.SetAttackMultiplier(1f);
    }

    /// <summary>
    /// 스킬이 비활성화될 때 이동 제한이 남지 않도록 처리
    /// </summary>
    private void OnDisable()
    {
        if (ctrl != null)
            ctrl.canMove = true;

        isActive = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
