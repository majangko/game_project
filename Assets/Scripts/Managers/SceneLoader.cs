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
        SceneManager.sceneLoaded -= OnSceneLoaded;

        string name = scene.name;
        Debug.Log($"[SceneLoader] Scene Loaded: {name}");

        StartCoroutine(DelayedSceneSync(name));
    }

    private IEnumerator DelayedSceneSync(string name)
    {
        // 씬 내 객체 초기화 완료 대기
        yield return null;
        yield return new WaitForSeconds(0.1f);

        var party = PartyManager.Instance;
        var tag = TagManager.Instance ?? FindObjectOfType<TagManager>();

        // ✅ StartIsland로 돌아온 경우 → 파티 완전 초기화
        if (party != null && name.StartsWith("StartIsland-1"))
        {
            party.ClearParty();
            Debug.Log("<color=orange>[SceneLoader]</color> StartIsland 복귀 → 파티 초기화 완료 ✅");
            yield break;
        }

        // ✅ 전투 스테이지나 보스 씬이면 파티 유지한 채 동기화
        if (party != null && tag != null && (name.StartsWith("Stage") || name.StartsWith("Boss")))
        {
            bool keepExisting = true;
            party.AssignToTagManager(tag, keepExisting);
            Debug.Log($"<color=cyan>[SceneLoader]</color> {name} 진입 → 파티 동기화 완료 (keepExisting={keepExisting}) ✅");
        }
        else
        {
            Debug.LogWarning("[SceneLoader] PartyManager 또는 TagManager가 준비되지 않음 ❌");
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
