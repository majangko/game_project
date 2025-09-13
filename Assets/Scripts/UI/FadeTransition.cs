using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace UI
{
    public class FadeTransition : MonoBehaviour
    {
        public static FadeTransition Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image wipeImage; // 풀스크린 검정 Image

        [Header("Timings (seconds)")]
        [SerializeField] private float outDuration = 0.7f;
        [SerializeField] private float holdBlack = 0.15f;
        [SerializeField] private float inDuration = 0.7f;

        [Header("Wipe Timings (seconds)")]
        [SerializeField] private float wipeOut = 0.35f;
        [SerializeField] private float wipeHold = 0.9f;
        [SerializeField] private float wipeIn = 0.35f;

        private bool isFading = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (!canvasGroup) canvasGroup = GetComponentInChildren<CanvasGroup>(true);
            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        public void FadeToScene(string sceneName)
        {
            if (!isFading) StartCoroutine(FadeAndLoad(sceneName, outDuration, holdBlack, inDuration));
        }

        public void FadeToScene(string sceneName, float outDur, float holdDur, float inDur)
        {
            if (!isFading) StartCoroutine(FadeAndLoad(sceneName, outDur, holdDur, inDur));
        }

        public void WipeToScene(string sceneName, float outDur, float holdDur, float inDur)
        {
            if (!isFading) StartCoroutine(WipeAndLoad(sceneName, outDur, holdDur, inDur));
        }

        private IEnumerator FadeAndLoad(string sceneName, float outDur, float holdDur, float inDur)
        {
            if (!canvasGroup) yield break;
            isFading = true;
            canvasGroup.blocksRaycasts = true;

            yield return StartCoroutine(Fade(0f, 1f, outDur));

            if (holdDur > 0f) yield return new WaitForSecondsRealtime(holdDur);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone) yield return null;

            yield return StartCoroutine(Fade(1f, 0f, inDur));

            canvasGroup.blocksRaycasts = false;
            isFading = false;
        }

        private IEnumerator WipeAndLoad(string sceneName, float outDur, float holdDur, float inDur)
        {
            if (!wipeImage) { FadeToScene(sceneName); yield break; }
            isFading = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            SetupWipe(Image.OriginHorizontal.Left);
            yield return StartCoroutine(Wipe(0f, 1f, outDur));
            if (holdDur > 0f) yield return new WaitForSecondsRealtime(holdDur);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone) yield return null;

            SetupWipe(Image.OriginHorizontal.Left);
            yield return StartCoroutine(Wipe(1f, 0f, inDur));

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            isFading = false;
        }

        private void SetupWipe(Image.OriginHorizontal origin)
        {
            if (!wipeImage) return;
            wipeImage.type = Image.Type.Filled;
            wipeImage.fillMethod = Image.FillMethod.Horizontal;
            wipeImage.fillOrigin = (int)origin;
        }

        private IEnumerator Wipe(float from, float to, float duration)
        {
            float t = 0f;
            wipeImage.fillAmount = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / duration);
                wipeImage.fillAmount = Mathf.SmoothStep(from, to, u);
                yield return null;
            }
            wipeImage.fillAmount = to;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            canvasGroup.alpha = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / duration);
                canvasGroup.alpha = Mathf.SmoothStep(from, to, u);
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}
