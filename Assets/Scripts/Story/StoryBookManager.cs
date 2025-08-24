using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

[System.Serializable]
public class StoryPage
{
    public Sprite background;            // 배경 이미지
    [TextArea(3, 10)] public string text; // 페이지 텍스트
}

public class StoryBookManager : MonoBehaviour
{
    [Header("SFX")]
    public AudioSource sfxSource;     // 2D 오디오소스
    public AudioClip sfxFlipForward;  // 다음 페이지
    public AudioClip sfxFlipBackward; // 이전 페이지
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    void PlayFlipSfx(int dir)
    {
        if (!sfxSource) return;
        var clip = dir > 0 ? (sfxFlipForward ?? sfxFlipBackward)
                           : (sfxFlipBackward ?? sfxFlipForward);
        if (!clip) return;

        sfxSource.pitch = Random.Range(0.97f, 1.03f);
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    [Header("Pages (순서대로)")]
    public StoryPage[] pages;

    [Header("UI Refs")]
    public Image bgA;    // 현재 배경
    public Image bgB;    // 와이프용 배경
    public TMP_Text storyText;
    public CanvasGroup pageGroup;
    public Button prevButton;
    public Button nextButton;
    public Button skipButton;

    [Header("Flow")]
    public string nextSceneName = "StartIsland";
    [Range(0.1f, 2f)] public float flipDuration = 0.55f;
    [Range(0f, 0.3f)] public float tilt = 0.07f; // 라디안

    int index = 0;
    bool busy = false;

    void Start()
    {
        if (prevButton) prevButton.onClick.AddListener(() => TryFlip(-1));
        if (nextButton) nextButton.onClick.AddListener(() => TryFlip(+1));
        if (skipButton) skipButton.onClick.AddListener(SkipToGame);

        index = Mathf.Clamp(index, 0, pages.Length - 1);
        ApplyInstant(index);
        UpdateNav();
    }

    void TryFlip(int dir)
    {
        if (busy || pages == null || pages.Length == 0) return;
        int target = index + dir;
        if (target < 0 || target >= pages.Length) return;
        PlayFlipSfx(dir);
        StartCoroutine(CoFlip(target, dir));
    }

    IEnumerator CoFlip(int targetIndex, int dir)
    {
        busy = true;

        // 다음 배경 준비
        bgB.sprite = pages[targetIndex].background;
        var cA = bgA.color; var cB = bgB.color;
        cA.a = 1f; bgA.color = cA;
        cB.a = 1f; bgB.color = cB;

        // 와이프 설정
        bgB.type = Image.Type.Filled;
        bgB.fillMethod = Image.FillMethod.Horizontal;
        bgB.fillOrigin = (dir > 0) ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left;
        bgB.fillAmount = 0f;

        float dur = Mathf.Max(0.05f, flipDuration);
        float outTime = dur * 0.45f;     // 텍스트 아웃
        float wipeTime = dur * 0.55f;    // 배경 전환
        var pr = pageGroup.GetComponent<RectTransform>();
        var startEuler = pr.localEulerAngles;
        var targetEuler = startEuler + new Vector3(0f, 0f, Mathf.Rad2Deg * (dir > 0 ? -tilt : tilt));
        var startPos = pr.anchoredPosition;
        var targetPos = startPos + new Vector2(dir > 0 ? 36f : -36f, 0f);

        // 1) 텍스트 아웃
        float t = 0f;
        while (t < outTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / outTime);
            pageGroup.alpha = Mathf.Lerp(1f, 0f, u);
            pr.anchoredPosition = Vector2.Lerp(startPos, targetPos, u);
            pr.localEulerAngles = Vector3.Lerp(startEuler, targetEuler, u);
            yield return null;
        }
        pageGroup.alpha = 0f;

        // 2) 배경 와이프
        t = 0f;
        while (t < wipeTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / wipeTime);
            bgB.fillAmount = Mathf.SmoothStep(0f, 1f, u);
            yield return null;
        }
        bgB.fillAmount = 1f;

        // 확정 전환
        bgA.sprite = bgB.sprite;
        bgB.fillAmount = 0f;
        cB.a = 0f; bgB.color = cB;

        // 3) 텍스트 인
        storyText.text = pages[targetIndex].text;
        t = 0f;
        var endEuler = startEuler;
        var endPos = startPos;
        while (t < outTime)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / outTime);
            pageGroup.alpha = Mathf.Lerp(0f, 1f, u);
            pr.anchoredPosition = Vector2.Lerp(targetPos, endPos, u);
            pr.localEulerAngles = Vector3.Lerp(targetEuler, endEuler, u);
            yield return null;
        }
        pageGroup.alpha = 1f;
        pr.anchoredPosition = endPos;
        pr.localEulerAngles = endEuler;

        index = targetIndex;
        UpdateNav();
        busy = false;
    }

    void ApplyInstant(int i)
    {
        if (pages == null || pages.Length == 0) return;
        bgA.sprite = pages[i].background;
        var ca = bgA.color; ca.a = 1f; bgA.color = ca;
        var cb = bgB.color; cb.a = 0f; bgB.color = cb;

        storyText.text = pages[i].text;
        pageGroup.alpha = 1f;
        var pr = pageGroup.GetComponent<RectTransform>();
        pr.localEulerAngles = Vector3.zero;
        pr.anchoredPosition = Vector2.zero;
    }

    void UpdateNav()
    {
        bool hasPrev = index > 0;
        bool hasNext = index < pages.Length - 1;

        if (prevButton != null) prevButton.gameObject.SetActive(hasPrev);
        if (nextButton != null) nextButton.gameObject.SetActive(hasNext);
    }

    void SkipToGame()
    {
        var target = string.IsNullOrEmpty(nextSceneName) ? "StartIsland" : nextSceneName;
        var fade = FadeTransition.Instance;
        if (fade != null)
        {
            fade.WipeToScene(target, 0.35f, 0.9f, 0.35f);
            return;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(target);
    }
}
