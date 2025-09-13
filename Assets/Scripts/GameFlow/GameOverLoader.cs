// Assets/Scripts/GameFlow/GameOverLoader.cs
using UnityEngine;
using UI;

public static class GameOverLoader
{
    static Texture2D lastShot;

    public static void ShowGameOver()
    {
        lastShot = ScreenCapture.CaptureScreenshotAsTexture();
        if (lastShot)
        {
            // ★ 런타임 텍스처가 씬/프리팹에 저장되지 않도록
            lastShot.hideFlags = HideFlags.HideAndDontSave;
        }

        var fade = FadeTransition.Instance;
        if (fade) fade.FadeToScene("GameOver");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
    }

    public static Texture2D ConsumeShot()
    {
        var t = lastShot; lastShot = null;
        return t;
    }
}
