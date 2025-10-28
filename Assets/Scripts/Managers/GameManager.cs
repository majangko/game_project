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
        public int coins = 0;   // 메타 재화 (영구 저장 대상)
        public int level = 1;
        public int maxHP = 100;
        public int attack = 10;
    }

    // ====== 저장 정책 ======
    // - Meta : 오직 ManualSave() 또는 SaveAfterRunEnd() 때만 PlayerPrefs에 기록
    // - CurrentStage : 런타임 동안만 유지, 저장하지 않음
    // ======================

    // 영구 저장 대상
    public PlayerMeta Meta { get; private set; } = new PlayerMeta();

    // 런타임 진행 스테이지 (세이브 X)
    public int CurrentStage { get; private set; } = 1;

    // 하위 호환: 기존 코드에서 참조하던 소문자 이름
    public int currentStage => CurrentStage;

    // 통계성(영구 저장)
    public int FurthestClearedStage { get; private set; } = 0;

    public event Action OnMetaChanged;

    // PlayerPrefs 키
    const string KEY_PLAYER_META      = "PLAYER_META";
    const string KEY_FURTHEST_STAGE   = "FURTHEST_STAGE";

    [Header("Revive Cost")]
    public int reviveBaseCost = 100;
    public int reviveCostPerStage = 50;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadMeta();   // 저장된 메타만 로드
        ResetRun();   // 런은 항상 새로 시작
    }

    // ---------- 런타임 진행 ----------
    public void ResetRun() => CurrentStage = 1;

    public void AdvanceStage() => CurrentStage = Mathf.Clamp(CurrentStage + 1, 1, 5);

    // 레거시 호환: 진행 값만 갱신(세이브 X)
    public void SaveStageProgress(int stage)
    {
        CurrentStage = Mathf.Clamp(stage, 1, 5);
    }

    public void RegisterStageClearForStats(int stageIndex)
    {
        if (stageIndex > FurthestClearedStage)
        {
            FurthestClearedStage = stageIndex;
            // 통계는 저장 시점에 함께 기록됨(ManualSave/SaveAfterRunEnd)
        }
    }

    // 사망 시: 자동 저장 금지. 결과 처리/선택은 UI에서 결정.
    public void OnPlayerDeath(string cause = "")
    {
        Debug.Log($"[GameManager] Player Death Detected ({cause})");

        // 저장은 하지 않음 (플레이어 선택에 따라 부활/포기)
        // 단, 이후 UI/씬 전환 담당은 SceneLoader에 위임
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadGameOver();
        else
            SceneManager.LoadScene("GameOver");
    }


    // ---------- 메타 재화 ----------
    // 호환을 위해 gold 프로퍼티 유지(= Meta.coins 매핑). 자동 저장 없음.
    public int gold
    {
        get => Meta?.coins ?? 0;
        set
        {
            if (Meta == null) Meta = new PlayerMeta();
            Meta.coins = Mathf.Max(0, value);
            OnMetaChanged?.Invoke();
        }
    }

    public void AddCoins(int amount)
    {
        gold = Mathf.Max(0, gold + amount); // 저장은 호출자가 ManualSave/SaveAfterRunEnd로 명시
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold = gold - amount; // 저장은 호출자가 명시적으로
        return true;
    }

    public int GetReviveCost()
    {
        int stage = Mathf.Max(1, CurrentStage);
        return reviveBaseCost + reviveCostPerStage * (stage - 1);
    }

    // ---------- 저장/로드 ----------
    public bool HasSave => PlayerPrefs.HasKey(KEY_PLAYER_META);

    // 수동 저장(플레이어가 저장 버튼을 누를 때 호출)
    public void ManualSave()
    {
        SaveMetaInternal();
        Debug.Log("[Save] ManualSave completed.");
    }

    // 모험 종료 시 저장(클리어 또는 사망→그만하기 선택)
    public void SaveAfterRunEnd(bool cleared)
    {
        // 통계 갱신(클리어 루트에서만 최장 클리어 갱신을 원하면 cleared 조건으로 제한 가능)
        // 여기서는 예시로 현재 런 진행치를 기록하고 싶다면 필요시 호출부에서 전달
        SaveMetaInternal();

        // 런 리셋 (다음 시작은 항상 StartIsland에서)
        ResetRun();
        Debug.Log($"[Save] RunEnd saved. Cleared={cleared}");
    }

    void SaveMetaInternal()
    {
        var json = JsonUtility.ToJson(Meta);
        PlayerPrefs.SetString(KEY_PLAYER_META, json);
        PlayerPrefs.SetInt(KEY_FURTHEST_STAGE, FurthestClearedStage);
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

    // ---------- 메뉴 액션 ----------
    public void NewGame()
    {
        Meta = new PlayerMeta();
        FurthestClearedStage = 0;
        // 새 게임 시작 시점에는 저장하지 않음(원하면 시작 저장 버튼으로)
        ResetRun();

        PlayerInventory.Instance?.ResetAll();
        StatusEffectManager.Instance?.ClearRunEffects();
        
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadStoryIntro();
        else SceneManager.LoadScene("StoryIntro");
    }

    public void ContinueGame()
    {
        // 존재하는 메타만 로드해서 StartIsland로 복귀
        LoadMeta();
        ResetRun();

        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadStartIsland();
        else SceneManager.LoadScene("StartIsland-1");
    }
}
