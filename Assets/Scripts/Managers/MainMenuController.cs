using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    // 새 게임 시작 (StoryIntro → StartIsland)
    public void OnNewGame()
    {
        System.Action go = () =>
        {
            if (GameManager.Instance != null)
                GameManager.Instance.NewGame();
            else
                Debug.LogError("[MainMenu] GameManager 인스턴스를 찾을 수 없습니다 ❌");
        };

        if (MenuBGM.Instance != null)
            MenuBGM.Instance.FadeOutThen(go);
        else
            go();

        Debug.Log("[MainMenu] 새 게임 시작 버튼 눌림 ✅");
    }

    // 이어하기 (StartIsland로 바로 이동)
    public void OnContinueGame()
    {
        System.Action go = () =>
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ContinueGame();
            else
                Debug.LogError("[MainMenu] GameManager 인스턴스를 찾을 수 없습니다 ❌");
        };

        if (MenuBGM.Instance != null)
            MenuBGM.Instance.FadeOutThen(go);
        else
            go();

        Debug.Log("[MainMenu] 이어하기 버튼 눌림 ✅");
    }

    // 설정창 열기
    public void OnOpenSettings()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadSettings();
            Debug.Log("[MainMenu] 설정창 이동");
        }
        else
        {
            Debug.LogWarning("[MainMenu] SceneLoader가 없음 ❌");
        }
    }
}
