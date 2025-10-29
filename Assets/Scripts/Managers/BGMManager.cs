using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("BGM Clips (StartIsland-1 ~ Boss03)")]
    public AudioClip startIslandBGM;
    public AudioClip stage01BGM;
    public AudioClip boss01BGM;
    public AudioClip stage02BGM;
    public AudioClip boss02BGM;
    public AudioClip stage03BGM;
    public AudioClip boss03BGM;

    [Header("Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    public float fadeTime = 1.0f; // 씬 전환 시 페이드 시간(초)

    private AudioSource _source;
    private Coroutine _fadeRoutine;

    // 우리가 관리하는 씬 목록
    private static readonly string[] ManagedScenes =
    {
        "StartIsland-1", "Stage01", "Boss01",
        "Stage02", "Boss02",
        "Stage03", "Boss03"
    };

    // 외부에서 BGM이 이미 있는 씬 (Main, StoryIntro)
    private static readonly string[] IgnoreScenes =
    {
        "Main", "StoryIntro"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = true;
        _source.volume = 0f;

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 현재 씬이 StartIsland-1로 시작하면 바로 재생
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // Main, StoryIntro → 외부 BGM 사용 중이므로 정지
        foreach (string ignore in IgnoreScenes)
        {
            if (sceneName == ignore)
            {
                StopBGM();
                return;
            }
        }

        // 우리가 관리하는 씬이면 BGM 전환
        foreach (string managed in ManagedScenes)
        {
            if (sceneName == managed)
            {
                AudioClip clip = GetClipForScene(sceneName);
                if (clip != null)
                {
                    PlayBGM(clip);
                }
                else
                {
                    StopBGM();
                }
                return;
            }
        }

        // 그 외 씬(예: Stage04 이후)은 관리 안 함 → 정지
        StopBGM();
    }

    private AudioClip GetClipForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "StartIsland-1": return startIslandBGM;
            case "Stage01":       return stage01BGM;
            case "Boss01":        return boss01BGM;
            case "Stage02":       return stage02BGM;
            case "Boss02":        return boss02BGM;
            case "Stage03":       return stage03BGM;
            case "Boss03":        return boss03BGM;
            default:              return null;
        }
    }

    private void PlayBGM(AudioClip clip)
    {
        // 같은 클립이면 다시 재생하지 않음
        if (_source.clip == clip && _source.isPlaying) return;

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOutInAndPlay(clip));
    }

    private void StopBGM()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeOutInAndPlay(AudioClip newClip)
    {
        // Fade out
        float t = 0f;
        float startVol = _source.volume;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            _source.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        _source.Stop();
        _source.clip = newClip;
        _source.loop = true;
        _source.Play();

        // Fade in
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            _source.volume = Mathf.Lerp(0f, masterVolume, t / fadeTime);
            yield return null;
        }

        _source.volume = masterVolume;
        _fadeRoutine = null;
    }

    private IEnumerator FadeOutAndStop()
    {
        float t = 0f;
        float startVol = _source.volume;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            _source.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        _source.Stop();
        _source.clip = null;
        _fadeRoutine = null;
    }
}
