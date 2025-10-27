// Scripts/UI/HUD/BossHPUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHPUI : MonoBehaviour
{
    public Image fill;      // BossHP_Fill
    public TMP_Text hpText; // 선택

    private int maxHP, curHP;

    public void Bind(int current, int max)
    {
        curHP = current; maxHP = max;
        Refresh();
    }

    public void UpdateHP(int current)
    {
        curHP = Mathf.Clamp(current, 0, maxHP);
        Refresh();
    }

    void Refresh()
    {
        if (fill) fill.fillAmount = (maxHP > 0) ? (float)curHP / maxHP : 0f;
        if (hpText) hpText.text = $"{curHP} / {maxHP}";
    }
}
