using UnityEngine;
using UnityEngine.EventSystems;

public class SkillIconTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Data")]
    [Tooltip("이 아이콘에 연결된 SkillBase (HUD/TeamSelect 자동 연결)")]
    public SkillBase skill;

    [Tooltip("이름을 직접 덮어쓸 경우 입력 (비워두면 SkillBase에서 자동 읽음)")]
    public string skillNameOverride;

    [TextArea(2, 4)]
    [Tooltip("설명을 직접 덮어쓸 경우 입력 (비워두면 SkillBase에서 자동 읽음)")]
    public string skillDescriptionOverride;

    [Tooltip("추가 정보 (쿨타임 등, 비워두면 자동 생성)")]
    public string extraInfoOverride;

    private string resolvedName;
    private string resolvedDesc;
    private string resolvedExtra;
    private bool isHovered = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SkillTooltipUI.Instance == null)
            return;

        ResolveTooltipData();
        SkillTooltipUI.Instance.Show(resolvedName, resolvedDesc, resolvedExtra);
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SkillTooltipUI.Instance != null)
            SkillTooltipUI.Instance.Hide();
        isHovered = false;
    }

    void Update()
    {
        if (isHovered && SkillTooltipUI.Instance != null && SkillTooltipUI.Instance.gameObject.activeSelf)
        {
            RectTransform rect = SkillTooltipUI.Instance.GetComponent<RectTransform>();
            Vector3 pos = Input.mousePosition + new Vector3(20f, -15f, 0f);
            pos.x = Mathf.Clamp(pos.x, 0, Screen.width - rect.rect.width);
            pos.y = Mathf.Clamp(pos.y, rect.rect.height, Screen.height);
            rect.position = pos;
        }
    }

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
            resolvedName = string.IsNullOrEmpty(skillNameOverride) ? "스킬 없음" : skillNameOverride;
            resolvedDesc = string.IsNullOrEmpty(skillDescriptionOverride) ? "설명이 없습니다." : skillDescriptionOverride;
            resolvedExtra = string.IsNullOrEmpty(extraInfoOverride) ? "" : extraInfoOverride;
        }
    }
}
