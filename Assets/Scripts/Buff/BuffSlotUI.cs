using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BuffSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image icon;          // 버프 아이콘
    public Image fillBar;       // 남은시간 게이지
    public TMP_Text timerText;  // 남은 시간 텍스트

    private float duration;
    private float remaining;
    private CanvasGroup canvasGroup;
    private bool fadingOut = false;

    void Awake()
    {
        // CanvasGroup은 투명도 제어용
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // 버프 초기화
    public void Init(BuffData data)
    {
        if (icon) icon.sprite = data.icon;
        duration = data.duration;
        remaining = duration;
        if (fillBar) fillBar.fillAmount = 1f;
        if (timerText) timerText.text = $"{remaining:F1}s";
        fadingOut = false;
        canvasGroup.alpha = 1f;
    }

    // 기존 버프 갱신 (버프 재적용 시)
    public void ResetTimer(float newDuration)
    {
        duration = newDuration;
        remaining = newDuration;
        fadingOut = false;
        canvasGroup.alpha = 1f;
    }

    void Update()
    {
        if (fadingOut) return;

        remaining -= Time.deltaTime;
        remaining = Mathf.Max(0f, remaining);

        // UI 갱신
        if (fillBar) fillBar.fillAmount = remaining / duration;
        if (timerText) timerText.text = $"{remaining:F1}s";

        // 종료 1초 전부터 서서히 사라지기 시작
        if (remaining <= 1f)
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * 3f);

        if (remaining <= 0f)
            FadeOutAndDestroy();
    }

    // 버프 자연스럽게 사라지기
    public void FadeOutAndDestroy()
    {
        if (!fadingOut)
            StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        fadingOut = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2.5f;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        Destroy(gameObject);
    }
}
