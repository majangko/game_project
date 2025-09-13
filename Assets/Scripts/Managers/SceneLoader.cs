using UnityEngine;
using UnityEngine.SceneManagement;
using UI;

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
    }
}
