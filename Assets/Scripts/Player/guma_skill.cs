using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpumPlatformerController))]
public class guma_skill : MonoBehaviour
{
    SpumPlatformerController ctrl;
    Rigidbody2D rb;
    Animator anim;

    [Header("Common")]
    public Transform hitOrigin;
    public LayerMask enemyMask;

    [Header("Animator Param Names (자동 탐색 지원)")]
    public string slashParam = "";
    public string buffParam = "";

    [Header("Crescent Slash (배기형) - Key: X")]
    public KeyCode slashKey = KeyCode.X;
    public float slashDamage = 30f;
    public Vector2 slashBoxSize = new Vector2(2.8f, 1.2f);
    public Vector2 slashBoxOffset = new Vector2(1.6f, 0.2f); // 항상 오른쪽 기준으로만 입력
    public float slashKnockback = 8f;
    public float slashCooldown = 1.2f;

    public bool useAnimEventForSlash = true;
    public float slashWindupFallback = 0.08f;
    public float slashFreeze = 0.10f;

    float lastSlashTime = -999f;
    bool slashEventFired = false;

    [Header("Battle Cry (자기 버프) - Key: C")]
    public KeyCode buffKey = KeyCode.C;
    public float moveSpeedMul = 1.3f;
    public float attackPowerMul = 1.4f;
    public float buffDuration = 6f;
    public float buffCooldown = 12f;

    public bool useAnimEventForBuff = false;

    float lastBuffTime = -999f;
    Coroutine buffCo;

    [Header("Effects (투명 PNG 버전 사용 권장)")]
    public GameObject slashEffectPrefab;
    public Transform effectSpawnPoint;
    public GameObject hitEffectPrefab;
    public GameObject buffEffectPrefab;       // 버프 오라 FX
    public GameObject buffCastEffectPrefab;   // 버프 캐스트 FX

    [Header("Effect Offsets")]
    public Vector3 buffEffectOffset = new Vector3(0, 0.5f, 0);      // 버프 오라 위치
    public Vector3 buffCastEffectOffset = new Vector3(0, 0.2f, 0);  // 캐스트 FX 위치

    GameObject activeBuffFx;

    void Awake()
    {
        ctrl = GetComponent<SpumPlatformerController>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        if (!hitOrigin) hitOrigin = transform;

        // --- 파라미터 자동 탐색 ---
        if (anim)
        {
            if (string.IsNullOrEmpty(slashParam))
            {
                string[] cand = { "1_Skill_Normal", "1_Skill_Magic", "1_Skill_Bow",
                                  "0_Attack_Normal", "1_Attack", "Skill_Slash", "Slash" };
                slashParam = FindFirstExistingParam(anim, cand);
            }
            if (string.IsNullOrEmpty(buffParam))
            {
                string[] cand = { "0_Buff", "0_Concentrate", "Buff", "Skill_Buff" };
                buffParam = FindFirstExistingParam(anim, cand);
            }
        }
    }

    void Update()
    {
        HandleSlash();
        HandleBuff();
    }

    // ===== Slash =====
    void HandleSlash()
    {
        if (!Input.GetKeyDown(slashKey)) return;
        if (Time.time - lastSlashTime < slashCooldown) return;

        lastSlashTime = Time.time;
        slashEventFired = false;

        FlexibleSetParam(anim, slashParam);

        StartCoroutine(CoSlash());
    }

    IEnumerator CoSlash()
    {
        if (useAnimEventForSlash)
        {
            float timeout = Mathf.Max(0.02f, slashWindupFallback + 0.25f);
            float t = 0f;
            while (t < timeout && !slashEventFired) { t += Time.deltaTime; yield return null; }
            if (!slashEventFired) DoSlashHit();
        }
        else
        {
            yield return new WaitForSeconds(Mathf.Max(0f, slashWindupFallback));
            DoSlashHit();
        }

        if (slashFreeze > 0f) yield return new WaitForSeconds(slashFreeze);
    }

    public void AnimEvent_SlashHit()
    {
        if (slashEventFired) return;
        DoSlashHit();
        slashEventFired = true;
    }

