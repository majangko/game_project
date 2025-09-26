using UnityEngine;
using System.Collections;

public class GhostSlash : SkillBase
{
    [Header("Slash Settings")]
    public Transform hitOrigin;
    public LayerMask enemyMask;
    public float damage = 30f;
    public Vector2 hitBoxSize = new Vector2(2.8f, 1.2f);
    public Vector2 hitBoxOffset = new Vector2(1.6f, 0.2f);
    public float knockback = 8f;
    public bool useAnimEvent = true;
    public float windupTime = 0.08f;
    public float freezeTime = 0.10f;
    public GameObject hitEffectPrefab;
    public Transform effectSpawnPoint;

    private bool eventTriggered = false;

    protected override void OnActivate()
    {
        eventTriggered = false;

        if (anim && !string.IsNullOrEmpty(animTrigger))
            anim.SetTrigger(animTrigger);

        StartCoroutine(CoSlash());
    }

    private IEnumerator CoSlash()
    {
        if (useAnimEvent)
        {
            float timeout = Mathf.Max(0.02f, windupTime + 0.25f);
            float t = 0f;
            while (t < timeout && !eventTriggered)
            {
                t += Time.deltaTime;
                yield return null;
            }
            if (!eventTriggered) DoHit();
        }
        else
        {
            yield return new WaitForSeconds(windupTime);
            DoHit();
        }

        if (freezeTime > 0f) yield return new WaitForSeconds(freezeTime);
    }

    public void AnimEvent_SlashHit()
    {
        if (eventTriggered) return;
        DoHit();
        eventTriggered = true;
    }

    private void DoHit()
    {
        int dir = ctrl.FacingDir;
        Vector2 center = (Vector2)(hitOrigin ? hitOrigin.position : transform.position)
                         + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);

        if (effectPrefab && effectSpawnPoint)
        {
            var fx = Instantiate(effectPrefab, effectSpawnPoint.position, Quaternion.identity);
            fx.transform.localScale = new Vector3(dir, 1, 1);
            Destroy(fx, 0.5f);
        }

        var hits = Physics2D.OverlapBoxAll(center, hitBoxSize, 0f, enemyMask);
        float finalDamage = damage * Mathf.Max(0.1f, ctrl.attackPowerMul);

        foreach (var h in hits)
        {
            var dmg = h.GetComponentInParent<Damageable>();
            if (dmg != null)
            {
                Vector2 knock = new Vector2(dir * knockback, knockback * 0.25f);
                dmg.TakeHit(Mathf.RoundToInt(finalDamage), knock, h.transform.position);

                if (hitEffectPrefab)
                {
                    var hitFx = Instantiate(hitEffectPrefab, h.transform.position, Quaternion.identity);
                    Destroy(hitFx, 0.3f);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        int dir = (ctrl ? ctrl.FacingDir : 1);
        Vector2 center = (Vector2)(hitOrigin ? hitOrigin.position : transform.position)
                         + new Vector2(hitBoxOffset.x * dir, hitBoxOffset.y);

        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.35f);
        Gizmos.DrawCube(center, hitBoxSize);
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 1f);
        Gizmos.DrawWireCube(center, hitBoxSize);
    }
}
