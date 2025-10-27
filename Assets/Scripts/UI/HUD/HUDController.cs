using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Portraits")]
    [Tooltip("파티 초상화 슬롯 (최대 3개)")]
    public Image[] portraits;            // 여러 멤버용 초상화 슬롯
    public Image activeHighlight;        // 현재 조작 중인 캐릭터 표시
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(1f, 1f, 1f, 0.4f);

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
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        // PartyManager, TagManager, PlayerStats 초기화 기다림
        yield return null;
        yield return null;

        // PartyManager 연결 후 초상화 세팅
        if (PartyManager.Instance != null)
            UpdatePartyPortraits();
        else
            Debug.LogWarning("[HUD] PartyManager.Instance가 아직 없음.");

        // PlayerStats 자동 감지
        if (_stats == null)
            _stats = FindObjectOfType<PlayerStats>();

        if (_stats != null)
            BindToPlayer(_stats);
        else
            Debug.LogWarning("[HUD] PlayerStats를 찾지 못함.");

        // 태그 이벤트 구독
        if (TagManager.Instance != null)
            TagManager.Instance.OnCharacterSwitched.AddListener(OnCharacterSwitched);
    }

    void OnDisable()
    {
        UnsubscribeSkillEvents();

        if (_stats != null)
        {
            _stats.OnHPChanged -= OnHPChanged;
            _stats.OnMPChanged -= OnMPChanged;
        }

        if (TagManager.Instance != null)
            TagManager.Instance.OnCharacterSwitched.RemoveListener(OnCharacterSwitched);
    }

    // ============================================================
    // 파티 초상화 자동 표시
    // ============================================================
    public void UpdatePartyPortraits()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogWarning("[HUD] PartyManager.Instance가 null");
            return;
        }

        if (portraits == null || portraits.Length == 0)
        {
            Debug.LogWarning("[HUD] portraits 배열이 비어있음 (인스펙터 연결 필요)");
            return;
        }

        var members = PartyManager.Instance.currentMembers;
        if (members == null)
        {
            Debug.LogWarning("[HUD] PartyManager.currentMembers가 null");
            return;
        }

        for (int i = 0; i < portraits.Length; i++)
        {
            if (i < members.Count && members[i] != null && members[i].portrait != null)
            {
                portraits[i].sprite = members[i].portrait;
                portraits[i].color = activeColor;
            }
            else
            {
                portraits[i].sprite = null;
                portraits[i].color = new Color(1, 1, 1, 0); // 숨김
            }
        }

        Debug.Log($"[HUD] {members.Count}명의 파티 초상화 갱신 완료 ✅");
    }

    // ============================================================
    // 태그 교체 시 하이라이트 처리
    // ============================================================
    private void OnCharacterSwitched(int index)
    {
        if (portraits == null || portraits.Length == 0) return;

        for (int i = 0; i < portraits.Length; i++)
        {
            if (portraits[i] != null)
                portraits[i].color = (i == index) ? activeColor : inactiveColor;
        }

        Debug.Log($"[HUD] 캐릭터 교체됨 → 활성 인덱스 {index}");
    }

    // ============================================================
    // 플레이어 바인딩
    // ============================================================
    public void BindToPlayer(PlayerStats newPlayer)
    {
        if (newPlayer == null)
        {
            Debug.LogWarning("[HUD] BindToPlayer() - newPlayer가 null입니다.");
            return;
        }

        // 기존 구독 해제
        if (_stats != null)
        {
            _stats.OnHPChanged -= OnHPChanged;
            _stats.OnMPChanged -= OnMPChanged;
            UnsubscribeSkillEvents();
        }

        _stats = newPlayer;
        _stats.OnHPChanged += OnHPChanged;
        _stats.OnMPChanged += OnMPChanged;

        OnHPChanged(_stats.HP, _stats.maxHP);
        OnMPChanged(_stats.MP, _stats.maxMP);

        ApplyCharacterData();
        ResetCooldowns();
        SubscribeSkillEvents(_stats);
        ApplySkillTooltips(_stats);

        Debug.Log($"[HUD] {_stats.name} 스킬 이벤트 구독 및 HUD 초기화 완료 ✅");
    }

    // ============================================================
    // 캐릭터 데이터 적용
    // ============================================================
    void ApplyCharacterData()
    {
        if (_stats == null)
        {
            Debug.LogWarning("[HUD] _stats가 null입니다. ApplyCharacterData 중단");
            return;
        }

        // ✅ 초상화 1번 슬롯에 현재 캐릭터 표시
        if (portraits != null && portraits.Length > 0)
        {
            if (portraits[0] != null && _stats.portrait != null)
            {
                portraits[0].sprite = _stats.portrait;
                portraits[0].color = activeColor;
            }
        }

        // ✅ 스킬 아이콘 방어
        if (skillIcons == null)
        {
            Debug.LogWarning("[HUD] skillIcons 배열이 null입니다.");
            return;
        }

        if (_stats.skillIcons == null)
        {
            Debug.LogWarning($"[HUD] {_stats.name}의 skillIcons 배열이 null입니다.");
            return;
        }

        ApplySkillIcons(_stats.skillIcons);
        cooldownCoroutines = new Coroutine[skillIcons.Length];
    }

    // ============================================================
    // 스킬 아이콘 적용
    // ============================================================
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
    // 스킬 이벤트 연결 / 해제
    // ============================================================
    private void SubscribeSkillEvents(PlayerStats stats)
    {
        var skills = stats.GetComponentsInChildren<SkillBase>(true);
        foreach (var skill in skills)
            skill.OnSkillUsedInstance += OnSkillUsed;
    }

    private void UnsubscribeSkillEvents()
    {
        if (_stats == null) return;
        var skills = _stats.GetComponentsInChildren<SkillBase>(true);
        foreach (var skill in skills)
            skill.OnSkillUsedInstance -= OnSkillUsed;
    }

    // ============================================================
    // 스킬 쿨타임 처리
    // ============================================================
    void OnSkillUsed(string skillName, float cooldown)
    {
        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i]?.sprite == null) continue;
            string iconName = skillIcons[i].sprite.name.ToLower();
            string skillLower = skillName.ToLower();

            if (iconName.Contains(skillLower) || skillLower.Contains(iconName))
            {
                if (cooldownCoroutines[i] != null)
                    StopCoroutine(cooldownCoroutines[i]);
                cooldownCoroutines[i] = StartCoroutine(CooldownRoutine(i, cooldown));
                return;
            }
        }
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
        Outline outline = icon.GetComponent<Outline>() ?? icon.gameObject.AddComponent<Outline>();
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
    // 쿨타임 초기화
    // ============================================================
    public void ResetCooldowns()
    {
        StopAllCoroutines();

        if (cooldownCoroutines != null)
        {
            for (int i = 0; i < cooldownCoroutines.Length; i++)
                cooldownCoroutines[i] = null;
        }

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
    }

    // ============================================================
    // 스킬 툴팁 등록
    // ============================================================
    private void ApplySkillTooltips(PlayerStats stats)
    {
        if (skillIcons == null || skillIcons.Length == 0) return;

        var skills = stats.GetComponentsInChildren<SkillBase>(true);

        for (int i = 0; i < skillIcons.Length; i++)
        {
            Image icon = skillIcons[i];
            if (icon == null || icon.sprite == null) continue;

            string iconName = icon.sprite.name.ToLower();

            foreach (var skill in skills)
            {
                if (skill == null || string.IsNullOrEmpty(skill.skillName)) continue;

                string skillLower = skill.skillName.ToLower();
                if (iconName.Contains(skillLower) || skillLower.Contains(iconName))
                {
                    var trigger = icon.GetComponent<SkillIconTooltipTrigger>() ?? icon.gameObject.AddComponent<SkillIconTooltipTrigger>();
                    trigger.skill = skill;
                    trigger.skillNameOverride = skill.GetSkillDisplayName();
                    trigger.skillDescriptionOverride = skill.GetSkillDescription();
                    trigger.extraInfoOverride = skill.GetCooldown() > 0
                        ? $"쿨타임: {skill.GetCooldown():F1}초"
                        : "패시브 스킬";
                    break;
                }
            }
        }
    }

    // ============================================================
    // HUD 표시 On/Off
    // ============================================================
    public void SetVisible(bool v) => gameObject.SetActive(v);
}
