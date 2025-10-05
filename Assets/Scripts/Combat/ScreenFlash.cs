using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance;
    private Image flashImage;
    private Coroutine flashRoutine;

    void Awake()
    {
        Instance = this;
        // Canvas 자동 생성 (없을 경우)
        Canvas canvas = FindObjectOfType<Canvas>();
        if (!canvas)
        {
            GameObject cObj = new GameObject("FlashCanvas");
            canvas = cObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        GameObject imgObj = new GameObject("FlashImage");
        imgObj.transform.SetParent(canvas.transform, false);
        flashImage = imgObj.AddComponent<Image>();
        flashImage.color = new Color(1, 0, 0, 0); // 초기 투명
        flashImage.rectTransform.anchorMin = Vector2.zero;
        flashImage.rectTransform.anchorMax = Vector2.one;
        flashImage.rectTransform.offsetMin = Vector2.zero;
        flashImage.rectTransform.offsetMax = Vector2.zero;
    }

    public void Flash(float duration = 0.2f)
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        float half = duration / 2f;

        // Fade in
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(0f, 0.4f, t / half);
            flashImage.color = new Color(1f, 0f, 0f, a);
            yield return null;
        }

        // Fade out
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(0.4f, 0f, t / half);
            flashImage.color = new Color(1f, 0f, 0f, a);
            yield return null;
        }

        flashImage.color = new Color(1, 0, 0, 0);
    }
}
