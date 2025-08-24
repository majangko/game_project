using UnityEngine;
using UI; // FadeTransition

public static class GameOverLoader
{
    static Texture2D lastShot;

    public static void ShowGameOver()
    {
        // 1) 현재 화면 캡처 (간단/빠름)
        lastShot = ScreenCapture.CaptureScreenshotAsTexture();
        // 2) 전환
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
