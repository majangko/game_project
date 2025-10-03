using UnityEngine;

public class TaegukSlash : SkillBase
{
    [Header("Charge Settings")]
    public float minChargeTime = 0.3f;
    public float maxChargeTime = 2f;
    public string chargeAnim = "8_Charge";   // 차징 모션 Bool
    public string slashAnim = "7_Skill";     // 발동 모션 Trigger

    [Header("Slash Settings")]
    public Transform hitOrigin;
    public LayerMask enemyMask;
    public float baseDamage = 30;
    public float damagePerCharge = 10f;
    public Vector2 hitBoxSize = new Vector2(2.8f, 1.2f);
    public Vector2 hitBoxOffset = new Vector2(1.6f, 0.2f);
    public float knockback = 8;

    [Header("Effects")]
    public GameObject slashEffectPrefab;
    public GameObject hitEffectPrefab;

    private float chargeTimer;
    private bool isCharging;

    protected override void OnActivate()
    {
        if (!isCharging)  // 스킬 키 처음 눌렀을 때
            StartCharge();
    }

    void Update()
    {
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0, maxChargeTime);

            // 키에서 손 뗀 순간 → 발동
            if (Input.GetKeyUp(KeyCode.X))
                ReleaseSlash();
        }
    }

    void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0f;

        if (anim)
            anim.SetBool(chargeAnim, true); // 차지 애니메이션 시작
    }

    void ReleaseSlash()
    {
        isCharging = false;

        if (anim)
        {
            anim.SetBool(chargeAnim, false); // 차지 모션 종료
            anim.SetTrigger(slashAnim);      // 베기 모션 발동
        }

        PerformSlash(); // 타격 판정 + 이펙트
    }

    public void PerformSlash()
    {
        float dmg = baseDamage + (chargeTimer * damagePerCharge);
        int dir = ctrl != null ? ctrl.FacingDir : 1;

        // 히트박스 위치
        Vector2 center = (Vector2)(hitOrigin ? hitOrigin.position : transform.position)
                         + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitBoxSize, 0, enemyMask);

        foreach (var hit in hits)
        {
            var dmgComp = hit.GetComponentInParent<Damageable>();
            if (dmgComp != null)
            {
                Vector2 knockDir = new Vector2(dir * knockback, knockback * 0.25f);
                Vector2 hitPoint = hit.ClosestPoint(hitOrigin ? hitOrigin.position : transform.position);
                dmgComp.TakeHit(Mathf.RoundToInt(dmg), knockDir, hitPoint);
            }

            if (hitEffectPrefab)
            {
                GameObject hitFx = Instantiate(hitEffectPrefab, hit.transform.position, Quaternion.identity);
                Destroy(hitFx, 0.3f);
            }
        }

        // 🔥 Slash 이펙트 (히트박스 위치 기준, 좌우 반전 포함)
        if (slashEffectPrefab && hitOrigin)
        {
            Vector3 boxCenter = (Vector2)hitOrigin.position + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);

            Quaternion spawnRot = Quaternion.identity;
            if (dir == -1)
                spawnRot *= Quaternion.Euler(0, 180f, 0);

            GameObject fx = Instantiate(slashEffectPrefab, boxCenter, spawnRot);
            Destroy(fx, 0.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;
        int dir = Application.isPlaying && ctrl ? ctrl.FacingDir : 1;
        Vector2 center = (Vector2)hitOrigin.position + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawCube(center, hitBoxSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, hitBoxSize);
    }
}
