using UnityEngine;

public abstract class ItemBaseSO : ScriptableObject
{
    [field: SerializeField] public string ItemId { get; private set; }   // 예: "HP_10"
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [TextArea, SerializeField] public string Description;
    public virtual bool IsConsumable => true; // 전부 1회용
}
