using UnityEngine;
using UnityEngine.Tilemaps;

public class DayNightAfterTime : MonoBehaviour
{
    [Header("전환 설정")]
    public float nightDelaySeconds = 30f; // 낮 → 밤 전환 대기(초)
    public float transitionSpeed = 1f;    // 색 전환 속도

    [Header("색상 설정")]
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.4f, 0.4f, 0.6f, 1f);

    [Header("대상")]
    public Tilemap[] tilemaps;              // 🔁 TilemapRenderer → Tilemap 로 변경
    public SpriteRenderer[] backgrounds;    // 배경 스프라이트

    private bool isNight = false;
    private float startTime;
    private Color targetColor;

    void Start()
    {
        startTime = Time.time;
        targetColor = dayColor;

        // 배열이 비어 있으면 씬에서 자동 수집 (선택적)
        if (tilemaps == null || tilemaps.Length == 0)
            tilemaps = FindObjectsOfType<Tilemap>();
    }

    void Update()
    {
        if (!isNight && Time.time - startTime >= nightDelaySeconds)
        {
            isNight = true;
            targetColor = nightColor;
        }

        // 타일맵 색 전환 (Tilemap.color)
        foreach (var tm in tilemaps)
            if (tm) tm.color = Color.Lerp(tm.color, targetColor, Time.deltaTime * transitionSpeed);

        // 배경 색 전환 (SpriteRenderer.color)
        foreach (var bg in backgrounds)
            if (bg) bg.color = Color.Lerp(bg.color, targetColor, Time.deltaTime * transitionSpeed);
    }
}
