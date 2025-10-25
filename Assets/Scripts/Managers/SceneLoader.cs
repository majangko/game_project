using UnityEngine;
using UnityEngine.SceneManagement;
using UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadStoryIntro() => Load("StoryIntro");
    public void LoadStartIsland() => Load("StartIsland");
    public void LoadSettings() => Load("Settings");
    public void LoadGameOver() => Load("GameOver");
    public void LoadGameOverResult() => Load("GameOverResult");
    public void LoadGameClear() => Load("GameClear");

    public void LoadStage(int stageIndex)
    {
        stageIndex = Mathf.Clamp(stageIndex, 1, 5);
        Load($"Stage0{stageIndex}");
    }

    public void LoadBoss(int stageIndex)
    {
        stageIndex = Mathf.Clamp(stageIndex, 1, 5);
        if (stageIndex == 5) Load("LastBoss");
        else Load($"Boss0{stageIndex}");
    }

    public void LoadRewardEvent(int stageIndex)
    {
        stageIndex = Mathf.Clamp(stageIndex, 1, 5);
        string name = $"RewardEvent0{stageIndex}";
        if (Application.CanStreamedLevelBeLoaded(name)) Load(name);
        else LoadNextStage();
    }

    public void LoadNextStage()
    {
        GameManager.Instance.AdvanceStage();
        LoadStage(GameManager.Instance.CurrentStage);
    }

    private void Load(string sceneName)
    {
        var fade = FadeTransition.Instance;
        if (fade != null) fade.FadeToScene(sceneName);
        else SceneManager.LoadScene(sceneName);

        // ✅ 씬 로드 후 자동 동기화
        SceneManager.sceneLoaded -= OnSceneLoaded; // 중복 방지
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // -------------------- 전투씬 진입 후 파티/태그 동기화 --------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 한 번만 실행

        string name = scene.name;
        if (name.StartsWith("Stage") || name.StartsWith("Boss"))
        {
            var party = PartyManager.Instance;
            var tag = TagManager.Instance ?? FindObjectOfType<TagManager>();

            if (party != null && tag != null)
            {
                party.AssignToTagManager(tag, keepExisting: false);
                Debug.Log($"[SceneLoader] {name} 진입 → 파티 동기화 완료 ✅");
            }
        }
    }
}
