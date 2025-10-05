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

    // ✅ 자동 연결 및 즉시 바인딩
    void Start()
    {
        // 1️⃣ PlayerStats 자동 탐색
        if (_stats == null)
        {
            _stats = FindObjectOfType<PlayerStats>();
        }

        // 2️⃣ Portrait 자동 탐색
        if (portrait == null)
        {
            portrait = transform.Find("HUD_Frame/Portrait")?.GetComponent<Image>();
            if (portrait == null)
                portrait = GetComponentInChildren<Image>(true);
        }

        // 3️⃣ PlayerStats가 있다면 즉시 바인딩
        if (_stats != null)
            BindToPlayer(_stats);
    }

    void OnEnable()
    {
        // ✅ 모든 스킬 이벤트 통합 구독
        SkillBase.OnSkillUsed += OnSkillUsed;
        ExorcismCombo.OnSkillUsed += OnSkillUsed;  // 예전 호환 유지 (혹시 남아있는 스킬용)
    }

    void OnDisable()
    {
        SkillBase.OnSkillUsed -= OnSkillUsed;
        ExorcismCombo.OnSkillUsed -= OnSkillUsed;

        if (_stats != null)
        {
            _stats.OnHPChanged -= OnHPChanged;
            _stats.OnMPChanged -= OnMPChanged;
        }
    }

    // 🔹 PlayerStats 연결
    public void BindToPlayer(PlayerStats newPlayer)
    {
        if (_stats != null)
        {
            _stats.OnHPChanged -= OnHPChanged;
            _stats.OnMPChanged -= OnMPChanged;
        }

        _stats = newPlayer;
        if (_stats == null) return;

        _stats.OnHPChanged += OnHPChanged;
        _stats.OnMPChanged += OnMPChanged;

        OnHPChanged(_stats.HP, _stats.maxHP);
        OnMPChanged(_stats.MP, _stats.maxMP);

        ApplyCharacterData();
    }

    // 🔸 HUD 표시 데이터 적용
    void ApplyCharacterData()
    {
        // ✅ 초상화 표시
        if (portrait && _stats.portrait)
            portrait.sprite = _stats.portrait;

        // ✅ 스킬 아이콘 표시
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

    // 🔹 HP / MP 갱신
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

    // 🔸 스킬 쿨다운 표시
    void OnSkillUsed(string skillName, float cooldown)
    {
        Debug.Log($"[HUD] Skill Used: {skillName}, Cooldown: {cooldown}");

        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (skillIcons[i]?.sprite == null) continue;

            // 🔹 이름이 같거나 부분 일치하는 스킬에 반응 (예: TaegukSlash_0)
            if (skillIcons[i].sprite.name.Contains(skillName))
            {
                if (cooldownCoroutines[i] != null)
                    StopCoroutine(cooldownCoroutines[i]);

                cooldownCoroutines[i] = StartCoroutine(CooldownRoutine(i, cooldown));
                break;
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
        if (cdText)
            cdText.text = "";

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

    public void SetVisible(bool v) => gameObject.SetActive(v);
}
