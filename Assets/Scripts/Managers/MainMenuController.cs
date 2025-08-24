using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void OnNewGame()
    {
        System.Action go = () => GameManager.Instance.NewGame();
        if (MenuBGM.Instance) MenuBGM.Instance.FadeOutThen(go);
        else go();
    }

    public void OnContinueGame()
    {
        System.Action go = () => GameManager.Instance.ContinueGame();
        if (MenuBGM.Instance) MenuBGM.Instance.FadeOutThen(go);
        else go();
    }

    public void OnOpenSettings() => SceneLoader.Instance.LoadSettings();
}
