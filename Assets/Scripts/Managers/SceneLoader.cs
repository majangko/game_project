using UnityEngine;
using UnityEngine.SceneManagement;
using UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadStoryIntro() => Load("StoryIntro");
    public void LoadStartIsland() => Load("StartIsland-1");
    public void LoadSettings() => Load("Settings");
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
                bool keepExisting = name.StartsWith("Boss");
                // 🎯 보스 씬에서는 파티 유지 (TeamSelect 후 복귀 시 중복 방지)
                // 일반 스테이지에서는 완전 새로 세팅

                party.AssignToTagManager(tag, keepExisting);
                Debug.Log($"[SceneLoader] {name} 진입 → 파티 동기화 완료 ✅ (keepExisting={keepExisting})");
            }
        }
    }

    public void LoadTeamSelect() => Load("TeamSelect UI");

    public void LoadGameOver()
    {
        if (UI.FadeTransition.Instance != null)
            UI.FadeTransition.Instance.FadeToScene("GameOver");
        else
            SceneManager.LoadScene("GameOver");
    }

}
