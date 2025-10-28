using UnityEngine;

[CreateAssetMenu(menuName = "Items/Consumable/MaxHPBoost")]
public class MaxHPBoostItemSO : ConsumableItemSO
{
    [SerializeField] private int hpPlus = 10; // 예: 10,20,30,40,50

    public override void ApplyEffect(PlayerContext ctx, StatusEffectManager effects)
    {
        effects.AddMaxHP(hpPlus);
    }
}
