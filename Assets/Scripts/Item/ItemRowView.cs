using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemRowView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtDesc;
    [SerializeField] private TextMeshProUGUI txtQty;
    [SerializeField] private Button rowButton;   // 누르면 선택

    private string _itemId;

    public void Bind(string itemId, string displayName, string description, Sprite sprite, int qty, Action<string> onClick)
    {
        _itemId = itemId;

        if (icon)    icon.sprite = sprite;
        if (txtName) txtName.text = displayName ?? itemId;
        if (txtDesc) txtDesc.text = description ?? "";
        if (txtQty)  txtQty.text  = $"x{qty}";

        if (rowButton == null) rowButton = GetComponent<Button>();
        if (rowButton != null)
        {
            rowButton.onClick.RemoveAllListeners();
            rowButton.onClick.AddListener(() => onClick?.Invoke(_itemId));
        }
    }

    // 선택 시 하이라이트(선택 효과가 필요하면 색만 조정)
    public void SetSelected(bool selected)
    {
        var img = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
        img.color = selected ? new Color(1, 1, 1, 0.15f) : new Color(1, 1, 1, 0f);
    }
}
