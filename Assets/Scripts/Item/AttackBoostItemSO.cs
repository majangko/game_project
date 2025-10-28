using UnityEngine;

[CreateAssetMenu(menuName = "Items/Consumable/AttackBoost")]
public class AttackBoostItemSO : ConsumableItemSO
{
    [SerializeField] private int attackFlat = 1; // 예: 1,3,5,7,10

    public override void ApplyEffect(PlayerContext ctx, StatusEffectManager effects)
    {
        effects.AddAttack(attackFlat);
    }
}
