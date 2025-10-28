using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// HUDRoot에 붙여서, 씬 이름에 따라 지정한 오브젝트들만 보여주기
public class HUDSceneLayoutRefs : MonoBehaviour
{
    [Header("Scene Names")]
    public string startIsland = "StartIsland";
    public string[] stageScenes = { "Stage01", "Stage02", "Stage03" };
    public string[] bossScenes  = { "Boss01", "Boss02", "Boss03" };

    [Header("StartIsland에서 보일 것")]
    public GameObject[] startIslandObjects;

    [Header("Stage01~03에서 보일 것")]
    public GameObject[] stageObjects;

    [Header("Boss01~03에서 보일 것")]
    public GameObject[] bossObjects;

    // 내부 캐시: 모든 등록 오브젝트의 합집합
    List<GameObject> _all = new List<GameObject>();

    void Awake()
    {
        void AddRange(GameObject[] arr)
        {
            if (arr == null) return;
            foreach (var go in arr)
            {
                if (go == null) continue;
                // ✅ SkillTooltipUI는 목록 제외
                if (go.name.Contains("SkillTooltipUI")) continue;
                if (!_all.Contains(go)) _all.Add(go);
            }
        }
        AddRange(startIslandObjects);
        AddRange(stageObjects);
        AddRange(bossObjects);
    }


    void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void Start()
    {
        ApplyForScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        ApplyForScene(s.name);
    }

    void ApplyForScene(string sceneName)
    {
        // 어떤 세트인지 판정
        bool isStart = sceneName == startIsland;
        bool isStage = Contains(stageScenes, sceneName);
        bool isBoss  = Contains(bossScenes, sceneName);

        // 기본: 전부 숨김
        foreach (var go in _all)
            if (go) go.SetActive(false);

        // 해당 세트만 표시
        if (isStart) Toggle(startIslandObjects, true);
        else if (isStage) Toggle(stageObjects, true);
        else if (isBoss) Toggle(bossObjects, true);
        // 그 외 씬(Main, StoryIntro, GameResult, GameOver…): 전부 숨김
    }

    void Toggle(GameObject[] arr, bool on)
    {
        if (arr == null) return;
        foreach (var go in arr)
            if (go) go.SetActive(on);
    }

    bool Contains(string[] arr, string name)
    {
        if (arr == null) return false;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] == name) return true;
        return false;
    }
}
