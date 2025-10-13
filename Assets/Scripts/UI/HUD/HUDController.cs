using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Portrait")]
    public Image portrait;

    [Header("HP / MP")]
    public Image hpFill;
    public TMP_Text hpText;
    public Image mpFill;
    public TMP_Text mpText;

    [Header("Skills")]
    public Image[] skillIcons;
    public Image[] skillCooldowns;
    public TMP_Text[] skillCooldownTexts;

    [Header("Audio")]
    public AudioClip cooldownEndSFX;

    private PlayerStats _stats;
    private Coroutine[] cooldownCoroutines;

    // ============================================================
    // Unity Life Cycle
    // ============================================================
    void Start()
    {
        if (_stats == null)
            _stats = FindObjectOfType<PlayerStats>();

        if (portrait == null)
        {
            portrait = transform.Find("HUD_Frame/Portrait")?.GetComponent<Image>();
            if (portrait == null)
                portrait = GetComponentInChildren<Image>(true);
        }

        if (_stats != null)
            BindToPlayer(_stats);
    }

    void OnDisable()
    {
        UnsubscribeSkillEvents();

        if (_stats != null)
        {
            _stats.OnHPChanged -= OnHPChanged;
            _stats.OnMPChanged -= OnMPChanged;
        }
    }

    // ============================================================
    // 플레이어 바인딩
    // ============================================================
    public void BindToPlayer(PlayerStats newPlayer)
    {
        // 기존 구독 해제
        if (_stats != null)
        {
            _stats.OnHPChanged -= OnHPChanged;
            _stats.OnMPChanged -= OnMPChanged;
            UnsubscribeSkillEvents();
        }

        _stats = newPlayer;
        if (_stats == null) return;

        _stats.OnHPChanged += OnHPChanged;
        _stats.OnMPChanged += OnMPChanged;

        OnHPChanged(_stats.HP, _stats.maxHP);
        OnMPChanged(_stats.MP, _stats.maxMP);

        ApplyCharacterData();

        // ✅ 쿨타임 즉시 리셋 (코루틴 포함)
        ResetCooldowns();

        // ✅ 새 캐릭터의 스킬 이벤트 등록
        SubscribeSkillEvents(_stats);

        Debug.Log($"[HUD] {_stats.name} 스킬 이벤트 구독 완료 및 HUD 초기화 완료");
    }

    void ApplyCharacterData()
    {
        if (portrait && _stats.portrait)
            portrait.sprite = _stats.portrait;

        ApplySkillIcons(_stats.skillIcons);
        cooldownCoroutines = new Coroutine[skillIcons.Length];
    }

    void ApplySkillIcons(Sprite[] icons)
    {
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (i < icons.Length && icons[i] != null)
                skillIcons[i].sprite = icons[i];
            else
                skillIcons[i].sprite = null;
        }
    }

    // ============================================================
    // 스킬 이벤트 연결 / 해제
    // ============================================================
    private void SubscribeSkillEvents(PlayerStats stats)
    {
        var skills = stats.GetComponentsInChildren<SkillBase>(true);
        foreach (var skill in skills)
            skill.OnSkillUsedInstance += OnSkillUsed;

        Debug.Log($"[HUD] {stats.name} 스킬 이벤트 {skills.Length}개 구독 완료");
    }

    private void UnsubscribeSkillEvents()
    {
        if (_stats == null) return;

        var skills = _stats.GetComponentsInChildren<SkillBase>(true);
        foreach (var skill in skills)
            skill.OnSkillUsedInstance -= OnSkillUsed;
    }

    // ============================================================
    // HP / MP 갱신
    // ============================================================
    void OnHPChanged(int cur, int max)
    {
        if (hpFill) hpFill.fillAmount = max > 0 ? (float)cur / max : 0f;
        if (hpText) hpText.text = $"{cur} / {max}";
    }

    void OnMPChanged(int cur, int max)
    {
        if (mpFill) mpFill.fillAmount = max > 0 ? (float)cur / max : 0f;
        if (mpText) mpText.text = $"{cur} / {max}";
    }

    // ============================================================
    // 스킬 쿨타임 처리
    // ============================================================
    void OnSkillUsed(string skillName, float cooldown)
    {
        Debug.Log($"[HUD] Skill Used: {skillName}, Cooldown: {cooldown}");

        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i]?.sprite == null) continue;

            string iconName = skillIcons[i].sprite.name.ToLower();
            string skillLower = skillName.ToLower();

            // 🔹 이름 유연 매칭 (앞뒤/대소문자 무시)
            if (iconName.Contains(skillLower) || skillLower.Contains(iconName))
            {
                Debug.Log($"[HUD] 쿨타임 매칭 성공 → {skillName} == {iconName}");

                if (cooldownCoroutines[i] != null)
                    StopCoroutine(cooldownCoroutines[i]);

                cooldownCoroutines[i] = StartCoroutine(CooldownRoutine(i, cooldown));
                return;
            }
        }

        Debug.LogWarning($"[HUD] {skillName} 과 일치하는 스킬 아이콘을 찾지 못함 ❌");
    }

    IEnumerator CooldownRoutine(int index, float cooldown)
    {
        float timer = cooldown;
        Image icon = skillIcons[index];
        Image overlay = (skillCooldowns != null && index < skillCooldowns.Length) ? skillCooldowns[index] : null;
        TMP_Text cdText = (skillCooldownTexts != null && index < skillCooldownTexts.Length) ? skillCooldownTexts[index] : null;

        if (icon) icon.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        if (overlay) overlay.gameObject.SetActive(true);

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / cooldown);
            if (overlay) overlay.fillAmount = ratio;
            if (cdText) cdText.text = $"{Mathf.Ceil(timer)}";
            yield return null;
        }

        if (overlay)
        {
            overlay.fillAmount = 0;
            overlay.gameObject.SetActive(false);
        }
        if (cdText) cdText.text = "";

        if (cooldownEndSFX)
            AudioSource.PlayClipAtPoint(cooldownEndSFX, Vector3.zero);

        if (icon)
            StartCoroutine(FlashIcon(icon));
    }

    IEnumerator FlashIcon(Image icon)
    {
        float duration = 0.3f;
        float t = 0f;

        Outline outline = icon.GetComponent<Outline>();
        if (!outline)
            outline = icon.gameObject.AddComponent<Outline>();

        outline.effectColor = new Color(1f, 0.9f, 0.4f, 1f);
        outline.effectDistance = new Vector2(3, 3);

        while (t < duration)
        {
            t += Time.deltaTime;
            float intensity = Mathf.PingPong(Time.time * 4f, 1f);
            outline.effectColor = Color.Lerp(Color.white, new Color(1f, 0.9f, 0.3f), intensity);
            yield return null;
        }

        Destroy(outline);
        icon.color = Color.white;
    }

    // ============================================================
    // 쿨타임 초기화 (태그 시 완전 정리)
    // ============================================================
    public void ResetCooldowns()
    {
        // ✅ 모든 쿨타임 코루틴 중지
        StopAllCoroutines();

        if (cooldownCoroutines != null)
        {
            for (int i = 0; i < cooldownCoroutines.Length; i++)
                cooldownCoroutines[i] = null;
        }

        // ✅ 색상, 텍스트, 오버레이 초기화
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i] != null)
                skillIcons[i].color = Color.white;

            if (skillCooldowns != null && i < skillCooldowns.Length && skillCooldowns[i] != null)
            {
                skillCooldowns[i].fillAmount = 0f;
                skillCooldowns[i].gameObject.SetActive(false);
            }

            if (skillCooldownTexts != null && i < skillCooldownTexts.Length && skillCooldownTexts[i] != null)
                skillCooldownTexts[i].text = "";
        }

        Debug.Log("[HUD] 쿨타임 UI 완전 초기화 완료 (StopAllCoroutines 포함)");
    }

    public void SetVisible(bool v) => gameObject.SetActive(v);
}
