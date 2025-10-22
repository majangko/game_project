using System.Collections.Generic;
using UnityEngine;

public class BuffUIController : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject buffSlotPrefab;
    public Transform buffContainer; // HUD 상단 영역

    private readonly Dictionary<BuffType, BuffSlotUI> activeBuffs = new();

    public void ShowBuff(BuffData data)
    {
        // 이미 존재하는 버프는 갱신
        if (activeBuffs.ContainsKey(data.type))
        {
            activeBuffs[data.type].ResetTimer(data.duration);
            return;
        }

        // 새 버프 생성
        GameObject obj = Instantiate(buffSlotPrefab, buffContainer);
        BuffSlotUI slot = obj.GetComponent<BuffSlotUI>();
        slot.Init(data);
        activeBuffs.Add(data.type, slot);
        ReorderIcons();
    }

    public void HideBuff(BuffType type)
    {
        if (!activeBuffs.ContainsKey(type)) return;

        BuffSlotUI slot = activeBuffs[type];
        activeBuffs.Remove(type);
        slot.FadeOutAndDestroy();
        ReorderIcons();
    }

    private void ReorderIcons()
    {
        // 수평 정렬 (왼쪽→오른쪽 또는 오른쪽→왼쪽)
        int index = 0;
        foreach (var kvp in activeBuffs)
        {
            var rect = kvp.Value.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(index * 64f, 0f); // 아이콘 간격 64px
            index++;
        }
    }
}
