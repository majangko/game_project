using UnityEngine;

public class DarkSlash : SkillBase
{
    [Header("Slash Settings")]
    public Transform hitOrigin;
    public LayerMask enemyMask;
    public float baseDamage = 40;
    public float knockback = 6f;
    public float range = 2.5f;

    [Header("Effects")]
    public GameObject slashEffectPrefab;

    private DamageableExtended dmg;

    void Start()
    {
        dmg = GetComponent<DamageableExtended>();
    }

    protected override void OnActivate()
    {
        int dir = ctrl.FacingDir;

        // ✅ 애니메이션 실행
        if (anim != null && !string.IsNullOrEmpty(animTrigger))
            anim.SetTrigger(animTrigger);

        // HP 낮을수록 데미지 강화 (최대 2배)
        float hpFactor = 1f;
        if (dmg != null)
            hpFactor = 1f + (1f - dmg.HPRatio);

        int damage = Mathf.RoundToInt(baseDamage * hpFactor);

        // 판정 위치
        Vector2 pos = hitOrigin ? hitOrigin.position : transform.position;
        pos += new Vector2(dir * range, 0);

        // 적 판정
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
