// Assets/Scripts/Managers/GameManager.cs
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Serializable]
    public class PlayerMeta
    {
        public int coins = 0;
        public int level = 1;
        public int maxHP = 100;
        public int attack = 10;
    }

    // 메타(영구 저장 대상)
    public PlayerMeta Meta { get; private set; } = new PlayerMeta();

    // 런타임 진행 스테이지(저장하지 않음: 정책 반영)
    public int CurrentStage { get; private set; } = 1;

    // 하위 호환용(기존 스크립트에서 쓰던 이름)
    public int currentStage => CurrentStage;

    // 통계: 가장 멀리 깬 스테이지(영구 저장)
    public int FurthestClearedStage { get; private set; } = 0;

    public event Action OnMetaChanged;

    // PlayerPrefs 키
    const string KEY_PLAYER_META = "PLAYER_META";
    const string KEY_FURTHEST_STAGE = "FURTHEST_STAGE";
    // 레거시 Continue 체크 호환 플래그(중간세이브 금지 정책하에 단지 "세이브 존재" 플래그용)
    const string KEY_LAST_STAGE = "LastStage";
    const string KEY_HAS_SAVE = "HAS_SAVE";

    [Header("Revive Cost")]
    public int reviveBaseCost = 100;
    public int reviveCostPerStage = 50;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadMeta();
        ResetRun();
    }

    // 런(진행) 제어 ------------------------------------------------------------

    public void ResetRun() => CurrentStage = 1;

    public void AdvanceStage() => CurrentStage = Mathf.Clamp(CurrentStage + 1, 1, 5);

    // 레거시 호환: 보상/진행 스크립트가 부르는 메서드
    // 중간 저장은 하지 않고, "현재 런의 진행 단계"만 갱신
    public void SaveStageProgress(int stage)
    {
        CurrentStage = Mathf.Clamp(stage, 1, 5);
        // Continue 버튼이 PlayerPrefs 키만 확인하던 레거시 호환(의미값은 1로 고정)
        PlayerPrefs.SetInt(KEY_LAST_STAGE, 1);
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.Save();
    }

    public void RegisterStageClearForStats(int stageIndex)
    {
        if (stageIndex > FurthestClearedStage)
        {
            FurthestClearedStage = stageIndex;
            PlayerPrefs.SetInt(KEY_FURTHEST_STAGE, FurthestClearedStage);
        }
    }

    public void OnPlayerDeath(string cause = "")
    {
        SaveMeta();  // 메타만 저장
        ResetRun();
    }

    // 골드(코인) --------------------------------------------------------------

    // 레거시 호환을 위해 gold 프로퍼티를 Meta.coins에 매핑
    public int gold
    {
        get => Meta?.coins ?? 0;
        set
        {
            if (Meta == null) Meta = new PlayerMeta();
            Meta.coins = Mathf.Max(0, value);
            SaveMeta();
            OnMetaChanged?.Invoke();
        }
    }

    public void AddCoins(int amount)
    {
        gold = Mathf.Max(0, gold + amount); // setter가 SaveMeta + 이벤트 호출
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold = gold - amount; // setter 통해 저장/이벤트
        return true;
    }

    public int GetReviveCost()
    {
        int stage = Mathf.Max(1, CurrentStage);
        return reviveBaseCost + reviveCostPerStage * (stage - 1);
    }

    // 저장/로드 ---------------------------------------------------------------

    public void SaveMeta()
    {
        var json = JsonUtility.ToJson(Meta);
        PlayerPrefs.SetString(KEY_PLAYER_META, json);
        PlayerPrefs.SetInt(KEY_FURTHEST_STAGE, FurthestClearedStage);
        // 세이브 존재 플래그(레거시 Continue 호환)
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.SetInt(KEY_LAST_STAGE, 1);
        PlayerPrefs.Save();
    }

    public void LoadMeta()
    {
        if (PlayerPrefs.HasKey(KEY_PLAYER_META))
        {
            var json = PlayerPrefs.GetString(KEY_PLAYER_META);
            Meta = JsonUtility.FromJson<PlayerMeta>(json);
        }
        else
        {
            Meta = new PlayerMeta();
        }

        FurthestClearedStage = PlayerPrefs.GetInt(KEY_FURTHEST_STAGE, 0);
    }

    // 메뉴 액션 ---------------------------------------------------------------

    public void NewGame()
    {
        Meta = new PlayerMeta();
        FurthestClearedStage = 0;
        SaveMeta();
        ResetRun();

        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadStoryIntro();
        else SceneManager.LoadScene("StoryIntro");
    }

    public void ContinueGame()
    {
        LoadMeta();
        ResetRun();

        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadStartIsland();
        else SceneManager.LoadScene("StartIsland");
    }
}
