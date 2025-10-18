using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전역 사운드 매니저.
/// - 스킬, UI, 효과음 등 전반적인 SFX 관리
/// - Singleton 구조로 어디서든 접근 가능
/// - Resources/Sounds 폴더의 오디오 자동 로드 지원
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("효과음(SFX) 재생용 AudioSource")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [Tooltip("SFX 전체 볼륨")]
    public float sfxVolume = 1f;

    // 내부 사운드 캐시
    private readonly Dictionary<string, AudioClip> clipDict = new();

    private void Awake()
    {
        // ✅ 싱글톤 유지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ✅ AudioSource 자동 생성 (없을 경우)
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        // ✅ 자동 로드
        LoadAllClips();
    }

    /// <summary>
    /// Resources/Sounds 폴더의 모든 오디오 클립을 자동 로드합니다.
    /// </summary>
    public void LoadAllClips()
    {
        clipDict.Clear();
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds");
        foreach (AudioClip c in clips)
        {
            clipDict[c.name] = c;
            // Debug.Log($"[SoundManager] Loaded clip: {c.name}");
        }

        Debug.Log($"[SoundManager] Loaded {clipDict.Count} sound clips.");
    }

    /// <summary>
    /// 특정 AudioClip 재생 (즉시 1회)
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] AudioClip is null.");
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// 클립 이름으로 재생 (Resources/Sounds 폴더에서 자동 검색)
    /// </summary>
    public void PlaySFX(string clipName)
    {
        if (string.IsNullOrEmpty(clipName))
        {
            Debug.LogWarning("[SoundManager] Clip name is empty.");
            return;
        }

        if (clipDict.TryGetValue(clipName, out AudioClip clip))
        {
            PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] Clip '{clipName}' not found in Resources/Sounds.");
        }
    }

    /// <summary>
    /// 볼륨 조정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
