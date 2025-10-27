// Assets/Scripts/Shop/ShopItemSO.cs
using UnityEngine;

public enum ShopItemType { Consumable, Equipment, KeyItem }

[CreateAssetMenu(fileName = "ShopItem", menuName = "Game/Shop/Item")]
public class ShopItemSO : ScriptableObject
{
    [Header("기본 정보")]
    public string itemId;                 // 내부 식별용(고유)
    public string displayName;            // 표시 이름
    [TextArea] public string description; // 설명
    public Sprite icon;                   // 아이콘
    public ShopItemType type = ShopItemType.Consumable;

    [Header("가격")]
    [Min(0)] public int basePrice = 10;   // 단가 G
}
