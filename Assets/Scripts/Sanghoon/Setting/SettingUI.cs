using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;   // 전체 창 루트(=Setting Panel)
    public Button closeButton;
    public Button saveButton;

    [Header("Confirm Panel")]
    public GameObject confirmPanel;    // 확인 창(Inspector 체크 꺼둔 오브젝트)
    public Button yesButton;
    public Button noButton;

    [Header("Audio & Brightness")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Slider brightnessSlider;
    public Toggle bgmMuteToggle;
    public Toggle sfxMuteToggle;

    [Tooltip("전체 화면을 덮는 검은 Image(알파로 밝기 조절). 없으면 비워둬도 됨")]
    public Image brightnessOverlay;
    [Range(0, 1)] public float maxDarkAlpha = 0.6f;

    [Header("AudioMixer (선택)")]
    public AudioMixer mixer;                   // 있으면 연결
    public string bgmParam = "BGMVolume";      // 노출 파라미터명(dB)
    public string sfxParam = "SFXVolume";

    const string PREF_BGM = "p_bgm";
    const string PREF_SFX = "p_sfx";
    const string PREF_BGM_MUTE = "p_bgm_mute";
    const string PREF_SFX_MUTE = "p_sfx_mute";
    const string PREF_BRIGHT = "p_bright";

    void Awake()
    {
        // 버튼들
        closeButton.onClick.AddListener(() => settingsPanel.SetActive(false));
        saveButton.onClick.AddListener(() => confirmPanel.SetActive(true));
        yesButton.onClick.AddListener(SaveAndClose);
        noButton.onClick.AddListener(() => confirmPanel.SetActive(false));
        confirmPanel.SetActive(false);

        // 슬라이더/토글 이벤트
        bgmSlider.onValueChanged.AddListener(v => ApplyBGM());
        sfxSlider.onValueChanged.AddListener(v => ApplySFX());
        brightnessSlider.onValueChanged.AddListener(v => ApplyBrightness());
        bgmMuteToggle.onValueChanged.AddListener(v => ApplyBGM());
        sfxMuteToggle.onValueChanged.AddListener(v => ApplySFX());

        // 로드 & 적용
        LoadPrefs();
        ApplyAll();
    }

    void OnEnable() => ApplyAll();

    void LoadPrefs()
    {
        bgmSlider.value = PlayerPrefs.GetFloat(PREF_BGM, 0.8f);
        sfxSlider.value = PlayerPrefs.GetFloat(PREF_SFX, 0.8f);
        bgmMuteToggle.isOn = PlayerPrefs.GetInt(PREF_BGM_MUTE, 0) == 1;
        sfxMuteToggle.isOn = PlayerPrefs.GetInt(PREF_SFX_MUTE, 0) == 1;
        brightnessSlider.value = PlayerPrefs.GetFloat(PREF_BRIGHT, 0.0f);
    }

    void SaveAndClose()
    {
        PlayerPrefs.SetFloat(PREF_BGM, bgmSlider.value);
        PlayerPrefs.SetFloat(PREF_SFX, sfxSlider.value);
        PlayerPrefs.SetInt(PREF_BGM_MUTE, bgmMuteToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(PREF_SFX_MUTE, sfxMuteToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat(PREF_BRIGHT, brightnessSlider.value);

        // 키 바인딩 저장
        KeyBindingManager.Instance.SaveToPrefs();

        PlayerPrefs.Save();
        confirmPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    void ApplyAll()
    {
        ApplyBGM();
        ApplySFX();
        ApplyBrightness();
    }

    // ====== Audio ======
    void ApplyBGM()
    {
        float v = bgmMuteToggle.isOn ? 0f : bgmSlider.value;
        SetVolume(bgmParam, v);
        SetSliderVisual(bgmSlider, bgmMuteToggle.isOn);
    }

    void ApplySFX()
    {
        float v = sfxMuteToggle.isOn ? 0f : sfxSlider.value;
        SetVolume(sfxParam, v);
        SetSliderVisual(sfxSlider, sfxMuteToggle.isOn);
    }

    void SetVolume(string exposedParam, float linear01)
    {
        if (mixer == null) return;

        // 0~1을 dB로 (-80~0 부근)
        float dB = Mathf.Log10(Mathf.Clamp(linear01, 0.0001f, 1f)) * 20f;
        mixer.SetFloat(exposedParam, dB);
    }

    void SetSliderVisual(Slider s, bool muted)
    {
        // 뮤트 시 회색 처리 + 비활성화 하고 싶으면 아래 주석 해제
        var fill = s.fillRect ? s.fillRect.GetComponent<Image>() : null;
        var handle = s.handleRect ? s.handleRect.GetComponent<Image>() : null;
        if (fill) fill.color = muted ? Color.gray : Color.white;
        if (handle) handle.color = muted ? Color.gray : Color.white;
        s.interactable = !muted;
    }

    // ====== Brightness ======
    void ApplyBrightness()
    {
        if (!brightnessOverlay) return;
        var c = brightnessOverlay.color;
        c.a = Mathf.Clamp01(brightnessSlider.value) * maxDarkAlpha;
        brightnessOverlay.color = c;
    }
}
