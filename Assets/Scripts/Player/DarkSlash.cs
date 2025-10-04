using UnityEngine;

public class DarkSlash : SkillBase
{
    [Header("Slash Settings")]
    public Transform hitOrigin;           // 공격 판정 기준점
    public LayerMask enemyMask;           // 적 판정 레이어
    public float baseDamage = 40f;        // 기본 피해량
    public float knockback = 6f;          // 넉백 힘
    public float range = 2.5f;            // 공격 범위

    [Header("Effects")]
    public GameObject slashEffectPrefab;  // 베기 이펙트 프리팹

    private DamageableExtended dmg;       // 체력 비례 강화용

    // 부모 SkillBase의 Start()와 충돌 방지 → override + base.Start() 호출
    protected override void Start()
    {
        base.Start();
        dmg = GetComponent<DamageableExtended>();
    }

    protected override void OnActivate()
    {
        int dir = ctrl != null ? ctrl.FacingDir : 1;

        // ✅ 애니메이션 실행
        TriggerAnimation();

        // HP 낮을수록 데미지 강화 (최대 2배)
        float hpFactor = 1f;
        if (dmg != null)
            hpFactor = 1f + (1f - dmg.HPRatio);

        int damage = Mathf.RoundToInt(baseDamage * hpFactor);

        // 판정 위치 계산
        Vector2 pos = hitOrigin ? hitOrigin.position : transform.position;
        pos += new Vector2(dir * range, 0);

        // 적 탐색 및 타격
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, range, enemyMask);
        foreach (var h in hits)
        {
            var target = h.GetComponent<Damageable>();
            if (target != null)
            {
                Vector2 knock = new Vector2(dir * knockback, 2f);
                target.TakeHit(damage, knock, pos);
            }
        }

        // 🔥 이펙트 생성 (방향 반대로 출력)
        if (slashEffectPrefab)
        {
            Quaternion rot = dir == 1 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            GameObject fx = Instantiate(slashEffectPrefab, pos, rot);
            Destroy(fx, 0.5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;

        int dir = Application.isPlaying && ctrl ? ctrl.FacingDir : 1;
        Vector2 pos = (Vector2)hitOrigin.position + new Vector2(dir * range, 0);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(pos, range);
    }
}
