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
    [SerializeField] private float dashDistance = 4f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float attackBuffMultiplier = 1.5f;
    [SerializeField] private float attackBuffDuration = 3f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private float baseDamage = 25f;
    [SerializeField] private float knockbackPower = 5f;
    [SerializeField] private GameObject bloodstormEffectPrefab;
    [SerializeField] private Vector3 effectOffset = new Vector3(0f, 1f, 0f);

    private bool isActive = false;
    private Coroutine buffCoroutine;

    protected override void OnActivate()
    {
        if (!isActive)
            StartCoroutine(BloodstormRoutine());
    }

    private IEnumerator BloodstormRoutine()
    {
        isActive = true;
        TriggerAnimation();

        if (ctrl != null)
            ctrl.canMove = false;

        int dir = GetFacingDir();
        float moved = 0f;
        while (moved < dashDistance)
        {
            float step = dashSpeed * Time.deltaTime;
            transform.position += Vector3.right * step * dir;
            moved += step;
            yield return null;
        }

        if (ctrl != null)
            ctrl.canMove = true;

        // ✅ 버프를 공격 전에 먼저 적용
        if (TryGetComponent(out PlayerStats playerStats))
        {
            playerStats.SetAttackMultiplier(attackBuffMultiplier);

            if (buffCoroutine != null)
                StopCoroutine(buffCoroutine);
            buffCoroutine = StartCoroutine(AttackBuffRoutine(playerStats));
        }

        // ✅ 이펙트 생성
        if (bloodstormEffectPrefab)
        {
            var fx = Instantiate(bloodstormEffectPrefab, transform.position + effectOffset, Quaternion.identity);
            fx.transform.SetParent(transform);
            Destroy(fx, 5f);
        }

        // ✅ 공격 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, ctrl ? ctrl.EnemyMask : 0);
        foreach (var hit in hits)
        {
            var dmg = hit.GetComponentInParent<Damageable>();
            if (dmg != null)
            {
                float finalDamage = baseDamage;

                if (playerStats != null)
                    finalDamage = playerStats.GetAttackPower(); // 버프 적용된 상태에서 계산

                Vector2 knock = new Vector2(dir * knockbackPower, knockbackPower * 0.25f);
                dmg.TakeHit(Mathf.RoundToInt(finalDamage), knock, hit.transform.position);
            }
        }

        isActive = false;
    }

    private IEnumerator AttackBuffRoutine(PlayerStats stats)
    {
        // ✅ 임시 버프는 tempAttackBuffMultiplier 사용
        stats.tempAttackBuffMultiplier = attackBuffMultiplier;
        yield return new WaitForSeconds(attackBuffDuration);
        stats.tempAttackBuffMultiplier = 1f;
    }


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
