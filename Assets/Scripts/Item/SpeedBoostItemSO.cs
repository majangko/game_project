using UnityEngine;

[CreateAssetMenu(menuName = "Items/Consumable/SpeedBoost")]
public class SpeedBoostItemSO : ConsumableItemSO
{
    [SerializeField, Tooltip("0.015 = +1.5%")]
    private float speedPercent = 0.015f; // 예: 0.015 / 0.02 / 0.03

    public override void ApplyEffect(PlayerContext ctx, StatusEffectManager effects)
    {
        effects.AddSpeed(speedPercent);
    }
}
