using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MenuBGM : MonoBehaviour
{
    public static MenuBGM Instance { get; private set; }

    [Header("Clip & Volume")]
    public AudioClip clip;
    [Range(0f, 1f)] public float targetVolume = 0.7f;

    [Header("Fades (seconds)")]
    public float fadeIn = 0.8f;
    public float fadeOut = 0.6f;

    [Header("Scenes that keep playing")]
    public string[] menuScenes = { "Main", "Settings" };

    AudioSource src;
    bool isFading;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f;
        src.dopplerLevel = 0f;
        src.clip = clip;

        SceneManager.activeSceneChanged += OnSceneChanged;

        if (IsMenuScene(SceneManager.GetActiveScene().name) && clip)
            StartCoroutine(CoFadeIn());
    }

    void OnDestroy() => SceneManager.activeSceneChanged -= OnSceneChanged;

    bool IsMenuScene(string sceneName)
        => Array.IndexOf(menuScenes, sceneName) >= 0;

    void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        if (IsMenuScene(newScene.name))
        {
            if (clip && !src.isPlaying && !isFading) StartCoroutine(CoFadeIn());
        }
        else
        {
            if (!isFading) StartCoroutine(CoFadeOutAndDestroy());
        }
    }

    IEnumerator CoFadeIn()
    {
        isFading = true;
        src.volume = 0f;
        if (clip) src.Play();

        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(0f, targetVolume, t / fadeIn);
            yield return null;
        }
        src.volume = targetVolume;
        isFading = false;
    }

    public void FadeOutThen(Action onDone)
    {
        StartCoroutine(CoFadeOutThen(onDone));
    }

    IEnumerator CoFadeOutThen(Action onDone)
    {
        yield return CoFadeOutAndDestroy(false);
        onDone?.Invoke();
    }

    IEnumerator CoFadeOutAndDestroy(bool destroy = true)
    {
        isFading = true;
        float start = src.volume;
        float t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(start, 0f, t / fadeOut);
            yield return null;
        }
        src.Stop();
        isFading = false;

        if (destroy) Destroy(gameObject);
    }
}
