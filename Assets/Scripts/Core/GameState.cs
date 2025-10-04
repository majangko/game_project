// Scripts/Core/GameState.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public static GameState I;

    [System.Serializable]
    public class PlayerSnapshot {
        public int maxHP = 100, maxMP = 50;
        public int hp = 100, mp = 50;
        public int gold = 0;
    }
    public PlayerSnapshot player = new PlayerSnapshot();

    void Awake(){
        if (I != null && I != this){ Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
        LoadFromDisk(); // 선택: 시작 시 복구
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m){
        // 새 씬에서 플레이어가 생성되면 스냅샷을 적용하도록 시도
        var binder = FindObjectOfType<PlayerSpawnBinder>();
        if (binder) binder.ApplySnapshotToPlayer();
    }

    // 외부에서 호출
    public void UpdateHPMP(int hp, int maxHP, int mp, int maxMP){
        player.hp = hp; player.maxHP = maxHP;
        player.mp = mp; player.maxMP = maxMP;
        SaveToDisk();
    }
    public void AddGold(int amount){ player.gold += amount; SaveToDisk(); }
    public bool SpendGold(int amount){
        if (player.gold < amount) return false;
        player.gold -= amount; SaveToDisk(); return true;
    }

    // 간단 저장(후에 Firebase로 교체 가능)
    const string KEY = "GSAVE_V1";
    void SaveToDisk(){
        PlayerPrefs.SetInt(KEY+"_hp", player.hp);
        PlayerPrefs.SetInt(KEY+"_maxhp", player.maxHP);
        PlayerPrefs.SetInt(KEY+"_mp", player.mp);
        PlayerPrefs.SetInt(KEY+"_maxmp", player.maxMP);
        PlayerPrefs.SetInt(KEY+"_gold", player.gold);
        PlayerPrefs.Save();
    }
    void LoadFromDisk(){
        if (!PlayerPrefs.HasKey(KEY+"_hp")) return;
        player.hp    = PlayerPrefs.GetInt(KEY+"_hp");
        player.maxHP = PlayerPrefs.GetInt(KEY+"_maxhp");
        player.mp    = PlayerPrefs.GetInt(KEY+"_mp");
        player.maxMP = PlayerPrefs.GetInt(KEY+"_maxmp");
        player.gold  = PlayerPrefs.GetInt(KEY+"_gold");
    }
}
