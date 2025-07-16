using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // [Main ȭ�� ��ư��]
    public void NewGame()
    {
        PlayerPrefs.DeleteAll(); // ������ �ʱ�ȭ
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
            Debug.Log("����� �����Ͱ� �����ϴ�.");
        }
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    // [GameOver �� StartIsland]
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

    // [�������� �ڵ� ����]
    public void LoadNextStage()
    {
        int currentStage = PlayerPrefs.GetInt("LastStage", 1);
        currentStage++;
        PlayerPrefs.SetInt("LastStage", currentStage);

        if (currentStage <= 5)
        {
            SceneManager.LoadScene("Stage0" + currentStage); // ��: Stage02
        }
        else
        {
            SceneManager.LoadScene("LastBoss");
        }
    }

    // [���丮 ���� �� �ڵ� �̵�]
    public void OnStoryComplete()
    {
        PlayerPrefs.SetInt("LastStage", 1);
        SceneManager.LoadScene("StartIsland");
    }
}
