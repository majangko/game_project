using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SceneFlow : MonoBehaviour
{
    public static SceneFlow Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private UseItemPopup useItemPopupPrefab;

    private UseItemPopup popupInstance;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[SceneFlow] Awake (Ok)");
    }

    public bool TryOpenUseItemPopup(Action afterClose)
    {
        if (!useItemPopupPrefab)
        {
            Debug.LogError("[SceneFlow] useItemPopupPrefab == null (Inspector에 프리팹 연결 필요)");
            return false;
        }
        Debug.Log($"[SceneFlow] Prefab OK: {useItemPopupPrefab.name}");

        // 1) 기존(비활성 포함) 찾기
        if (!popupInstance)
        {
            popupInstance = FindAnyObjectByType<UseItemPopup>(FindObjectsInactive.Include);
            if (popupInstance)
                Debug.Log($"[SceneFlow] 재사용 대상 발견: {popupInstance.gameObject.name} (active={popupInstance.gameObject.activeInHierarchy})");
        }

        // 2) 없으면 생성
        if (!popupInstance)
        {
            var canvas = EnsureCanvas();
            Debug.Log($"[SceneFlow] Instantiate 시도 under Canvas='{canvas.gameObject.name}' scene='{canvas.gameObject.scene.name}'");
            var go = Instantiate(useItemPopupPrefab.gameObject, canvas.transform);
            go.name = "UseItemPopup(Runtime)";
            popupInstance = go.GetComponent<UseItemPopup>();
            Debug.Log($"[SceneFlow] Instantiate 완료 -> {go.name} (scene='{go.scene.name}', parent='{go.transform.parent?.name}')");

            if (!popupInstance)
            {
                Debug.LogError("[SceneFlow] Instantiate는 됐지만 UseItemPopup 컴포넌트가 없습니다. (프리팹에 UseItemPopup 붙이기)");
                return false;
            }
        }

        // 3) 현재 씬 Canvas로 재부착
        var currentCanvas = EnsureCanvas();
        if (popupInstance.transform.parent != currentCanvas.transform)
        {
            popupInstance.transform.SetParent(currentCanvas.transform, false);
            Debug.Log($"[SceneFlow] 재부착 완료 -> parent='{currentCanvas.name}'");
        }

        // 4) 보임/정렬 보장
        ForceVisible(popupInstance.gameObject);

        // 5) Open 호출(예외 로깅)
        Debug.Log("[SceneFlow] Open() 호출 직전");
        try
        {
            popupInstance.Open(afterClose);
            Debug.Log("[SceneFlow] UseItemPopup.Open 호출 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneFlow] Open() 중 예외: {e.GetType().Name}\n{e}");
            return false;
        }
        return true;
    }

    private Canvas EnsureCanvas()
    {
        var activeScene = SceneManager.GetActiveScene();
        Canvas target = null;

        foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c && c.gameObject.scene == activeScene) { target = c; break; }
        }

        if (!target)
        {
            var go = new GameObject("AutoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var cv = go.GetComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            SceneManager.MoveGameObjectToScene(go, activeScene);
            target = cv;
            Debug.Log($"[SceneFlow] AutoCanvas 생성 (scene='{activeScene.name}')");
        }

        if (!FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include))
        {
            var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            SceneManager.MoveGameObjectToScene(esGo, activeScene);
            Debug.Log("[SceneFlow] EventSystem 자동 생성");
        }

        return target;
    }

    // ⬇⬇⬇ 반드시 클래스 내부에 있어야 합니다 ⬇⬇⬇
    private void ForceVisible(GameObject go)
    {
        if (!go.activeSelf) go.SetActive(true);

        // 루트 및 자식 RectTransform을 '화면 안'으로 강제
        foreach (var rt in go.GetComponentsInChildren<RectTransform>(true))
        {
            if (rt.transform == go.transform || rt.GetComponent<Canvas>() || rt.name.ToLower().Contains("panel"))
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.anchoredPosition = Vector2.zero;
            }
            else
            {
                if (rt.parent != null && rt.parent is RectTransform)
                    rt.anchoredPosition = Vector2.zero;
            }
        }

        // 내부 Canvas 전부 최상단 + Raycaster 보장
        var innerCanvases = go.GetComponentsInChildren<Canvas>(true);
        foreach (var c in innerCanvases)
        {
            c.enabled = true;
            c.overrideSorting = true;
            c.sortingOrder = 5000;
            if (!c.TryGetComponent<GraphicRaycaster>(out _))
                c.gameObject.AddComponent<GraphicRaycaster>();
        }

        // CanvasGroup 있으면 보임/입력 보장
        var cg = go.GetComponentInChildren<CanvasGroup>(true);
        if (cg)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        // 스프라이트 없는 Image들에 내장 스프라이트 자동 지정 + 가시화
        var bgSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        bool hasAnyGraphic = false;
        foreach (var g in go.GetComponentsInChildren<Graphic>(true))
        {
            if (!g.enabled) g.enabled = true;
            if (g.color.a < 0.01f) g.color = new Color(g.color.r, g.color.g, g.color.b, 1f);
            hasAnyGraphic = true;
        }
        foreach (var img in go.GetComponentsInChildren<Image>(true))
        {
            if (img.sprite == null)
            {
                img.sprite = bgSprite;
                var c = img.color;
                if (c.a < 0.2f) img.color = new Color(0.1f, 0.1f, 0.1f, 0.65f);
            }
        }

        // 레이아웃 즉시 갱신
        foreach (var t in go.GetComponentsInChildren<RectTransform>(true))
            LayoutRebuilder.ForceRebuildLayoutImmediate(t);
    }
}
