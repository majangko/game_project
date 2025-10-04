using UnityEngine;
using UnityEngine.SceneManagement;

public class ClockHandController : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform clockHand;

    [Header("Duration")]
    public float durationMinutes = 10f;

    [Header("Test")]
    public bool testMode = false;
    public float testDurationSeconds = 10f;
    public bool forceActiveInTest = true;

    [Header("Stage 제한")]
    public bool onlyInStages = true;

    [Header("Art 보정")]
    [Tooltip("스프라이트가 아래(↓)면 180, 오른쪽(→) 90, 왼쪽(←) -90")]
    public float zeroIsUpOffset = 0f;

    [Header("시작 각도 제어")]
    [Tooltip("체크 해제 시, 인스펙터에 세팅된 Z 각도를 그대로 시작 각도로 사용")]
    public bool OverrideStartAngle = true;
    [Tooltip("OverrideStartAngle이 켜졌을 때 사용할 시작 각도(단위: 도)")]
    public float startFromDeg = 90f; // 9시=90, 6시=180, 3시=270

    [Tooltip("회전 방향 반전")]
    public bool invertDirection = false;

    float durSec, elapsed;
    bool active;

    void Start()
    {
        string scene = SceneManager.GetActiveScene().name;
        bool stageScene = scene.StartsWith("Stage");
        active = (testMode && forceActiveInTest) || (!onlyInStages || stageScene);

        durSec = testMode ? Mathf.Max(0.1f, testDurationSeconds)
                          : Mathf.Max(1f, durationMinutes * 60f);

        if (clockHand)
        {
            // ★ 시작 각도 세팅 방식을 선택
            float startDeg = OverrideStartAngle
                ? (startFromDeg + zeroIsUpOffset)        // 스크립트가 강제로 설정
                : clockHand.localEulerAngles.z;          // 인스펙터 값 유지

            // AnchoredPosition은 중심 정렬만 보정
            clockHand.anchoredPosition = Vector2.zero;
            clockHand.localEulerAngles = new Vector3(0, 0, startDeg);
        }
    }

    void Update()
    {
        if (!active || clockHand == null || Time.timeScale == 0f) return;
        if (elapsed >= durSec) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / durSec);

        // 9시(90)→3시(270) 시계방향 / invert면 반대
        float baseDeg = invertDirection
            ? Mathf.Lerp(270f, 90f, t)
            : Mathf.Lerp( 90f, 270f, t);

        // 진행 각도 = 시작각 무시하고 '절대값'로 그려야 한다면:
        //   clockHand.localEulerAngles = new Vector3(0,0, baseDeg + zeroIsUpOffset);
        // 시작각을 '기준'으로 상대 회전을 쓰고 싶다면 아래처럼 변경:
        //   float startZ = OverrideStartAngle ? (startFromDeg + zeroIsUpOffset)
        //                                     : initialZCaptured;
        //   clockHand.localEulerAngles = new Vector3(0,0, Mathf.Lerp(startZ, startZ + 180f, t));

        clockHand.localEulerAngles = new Vector3(0, 0, baseDeg + zeroIsUpOffset);
    }
}
