using UnityEngine;
using UnityEngine.SceneManagement;

public class ClockHandController : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform clockHand;

    [Header("Duration")]
    [Tooltip("한 바퀴 회전(9시→3시)에 걸리는 시간 (분 단위)")]
    public float durationMinutes = 10f;

    [Header("Test Mode")]
    public bool testMode = false;
    public float testDurationSeconds = 10f;
    public bool forceActiveInTest = true;

    [Header("Stage 제한")]
    [Tooltip("Stage 씬에서만 시계 작동")]
    public bool onlyInStages = true;

    [Header("Art 보정")]
    [Tooltip("스프라이트 방향이 아래(↓)면 180, 오른쪽(→) 90, 왼쪽(←) -90")]
    public float zeroIsUpOffset = 0f;

    [Header("시작 각도 제어")]
    [Tooltip("시계 시작 각도 (기본값: 9시=90도)")]
    public float startFromDeg = 90f;
    [Tooltip("회전 종료 각도 (기본값: 3시=270도)")]
    public float endAtDeg = 270f;

    [Tooltip("회전 방향 반전")]
    public bool invertDirection = false;

    private float durSec;
    private float elapsed;
    private bool active;

    // ============================================================
    // UNITY LIFE CYCLE
    // ============================================================
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        InitializeClock();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름이 Stage로 시작할 때만 시계 리셋
        if (onlyInStages && !scene.name.StartsWith("Stage"))
            return;

        ResetClockHand();
        InitializeClock();
        Debug.Log($"[ClockHandController] Scene Loaded → {scene.name}, 시계 초기화 완료 ✅");
    }

    // ============================================================
    // CLOCK INITIALIZATION
    // ============================================================
    private void InitializeClock()
    {
        string scene = SceneManager.GetActiveScene().name;
        bool stageScene = scene.StartsWith("Stage");
        active = (testMode && forceActiveInTest) || (!onlyInStages || stageScene);

        durSec = testMode
            ? Mathf.Max(0.1f, testDurationSeconds)
            : Mathf.Max(1f, durationMinutes * 60f);

        elapsed = 0f;

        if (clockHand)
        {
            float startDeg = startFromDeg + zeroIsUpOffset;
            clockHand.anchoredPosition = Vector2.zero;
            clockHand.localEulerAngles = new Vector3(0, 0, startDeg);
        }
    }

    // ============================================================
    // UPDATE
    // ============================================================
    void Update()
    {
        if (!active || clockHand == null || Time.timeScale == 0f) return;
        if (elapsed >= durSec) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / durSec);

        // 9시(90)→3시(270) 시계방향 / invert면 반대
        float baseDeg = invertDirection
            ? Mathf.Lerp(endAtDeg, startFromDeg, t)
            : Mathf.Lerp(startFromDeg, endAtDeg, t);

        clockHand.localEulerAngles = new Vector3(0, 0, baseDeg + zeroIsUpOffset);
    }

    // ============================================================
    // PUBLIC API
    // ============================================================
    public void ResetClockHand()
    {
        elapsed = 0f;
        if (clockHand)
        {
            float startDeg = startFromDeg + zeroIsUpOffset;
            clockHand.localEulerAngles = new Vector3(0, 0, startDeg);
        }
        Debug.Log("[ClockHandController] ResetClockHand() 호출됨 → 9시로 초기화 🕘");
    }
}
