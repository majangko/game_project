#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class CreateStoryIntroPrefab
{
    const string RootFolder = "Assets/UI/StoryIntro";
    const string PrefabPath = RootFolder + "/StoryIntroRoot.prefab";

    [MenuItem("Tools/StoryIntro/Create Book Intro Prefab")]
    public static void CreatePrefab()
    {
        if (!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets", "UI");
        if (!AssetDatabase.IsValidFolder(RootFolder)) AssetDatabase.CreateFolder("Assets/UI", "StoryIntro");

        var root = new GameObject("StoryIntroRoot");

        var canvasGO = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(root.transform);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var bgA = CreateStretchImage("BgA", canvasGO.transform, Color.white);
        var bgAImg = bgA.GetComponent<Image>(); bgAImg.preserveAspect = true;

        var bgB = CreateStretchImage("BgB", canvasGO.transform, new Color(1,1,1,0f));
        var bgBImg = bgB.GetComponent<Image>(); bgBImg.preserveAspect = true;

        var pageGroup = new GameObject("PageGroup", typeof(RectTransform), typeof(CanvasGroup));
        pageGroup.transform.SetParent(canvasGO.transform);
        var pr = pageGroup.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(1200f, 520f);
        pr.anchoredPosition = Vector2.zero;

        var textGO = new GameObject("StoryText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(pageGroup.transform);
        var tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0f, 0f);
        tr.anchorMax = new Vector2(1f, 1f);
        tr.offsetMin = new Vector2(32f, 32f);
        tr.offsetMax = new Vector2(-32f, -32f);

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "여기에 스토리 문장을 입력하세요.";
        tmp.fontSize = 40;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.color = new Color32(0xEE, 0xEE, 0xEE, 0xFF);

        var prev = CreateButton(canvasGO.transform, "PrevButton", "◀ 이전");
        var prevRect = prev.GetComponent<RectTransform>();
        prevRect.anchorMin = prevRect.anchorMax = new Vector2(0f, 0f);
        prevRect.anchoredPosition = new Vector2(40f, 40f);
        prevRect.sizeDelta = new Vector2(160f, 64f);

        var next = CreateButton(canvasGO.transform, "NextButton", "다음 ▶");
        var nextRect = next.GetComponent<RectTransform>();
        nextRect.anchorMin = nextRect.anchorMax = new Vector2(1f, 0f);
        nextRect.anchoredPosition = new Vector2(-40f, 40f);
        nextRect.sizeDelta = new Vector2(160f, 64f);

        var skip = CreateButton(canvasGO.transform, "SkipButton", "건너뛰기");
        var skipRect = skip.GetComponent<RectTransform>();
        skipRect.anchorMin = skipRect.anchorMax = new Vector2(1f, 1f);
        skipRect.anchoredPosition = new Vector2(-24f, -24f);
        skipRect.sizeDelta = new Vector2(160f, 56f);

        var mgrGO = new GameObject("StoryBookManager", typeof(StoryBookManager));
        mgrGO.transform.SetParent(root.transform);
        var mgr = mgrGO.GetComponent<StoryBookManager>();
        mgr.bgA = bgAImg; mgr.bgB = bgBImg; mgr.storyText = tmp;
        mgr.pageGroup = pageGroup.GetComponent<CanvasGroup>();
        mgr.prevButton = prev.GetComponent<Button>();
        mgr.nextButton = next.GetComponent<Button>();
        mgr.skipButton = skip.GetComponent<Button>();
        mgr.nextSceneName = "StartIsland";
        mgr.flipDuration = 0.5f; mgr.tilt = 0.07f;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        GameObject.DestroyImmediate(root);

        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", "StoryIntro 프리팹 생성됨:\n" + PrefabPath, "확인");
    }

    static GameObject CreateStretchImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>(); img.color = color;
        return go;
    }

    static GameObject CreateButton(Transform parent, string name, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent);
        var img = go.GetComponent<Image>(); img.color = new Color32(0x22,0x22,0x22,0xFF);

        var textGO = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(go.transform);
        var tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0f, 0f); tr.anchorMax = new Vector2(1f, 1f);
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.alignment = TextAlignmentOptions.Center; tmp.fontSize = 30;
        tmp.color = new Color32(0xEE,0xEE,0xEE,0xFF);

        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = img.color;
        colors.highlightedColor = new Color32(0x33,0x33,0x33,0xFF);
        colors.pressedColor = new Color32(0x12,0x12,0x12,0xFF);
        colors.disabledColor = new Color32(0x55,0x55,0x55,0xFF);
        colors.fadeDuration = 0.15f;
        btn.colors = colors;

        return go;
    }
}
#endif
