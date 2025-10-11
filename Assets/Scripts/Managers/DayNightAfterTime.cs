using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;  // ✅ 씬 이름 확인용

public class DayNightAfterTime : MonoBehaviour
{
    [Header("전환 설정")]
    public float nightDelaySeconds = 30f; // 낮 → 밤 전환 대기(초)
    public float transitionSpeed = 1f;    // 색 전환 속도

    [Header("색상 설정")]
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.4f, 0.4f, 0.6f, 1f);

    [Header("대상")]
    public Tilemap[] tilemaps;              // Tilemap 컴포넌트
    public SpriteRenderer[] backgrounds;    // 배경 스프라이트

    private bool isNight = false;
    private float startTime;
    private Color targetColor;

    // ✅ 추가: 작동할 씬 이름들
    private readonly string[] targetScenes = 
        { "Stage01", "Stage02", "Stage03", "Stage04", "Stage05" };

    private bool canRun = false;

    void Start()
    {
        // 현재 씬 이름 확인
        string currentScene = SceneManager.GetActiveScene().name;

        // Stage01~05 중 해당되는 씬인지 검사
        foreach (string name in targetScenes)
        {
            if (currentScene == name)
            {
                canRun = true;
                break;
            }
        }

        // 해당 씬이 아니라면 비활성화
        if (!canRun)
        {
            enabled = false; // Update() 실행 안 됨
            return;
        }

        startTime = Time.time;
        targetColor = dayColor;

        // 배열이 비어 있으면 자동 수집
        if (tilemaps == null || tilemaps.Length == 0)
            tilemaps = FindObjectsOfType<Tilemap>();
    }

    void Update()
    {
        if (!canRun) return;

        if (!isNight && Time.time - startTime >= nightDelaySeconds)
        {
            isNight = true;
            targetColor = nightColor;
        }

        // 타일맵 색 전환
        foreach (var tm in tilemaps)
            if (tm) tm.color = Color.Lerp(tm.color, targetColor, Time.deltaTime * transitionSpeed);

        // 배경 색 전환
        foreach (var bg in backgrounds)
            if (bg) bg.color = Color.Lerp(bg.color, targetColor, Time.deltaTime * transitionSpeed);
    }
}