    void DoSlashHit()
    {
        int dir = ctrl.FacingDir;

        // --- 수정된 부분: 항상 baseOffset 기준으로 dir만 곱해줌 ---
        Vector2 baseOffset = slashBoxOffset; // 오른쪽 기준 값
        Vector2 center = (Vector2)(hitOrigin ? hitOrigin.position : transform.position)
                         + new Vector2(baseOffset.x * dir, baseOffset.y);

        // Slash FX
        if (slashEffectPrefab && effectSpawnPoint)
        {
            var fx = Instantiate(slashEffectPrefab, effectSpawnPoint.position, Quaternion.identity);
            fx.transform.localScale = new Vector3(dir, 1, 1);
            Destroy(fx, 0.5f);
        }

        var hits = Physics2D.OverlapBoxAll(center, slashBoxSize, 0f, enemyMask);
        float finalDamage = slashDamage * Mathf.Max(0.1f, ctrl.attackPowerMul);

        foreach (var h in hits)
        {
            if (h.attachedRigidbody && h.attachedRigidbody.gameObject == this.gameObject) continue;

            var dmg = h.GetComponentInParent<Damageable>();
            if (dmg != null)
            {
                Vector2 knock = new Vector2(dir * slashKnockback, slashKnockback * 0.25f);

                dmg.TakeHit(Mathf.RoundToInt(finalDamage), knock, h.transform.position);

                if (hitEffectPrefab)
                {
                    var hitFx = Instantiate(hitEffectPrefab, h.transform.position, Quaternion.identity);
                    Destroy(hitFx, 0.3f);
                }
            }
        }
    }

    // ===== Buff =====
    void HandleBuff()
    {
        if (!Input.GetKeyDown(buffKey)) return;
        if (Time.time - lastBuffTime < buffCooldown) return;

        lastBuffTime = Time.time;

        FlexibleSetParam(anim, buffParam);

        if (!useAnimEventForBuff)
        {
            if (buffCo != null) StopCoroutine(buffCo);
            buffCo = StartCoroutine(CoBuffTimer());
        }
    }

    public void AnimEvent_BuffOn()
    {
        if (!useAnimEventForBuff) return;
        if (buffCo != null) StopCoroutine(buffCo);
        buffCo = StartCoroutine(CoBuffTimer());
    }

    IEnumerator CoBuffTimer()
    {
        ctrl.moveSpeedMul *= moveSpeedMul;
        ctrl.attackPowerMul *= attackPowerMul;

        // 1) 버프 캐스트 FX
        if (buffCastEffectPrefab)
        {
            var castFx = Instantiate(buffCastEffectPrefab, transform.position + buffCastEffectOffset, Quaternion.identity);
            Destroy(castFx, 0.7f);
            yield return new WaitForSeconds(0.7f);
        }

        // 2) 버프 오라 FX
        if (buffEffectPrefab)
        {
            activeBuffFx = Instantiate(buffEffectPrefab, transform.position + buffEffectOffset, Quaternion.identity);
            activeBuffFx.transform.SetParent(transform);
        }

        // 3) 유지
        float end = Time.time + buffDuration;
        while (Time.time < end) yield return null;

        // 4) 해제
        ctrl.moveSpeedMul /= moveSpeedMul;
        ctrl.attackPowerMul /= attackPowerMul;

        if (activeBuffFx) Destroy(activeBuffFx);
        buffCo = null;
    }

    void OnDrawGizmosSelected()
    {
        if (!enabled) return;

        int dir = 1;
        var c = GetComponent<SpumPlatformerController>();
        if (c) dir = c.FacingDir;

        // --- 수정된 부분 ---
        Vector2 baseOffset = slashBoxOffset;
        Vector2 center = (Vector2)(hitOrigin ? hitOrigin.position : transform.position)
                         + new Vector2(baseOffset.x * dir, baseOffset.y);

        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.35f);
        Gizmos.DrawCube(center, slashBoxSize);
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 1f);
        Gizmos.DrawWireCube(center, slashBoxSize);
    }

    // ---------- 유틸 ----------
    static string FindFirstExistingParam(Animator a, string[] candidates)
    {
        foreach (var name in candidates)
        {
            if (string.IsNullOrEmpty(name)) continue;
            foreach (var p in a.parameters)
                if (p.name == name) return name;
        }
        return "";
    }

    static void FlexibleSetParam(Animator a, string name)
    {
        if (!a || string.IsNullOrEmpty(name)) return;

        foreach (var p in a.parameters)
        {
            if (p.name != name) continue;

            switch (p.type)
            {
                case AnimatorControllerParameterType.Trigger:
                    a.SetTrigger(name);
                    return;
                case AnimatorControllerParameterType.Bool:
                    a.SetBool(name, true);
                    a.Update(0f);
                    a.SetBool(name, false);
                    return;
                case AnimatorControllerParameterType.Int:
                    a.SetInteger(name, 1);
                    a.Update(0f);
                    a.SetInteger(name, 0);
                    return;
                case AnimatorControllerParameterType.Float:
                    a.SetFloat(name, 1f);
                    a.Update(0f);
                    a.SetFloat(name, 0f);
                    return;
            }
        }

        Debug.LogWarning($"Animator parameter '{name}' not found.");
    }
}
