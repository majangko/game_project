using UnityEngine;
using System.Collections;

public class GhostBuff : SkillBase
{
    [Header("Buff Settings")]
    public float moveSpeedMul = 1.3f;
    public float attackPowerMul = 1.4f;
    public bool useAnimEvent = false;
    public GameObject buffEffectPrefab;
    public GameObject buffCastEffectPrefab;
    public Vector3 buffEffectOffset = new Vector3(0, 0.5f, 0);
    public Vector3 buffCastEffectOffset = new Vector3(0, 0.2f, 0);

    private Coroutine buffCo;
    private GameObject activeBuffFx;

    protected override void OnActivate()
    {
        // ✅ HUD 쿨타임 갱신
        NotifySkillUsed();

        if (!useAnimEvent)
        {
            if (buffCo != null) StopCoroutine(buffCo);
            buffCo = StartCoroutine(CoBuffTimer());
        }

        if (anim && !string.IsNullOrEmpty(animTrigger))
            anim.SetTrigger(animTrigger);
    }

    public void AnimEvent_BuffOn()
    {
        if (!useAnimEvent) return;
        if (buffCo != null) StopCoroutine(buffCo);
        buffCo = StartCoroutine(CoBuffTimer());
    }

    private IEnumerator CoBuffTimer()
    {
        ctrl.moveSpeedMul *= moveSpeedMul;
        ctrl.attackPowerMul *= attackPowerMul;

        if (buffCastEffectPrefab)
        {
            var castFx = Instantiate(buffCastEffectPrefab, transform.position + buffCastEffectOffset, Quaternion.identity);
            Destroy(castFx, 0.7f);
            yield return new WaitForSeconds(0.7f);
        }

        if (buffEffectPrefab)
        {
            activeBuffFx = Instantiate(buffEffectPrefab, transform.position + buffEffectOffset, Quaternion.identity);
            activeBuffFx.transform.SetParent(transform);
        }

        float end = Time.time + duration;
        while (Time.time < end) yield return null;

        ctrl.moveSpeedMul /= moveSpeedMul;
        ctrl.attackPowerMul /= attackPowerMul;

        if (activeBuffFx) Destroy(activeBuffFx);
        buffCo = null;
    }
}
