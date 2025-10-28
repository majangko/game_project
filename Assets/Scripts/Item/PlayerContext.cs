using UnityEngine;

public class PlayerContext
{
    public PlayerStats PlayerStats { get; private set; }
    public PlayerContext(PlayerStats stats){ PlayerStats=stats; }
    public static PlayerContext TryGet(){ var ps = Object.FindFirstObjectByType<PlayerStats>(); return ps? new PlayerContext(ps) : null; }
}
