using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // [Main 화면 버튼용]
    public void NewGame()
    {
        PlayerPrefs.DeleteAll(); // 데이터 초기화
        SceneManager.LoadScene("StoryIntro");
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("LastStage"))
        {
            SceneManager.LoadScene("StartIsland");
        }
        else
        {
            Debug.Log("저장된 데이터가 없습니다.");
        }
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    // [GameOver → StartIsland]
    public void GoToStartIsland()
    {
        SceneManager.LoadScene("StartIsland");
    }

    public void GoToGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void GoToGameOverResult()
    {
        SceneManager.LoadScene("GameOverResult");
    }

    public void GoToGameClear()
    {
        SceneManager.LoadScene("GameClear");
    }

    // [스테이지 자동 진행]
    public void LoadNextStage()
    {
        int currentStage = PlayerPrefs.GetInt("LastStage", 1);
        currentStage++;
        PlayerPrefs.SetInt("LastStage", currentStage);

        if (currentStage <= 5)
        {
            SceneManager.LoadScene("Stage0" + currentStage); // 예: Stage02
        }
        else
        {
            SceneManager.LoadScene("LastBoss");
        }
    }

    // [스토리 종료 후 자동 이동]
    public void OnStoryComplete()
    {
        PlayerPrefs.SetInt("LastStage", 1);
        SceneManager.LoadScene("StartIsland");
    }
}
