using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }
    public GameRunState RunState { get; private set; } = new GameRunState();

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // === 아이템 적용 ===
    public void AddAttack(int v)
    {
        RunState.AttackFlat += v;
        var ps = PlayerContext.TryGet()?.PlayerStats;
        ps?.AddAttackBonus(v);
    }

    public void AddSpeed(float percent)
    {
        RunState.SpeedPercent += percent;
        var ps = PlayerContext.TryGet()?.PlayerStats;
        ps?.AddSpeedMultiplier(percent);
    }

    public void AddMaxHP(int v)
    {
        RunState.MaxHPBonus += v;
        var ps = PlayerContext.TryGet()?.PlayerStats;
        ps?.AddMaxHP(v);
    }

    // === 런 효과 초기화 (GameOver 등에서 호출) ===
    public void ClearRunEffects()
    {
        var ps = PlayerContext.TryGet()?.PlayerStats;
        if (ps != null)
        {
            ps.AddAttackBonus(-RunState.AttackFlat);
            ps.AddSpeedMultiplier(-RunState.SpeedPercent);
            ps.AddMaxHP(-RunState.MaxHPBonus);
        }
        RunState = new GameRunState();
    }
}
