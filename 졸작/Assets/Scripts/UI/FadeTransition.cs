using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeTransition : MonoBehaviour
{
    public Animator fadeAnimator;

    void Awake()
    {
        // �ڵ����� Animator ����
        if (fadeAnimator == null)
        {
            fadeAnimator = GetComponent<Animator>();
            if (fadeAnimator == null)
            {
                Debug.LogError("[FadeTransition] Animator�� ã�� �� �����ϴ�.");
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
