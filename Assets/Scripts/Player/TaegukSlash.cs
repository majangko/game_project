using UnityEngine;
using System.Collections;

public class TaegukSlash : SkillBase
{
    [Header("Charge Settings")]
    public float minChargeTime = 0.3f;
    public float maxChargeTime = 2f;
    public string chargeAnim = "8_Charge";
    public string slashAnim = "7_Skill";

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

    [Header("Audio Clips")]
    [Tooltip("차징 중 재생되는 소리 (루프)")]
    public AudioClip chargeSound;
    [Tooltip("공격 발동 시 재생되는 소리 (1회)")]
    public AudioClip slashSound;

    private bool isCharging = false;
    private float chargeTimer = 0f;
    private AudioSource chargeAudioSource; // 루프용 사운드 컨트롤러

    protected override void Start()
    {
        base.Start();
        // 오디오 소스 준비
        chargeAudioSource = gameObject.AddComponent<AudioSource>();
        chargeAudioSource.loop = true;
        chargeAudioSource.playOnAwake = false;
    }

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

        // 🔹 차징 사운드 시작 (루프 재생)
        if (chargeSound && SoundManager.Instance)
        {
            chargeAudioSource.clip = chargeSound;
            chargeAudioSource.volume = 1.0f;
            chargeAudioSource.Play();
        }

        // 🔹 키를 누르고 있는 동안 차징
        while (Input.GetKey(KeyCode.X))
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0, maxChargeTime);
            yield return null;
        }

        // 🔹 키에서 손을 뗀 순간 — 발동
        if (anim)
        {
            anim.SetBool(chargeAnim, false);
            anim.SetTrigger(slashAnim);
        }

        // 🔹 차징 사운드 즉시 정지
        if (chargeAudioSource.isPlaying)
            chargeAudioSource.Stop();

        // 🔹 공격 사운드 재생
        if (slashSound && SoundManager.Instance)
            SoundManager.Instance.PlaySFX(slashSound);

        // 🔹 타격 처리
        PerformSlash();

        isCharging = false;

        // ✅ SkillBase 쿨타임 및 HUD 처리
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
