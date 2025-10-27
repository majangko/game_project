using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class PromptUIController : MonoBehaviour
{
    public TMP_Text label;          // Label(TMP_Text) 드래그 연결
    public float fadeDuration = 0.12f;

    CanvasGroup group;
    Coroutine fading;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (group != null) group.alpha = 0f; // 시작은 숨김
    }

    public void Show(string text)
    {
        if (label != null) label.text = text;
        gameObject.SetActive(true);
        if (fading != null) StopCoroutine(fading);
        fading = StartCoroutine(FadeTo(1f));
    }

    public void Hide()
    {
        if (fading != null) StopCoroutine(fading);
        fading = StartCoroutine(FadeOutAndDisable());
    }

    IEnumerator FadeTo(float target)
    {
        if (group == null) yield break;
        float start = group.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        group.alpha = target;
    }

    IEnumerator FadeOutAndDisable()
    {
        yield return FadeTo(0f);
    }
}
