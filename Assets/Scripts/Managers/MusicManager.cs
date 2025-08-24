using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Mixer (optional)")]
    public AudioMixerGroup musicGroup;   // 나중에 옵션 메뉴서 볼륨 제어용 (없어도 OK)

    AudioSource a, b, current, next;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);

        a = gameObject.AddComponent<AudioSource>();
        b = gameObject.AddComponent<AudioSource>();
        foreach (var s in new[] { a, b })
        {
            s.loop = true; s.playOnAwake = false;
            s.spatialBlend = 0f;           // 2D
            s.outputAudioMixerGroup = musicGroup;
        }
        current = a; next = b;
    }

    public void PlayImmediate(AudioClip clip, float volume = 1f)
    {
        if (!clip) return;
        current.Stop(); current.clip = clip; current.volume = volume; current.Play();
    }

    public void CrossFade(AudioClip clip, float fade = 1.5f, float targetVolume = 1f)
    {
        if (!clip) return;
        if (current.clip == clip && current.isPlaying) return;

        next.clip = clip; next.volume = 0f; next.Play();
        StopAllCoroutines();
        StartCoroutine(CoCrossFade(fade, targetVolume));
    }

    IEnumerator CoCrossFade(float dur, float targetVol)
    {
        float start = current.volume, t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / dur);
            current.volume = Mathf.Lerp(start, 0f, u);
            next.volume    = Mathf.Lerp(0f, targetVol, u);
            yield return null;
        }
        current.Stop();
        (current, next) = (next, current);
    }

    public void Stop(float fade = 0.8f)
    {
        StopAllCoroutines();
        StartCoroutine(CoStop(fade));
    }
    IEnumerator CoStop(float dur)
    {
        float start = current.volume, t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            current.volume = Mathf.Lerp(start, 0f, t / dur);
            yield return null;
        }
        current.Stop();
    }
}
