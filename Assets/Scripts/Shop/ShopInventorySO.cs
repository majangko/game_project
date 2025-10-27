// Assets/Scripts/Shop/ShopInventorySO.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopInventory", menuName = "Game/Shop/Inventory")]
public class ShopInventorySO : ScriptableObject
{
    public string shopTitle = "상점";
    public List<ShopItemSO> items = new List<ShopItemSO>();
}
