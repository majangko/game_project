// Assets/Scripts/Shop/ShopItemSO.cs
using UnityEngine;

public enum ShopItemType { Consumable, Equipment, KeyItem }

[CreateAssetMenu(fileName = "ShopItem", menuName = "Game/Shop/Item")]
public class ShopItemSO : ScriptableObject
{
    // -------------------------
    // 🔗 단일 소스(아이템 카탈로그) 참조
    // -------------------------
    [Header("Catalog Link (선택)")]
    [Tooltip("여기에 실제 아이템 SO(예: AttackBoostItemSO, MaxHPBoostItemSO 등)를 연결하면, 아래 표시/식별 값은 이 참조를 우선 사용합니다.")]
    public ItemBaseSO itemRef;   // ← 연결하면 아래 값들보다 우선

    // -------------------------
    // 🧾 기존/레거시 표시 & 식별 정보 (fallback)
    // -------------------------
    [Header("기본 정보 (Fallback)")]
    [Tooltip("itemRef가 비어있을 때 사용하는 내부 식별자. 가능하면 itemRef를 사용하세요.")]
    public string itemId;                 // 내부 식별용(고유)
    public string displayName;            // 표시 이름
    [TextArea] public string description; // 설명
    public Sprite icon;                   // 아이콘
    public ShopItemType type = ShopItemType.Consumable;

    // -------------------------
    // 💰 가격
    // -------------------------
    [Header("가격")]
    [Min(0)] public int basePrice = 10;   // 단가 G

    // -------------------------
    // 📦 읽기전용 접근자 (Shop/UI/Popup에서 공통 사용)
    // -------------------------
    /// <summary>실제 사용할 아이템 ID (itemRef가 있으면 그쪽, 없으면 레거시 itemId)</summary>
    public string ItemId => itemRef ? itemRef.ItemId : itemId;

    /// <summary>표시 이름</summary>
    public string DisplayName => itemRef
        ? (string.IsNullOrEmpty(itemRef.DisplayName) ? itemRef.name : itemRef.DisplayName)
        : (string.IsNullOrEmpty(displayName) ? name : displayName);

    /// <summary>아이콘</summary>
    public Sprite Icon => itemRef ? itemRef.Icon : icon;

    /// <summary>설명</summary>
    public string Description => itemRef ? itemRef.Description : description;

    /// <summary>소비형 아이템으로 캐스팅 (없으면 null)</summary>
    public ConsumableItemSO AsConsumable() => itemRef as ConsumableItemSO;

#if UNITY_EDITOR
    // 편의: itemRef를 연결했는데 레거시 필드가 비어있으면 자동 동기화(보기 좋게)
    private void OnValidate()
    {
        if (!itemRef) return;

        if (string.IsNullOrEmpty(itemId))
            itemId = itemRef.ItemId;

        if (string.IsNullOrEmpty(displayName))
            displayName = string.IsNullOrEmpty(itemRef.DisplayName) ? itemRef.name : itemRef.DisplayName;

        if (!icon && itemRef.Icon)
            icon = itemRef.Icon;

        if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(itemRef.Description))
            description = itemRef.Description;
    }
#endif
}
