using UnityEngine;
using System.Collections;

public class TaegukSlash : SkillBase
{
    [Header("Charge Settings")]
    public float minChargeTime = 0.3f;        // 최소 차징 시간
    public float maxChargeTime = 2f;          // 최대 차징 시간
    public string chargeAnim = "8_Charge";    // 차징 애니메이션 Bool
    public string slashAnim = "7_Skill";      // 발동 애니메이션 Trigger

    [Header("Slash Settings")]
    public Transform hitOrigin;
    public LayerMask enemyMask;
    public float baseDamage = 30f;
    public float damagePerCharge = 10f;
    public Vector2 hitBoxSize = new Vector2(2.8f, 1.2f);
    public Vector2 hitBoxOffset = new Vector2(1.6f, 0.2f);
    public float knockback = 8f;

    [Header("Effects")]
    public GameObject slashEffectPrefab;
    public GameObject hitEffectPrefab;

    private bool isCharging = false;
    private float chargeTimer = 0f;

    protected override void OnActivate()
    {
        if (isCharging || isCoolingDown)
            return;

        StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        isCharging = true;
        chargeTimer = 0f;

        // 🔹 차징 애니메이션 시작
        if (anim) anim.SetBool(chargeAnim, true);

        // 🔹 키를 누르고 있는 동안 차징 (X 키 기준)
        while (Input.GetKey(KeyCode.X))
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0, maxChargeTime);
            yield return null;
        }

        // 🔹 키에서 손 뗀 순간 — 발동
        if (anim)
        {
            anim.SetBool(chargeAnim, false);
            anim.SetTrigger(slashAnim);
        }

        // 타격 처리
        PerformSlash();

        // 차징 종료
        isCharging = false;

        // ✅ SkillBase에서 자동 쿨타임 처리 + HUD 알림
        if (cooldown > 0)
        {
            isCoolingDown = true;
            StartCoroutine(CooldownRoutine());
            NotifySkillUsed();
        }
    }

    private IEnumerator CooldownRoutine()
    {
        float timer = cooldown;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        isCoolingDown = false;
    }

    private void PerformSlash()
    {
        // 🔹 차징 시간에 따른 데미지 계산
        float dmg = baseDamage + (chargeTimer * damagePerCharge);
        int dir = GetFacingDir();

        // 🔹 공격 범위 계산
        Vector2 center = (Vector2)(hitOrigin ? hitOrigin.position : transform.position)
                         + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitBoxSize, 0, enemyMask);

        foreach (var hit in hits)
        {
            var dmgComp = hit.GetComponentInParent<Damageable>();
            if (dmgComp != null)
            {
                Vector2 knockDir = new Vector2(dir * knockback, knockback * 0.25f);
                Vector2 hitPoint = hit.ClosestPoint(center);
                dmgComp.TakeHit(Mathf.RoundToInt(dmg), knockDir, hitPoint);
            }

            // 🔹 히트 이펙트
            if (hitEffectPrefab)
            {
                GameObject hitFx = Instantiate(hitEffectPrefab, hit.transform.position, Quaternion.identity);
                Destroy(hitFx, 0.3f);
            }
        }

        // 🔹 슬래시 이펙트 (좌우 반전 포함)
        if (slashEffectPrefab && hitOrigin)
        {
            Vector3 spawnPos = hitOrigin.position + new Vector3(hitBoxOffset.x * dir, hitBoxOffset.y);
            Quaternion rot = dir == -1 ? Quaternion.Euler(0, 180f, 0) : Quaternion.identity;

            GameObject fx = Instantiate(slashEffectPrefab, spawnPos, rot);
            Destroy(fx, 0.5f);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;
        int dir = Application.isPlaying ? GetFacingDir() : 1;
        Vector2 center = (Vector2)hitOrigin.position + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);

        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);
        Gizmos.DrawCube(center, hitBoxSize);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, hitBoxSize);
    }
#endif
}
