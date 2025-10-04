// Scripts/Player/PlayerSpawnBinder.cs
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerSpawnBinder : MonoBehaviour
{
    PlayerStats stats;
    void Awake(){ stats = GetComponent<PlayerStats>(); }

    // 씬이 로드되거나 리스폰 시 GameState -> Player에 적용
    public void ApplySnapshotToPlayer(){
        var s = GameState.I.player;
        stats.maxHP = s.maxHP; stats.maxMP = s.maxMP;
        stats.SetHPMP(s.hp, s.mp); // 아래 확장 메서드 참고
    }

    void Start(){
        // HUD가 켜져 있는 중간 합류 대비
        ApplySnapshotToPlayer();

        // 플레이어 변화 -> GameState로 반영
        stats.OnHPChanged += (cur,max) => GameState.I.UpdateHPMP(cur, max, stats.MP, stats.maxMP);
        stats.OnMPChanged += (cur,max) => GameState.I.UpdateHPMP(stats.HP, stats.maxHP, cur, max);
        stats.OnDied += () => GameState.I.UpdateHPMP(0, stats.maxHP, stats.MP, stats.maxMP);
    }
}
