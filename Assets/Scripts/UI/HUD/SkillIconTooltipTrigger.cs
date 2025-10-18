using UnityEngine;
using UnityEngine.EventSystems;

public class SkillIconTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Data")]
    [Tooltip("이 아이콘에 연결된 SkillBase (자동 또는 수동 연결 가능)")]
    public SkillBase skill;  // ✅ HUDController에서 자동으로 채워짐

    [Tooltip("이름을 직접 덮어쓸 경우 입력 (비워두면 SkillBase에서 자동 읽음)")]
    public string skillNameOverride;

    [TextArea(2, 4)]
    [Tooltip("설명을 직접 덮어쓸 경우 입력 (비워두면 SkillBase에서 자동 읽음)")]
    public string skillDescriptionOverride;

    [Tooltip("추가 정보 (쿨타임 등, 비워두면 자동 생성)")]
    public string extraInfoOverride;

    // 내부 캐시용
    private string resolvedName;
    private string resolvedDesc;
    private string resolvedExtra;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SkillTooltipUI.Instance == null)
            return;

        ResolveTooltipData();
        SkillTooltipUI.Instance.Show(resolvedName, resolvedDesc, resolvedExtra);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SkillTooltipUI.Instance != null)
            SkillTooltipUI.Instance.Hide();
    }

    // 🔹 SkillBase로부터 정보 자동 가져오기
    private void ResolveTooltipData()
    {
        if (skill != null)
        {
            resolvedName = !string.IsNullOrEmpty(skillNameOverride)
                ? skillNameOverride
                : skill.GetSkillDisplayName();

            resolvedDesc = !string.IsNullOrEmpty(skillDescriptionOverride)
                ? skillDescriptionOverride
                : skill.GetSkillDescription();

            resolvedExtra = !string.IsNullOrEmpty(extraInfoOverride)
                ? extraInfoOverride
                : (skill.GetCooldown() > 0
                    ? $"쿨타임: {skill.GetCooldown():F1}초"
                    : "패시브 스킬");
        }
        else
        {
            // SkillBase가 연결되지 않았을 때 대비용
            resolvedName = string.IsNullOrEmpty(skillNameOverride) ? "스킬 없음" : skillNameOverride;
            resolvedDesc = string.IsNullOrEmpty(skillDescriptionOverride) ? "설명이 없습니다." : skillDescriptionOverride;
            resolvedExtra = string.IsNullOrEmpty(extraInfoOverride) ? "" : extraInfoOverride;
        }
    }
}
