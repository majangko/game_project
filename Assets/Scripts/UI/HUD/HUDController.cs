using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Portrait")]
    public Image portrait;

    [Header("HP")]
    public Image hpFill;     // HPBar_Fill (Image, Type=Filled)
    public TMP_Text hpText;  // "현재 / 최대"

    [Header("MP")]
    public Image mpFill;     // MPBar_Fill (Image, Type=Filled)
    public TMP_Text mpText;  // "현재 / 최대"

    private PlayerStats _stats;

    void Start() { BindToPlayer(); }
    void OnEnable() { BindToPlayer(); }

    void BindToPlayer()
    {
        _stats = FindObjectOfType<PlayerStats>();
        if (_stats == null) return;

        // 이벤트 연결
        _stats.OnHPChanged -= OnHPChanged;
        _stats.OnMPChanged -= OnMPChanged;
        _stats.OnHPChanged += OnHPChanged;
        _stats.OnMPChanged += OnMPChanged;

        // 초기값 표시
        OnHPChanged(_stats.HP, _stats.maxHP);
        OnMPChanged(_stats.MP, _stats.maxMP);
    }

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

    // HUD를 특정 씬에서 끄고 싶을 때
    public void SetVisible(bool v) => gameObject.SetActive(v);
}
