using UnityEngine;

public abstract class ConsumableItemSO : ItemBaseSO
{
    // 아이템 사용 시 호출되는 효과 적용 진입점
    public abstract void ApplyEffect(PlayerContext ctx, StatusEffectManager effects);
}
