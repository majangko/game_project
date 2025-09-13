using UnityEngine;

public class StoryIntroMusic : MonoBehaviour
{
    public AudioClip bgm;
    public float crossFade = 1.5f;
    public float volume = 0.8f;

    void Start()
    {
        if (bgm == null) return;
        if (MusicManager.Instance != null) MusicManager.Instance.CrossFade(bgm, crossFade, volume);
        else { var s = gameObject.AddComponent<AudioSource>(); s.clip = bgm; s.loop = true; s.Play(); }
    }
}
