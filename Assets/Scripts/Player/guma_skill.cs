// guma_skill.cs
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
    [Tooltip("배기 스킬용 파라미터(Trigger/Bool/Int/Float 상관없음). 비워두면 자동 탐색합니다.")]
    public string slashParam = "";  // ex) "1_Skill_Normal"
    [Tooltip("버프 스킬용 파라미터(Trigger/Bool/Int/Float 상관없음). 비워두면 자동 탐색합니다.")]
    public string buffParam = "";   // ex) "0_Buff"

    [Header("Crescent Slash (배기형) - Key: X")]
    public KeyCode slashKey = KeyCode.X;
    public float slashDamage = 30f;
    public Vector2 slashBoxSize = new Vector2(2.8f, 1.2f);
    public Vector2 slashBoxOffset = new Vector2(1.6f, 0.2f);
    public float slashKnockback = 8f;
    public float slashCooldown = 1.2f;

    [Tooltip("애니메이션 이벤트 AnimEvent_SlashHit를 사용할지")]
    public bool useAnimEventForSlash = true;
    [Tooltip("이벤트가 없거나 누락되었을 때 대체 대기 시간")]
    public float slashWindupFallback = 0.08f;
    [Tooltip("히트 후 짧은 경직")]
    public float slashFreeze = 0.10f;

    float lastSlashTime = -999f;
    bool slashEventFired = false;

    [Header("Battle Cry (자기 버프) - Key: C")]
    public KeyCode buffKey = KeyCode.C;
    public float moveSpeedMul = 1.3f;
    public float attackPowerMul = 1.4f;
    public float buffDuration = 6f;
    public float buffCooldown = 12f;

    [Tooltip("버프 적용을 애니메이션 이벤트 AnimEvent_BuffOn 타이밍에 할지")]
    public bool useAnimEventForBuff = false;

    float lastBuffTime = -999f;
    Coroutine buffCo;

    void Awake()
    {
        ctrl = GetComponent<SpumPlatformerController>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        if (!hitOrigin) hitOrigin = transform;

        // --- 파라미터 자동 탐색 (SPUM 네이밍 우선 후보 반영) ---
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

    // ===== 배기 스킬 =====
    void HandleSlash()
    {
        if (!Input.GetKeyDown(slashKey)) return;
        if (Time.time - lastSlashTime < slashCooldown) return;

        lastSlashTime = Time.time;
        slashEventFired = false;

        // 애니메이션 호출(파라미터 타입에 상관없이 유연 처리)
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
            if (!slashEventFired) DoSlashHit(); // 폴백
        }
        else
        {
            yield return new WaitForSeconds(Mathf.Max(0f, slashWindupFallback));
            DoSlashHit();
        }

        if (slashFreeze > 0f) yield return new WaitForSeconds(slashFreeze);
    }

    // 애니메이션 이벤트에서 호출
    public void AnimEvent_SlashHit()
    {
        if (slashEventFired) return;
        DoSlashHit();
        slashEventFired = true;
    }

    void DoSlashHit()
    {
        int dir = ctrl.FacingDir;
        Vector2 center = (Vector2)(hitOrigin ? hitOrigin.position : transform.position)
                         + new Vector2(slashBoxOffset.x * dir, slashBoxOffset.y);

        var hits = Physics2D.OverlapBoxAll(center, slashBoxSize, 0f, enemyMask);
        float finalDamage = slashDamage * Mathf.Max(0.1f, ctrl.attackPowerMul);

        foreach (var h in hits)
        {
            if (h.attachedRigidbody && h.attachedRigidbody.gameObject == this.gameObject) continue;

            var dmg = h.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(finalDamage);
                var rb2 = h.attachedRigidbody;
                if (rb2 != null)
                    rb2.AddForce(new Vector2(dir * slashKnockback, slashKnockback * 0.25f), ForceMode2D.Impulse);
            }
        }
    }

    // ===== 버프 스킬 =====
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

    // 애니메이션 이벤트에서 호출
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

        float end = Time.time + buffDuration;
        while (Time.time < end) yield return null;

        ctrl.moveSpeedMul /= moveSpeedMul;
        ctrl.attackPowerMul /= attackPowerMul;
        buffCo = null;
    }

    void OnDrawGizmosSelected()
    {
        if (!enabled) return;

        int dir = 1;
        var c = GetComponent<SpumPlatformerController>();
        if (c) dir = c.FacingDir;

        Vector2 center = (Vector2)(hitOrigin ? hitOrigin.position : transform.position)
                         + new Vector2(slashBoxOffset.x * dir, slashBoxOffset.y);

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
        return ""; // 못 찾으면 빈 문자열
    }

    // 파라미터 타입에 상관없이 한 번 재생/트리거
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
                    // 한 프레임 true로 쏘고 다음 프레임에 false로 복귀
                    a.SetBool(name, true);
                    a.Update(0f); // 즉시 반영
                    a.SetBool(name, false);
                    return;
                case AnimatorControllerParameterType.Int:
                    // 0→1 펄스
                    a.SetInteger(name, 1);
                    a.Update(0f);
                    a.SetInteger(name, 0);
                    return;
                case AnimatorControllerParameterType.Float:
                    // 0→1 펄스
                    a.SetFloat(name, 1f);
                    a.Update(0f);
                    a.SetFloat(name, 0f);
                    return;
            }
        }

        // 못 찾았으면 안전하게 시도 중단
        Debug.LogWarning($"Animator parameter '{name}' not found.");
    }
}
