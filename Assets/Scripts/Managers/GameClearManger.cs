using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameClearSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup fadeGroup;
    public Text clearText;
    public Text pressAnyKeyText;

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioClip clearBGM;

    [Header("Animation Settings")]
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 1.2f;
    public float blinkDelay = 1f;

    [Header("Stage Info")]
    public bool isFinalStage = true; // ✅ 마지막 보스면 true

    private bool _isExiting = false;

    void Start()
    {
        Time.timeScale = 1f;

        // ✅ 안전하게 씬 로드 이벤트 재등록
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;

        StartCoroutine(ShowSequence());
    }

    IEnumerator ShowSequence()
    {
        // 1️⃣ BGM 재생
        if (bgmSource && clearBGM)
        {
            bgmSource.clip = clearBGM;
            bgmSource.Play();
        }

        // 2️⃣ 배경만 페이드인
        fadeGroup.alpha = 0f;
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0, 1, t / fadeInDuration);
            yield return null;
        }

        // 3️⃣ 텍스트 즉시 표시
        SetTextVisible(clearText, true);
        SetTextVisible(pressAnyKeyText, true);

        // 4️⃣ Press Any Key 깜빡임
        StartCoroutine(BlinkText(pressAnyKeyText));

        // 5️⃣ 입력 대기 후 처리
        yield return new WaitUntil(() => Input.anyKeyDown);
        if (!_isExiting)
        {
            _isExiting = true;
            StartCoroutine(FadeOutAndExit());
        }
    }

    IEnumerator FadeOutAndExit()
    {
        float t = 0f;
        float startVolume = (bgmSource != null) ? bgmSource.volume : 1f;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float progress = t / fadeOutDuration;
            fadeGroup.alpha = Mathf.Lerp(1, 0, progress);

            if (bgmSource)
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, progress);

            yield return null;
        }

        // ✅ 마지막 스테이지만 StartIsland로 이동
        if (isFinalStage)
        {
            SceneManager.LoadScene("StartIsland-1");
        }
        else
        {
            // ✅ 중간 스테이지 보스면 UI만 닫기
            Destroy(gameObject);
        }
    }

    IEnumerator BlinkText(Text txt)
    {
        Color c = txt.color;
        c.a = 1f;
        while (true)
        {
            for (float t = 0; t < 1f; t += Time.deltaTime)
            {
                c.a = Mathf.Lerp(0.3f, 1f, Mathf.PingPong(t * 2f, 1f));
                txt.color = c;
                yield return null;
            }
        }
    }

    void SetTextVisible(Text txt, bool visible)
    {
        if (txt == null) return;
        Color c = txt.color;
        c.a = visible ? 1f : 0f;
        txt.color = c;
    }
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameClearSceneManager] 씬 전환 감지됨 → {scene.name}");

        if (scene.name.StartsWith("StartIsland-1"))
        {
            Debug.Log("[GameClearSceneManager] StartIsland-1 진입 감지 → GameClearUI 제거 ✅");
            Destroy(gameObject);
        }
    }

}


