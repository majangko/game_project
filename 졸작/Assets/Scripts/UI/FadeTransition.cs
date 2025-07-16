using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeTransition : MonoBehaviour
{
    public Animator fadeAnimator;

    void Awake()
    {
        // 자동으로 Animator 연결
        if (fadeAnimator == null)
        {
            fadeAnimator = GetComponent<Animator>();
            if (fadeAnimator == null)
            {
                Debug.LogError("[FadeTransition] Animator를 찾을 수 없습니다.");
            }
        }
    }

    private void OnEnable()
    {
        if (fadeAnimator != null)
            fadeAnimator.SetTrigger("FadeIn");
    }

    public void FadeToScene(string sceneName)
    {
        if (fadeAnimator != null)
            StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        fadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
    }
}
