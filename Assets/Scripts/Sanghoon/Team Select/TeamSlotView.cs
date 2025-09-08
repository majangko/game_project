// FILE: TeamSlotView.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamSlotView : MonoBehaviour
{
    [Header("UI")]
    public Image slotImage;     // 얼굴만(또는 작은 아이콘)
    public TMP_Text slotText;   // 이름(옵션)

    public void Set(TeamMemberSO m)
    {
        if (!m) return;
        if (slotImage) slotImage.sprite = m.portrait;
        if (slotText) slotText.text = m.displayName;
    }

    public void Clear()
    {
        if (slotImage) slotImage.sprite = null;
        if (slotText) slotText.text = string.Empty;
    }
}
