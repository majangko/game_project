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
    }

    public bool TryOpenUseItemPopup(Action afterClose)
    {
        if (!useItemPopupPrefab)
        {
            Debug.LogError("[SceneFlow] UseItemPopup Prefab 미할당");
            return false;
        }

        // 1) 기존 객체(비활성 포함) 재사용
        if (!popupInstance)
            popupInstance = FindAnyObjectByType<UseItemPopup>(FindObjectsInactive.Include);

        // 2) 없으면 생성
        if (!popupInstance)
        {
            var canvas = EnsureCanvas();
            var go = Instantiate(useItemPopupPrefab.gameObject, canvas.transform);
            go.name = "UseItemPopup(Runtime)";
            popupInstance = go.GetComponent<UseItemPopup>();
            if (!popupInstance)
            {
                Debug.LogError("[SceneFlow] 프리팹에 UseItemPopup 컴포넌트가 없습니다.");
                return false;
            }
        }

        // 3) 현재 씬 Canvas로만 재부착 (RectTransform, 이미지 등에는 손대지 않음)
        var currentCanvas = EnsureCanvas();
        if (popupInstance.transform.parent != currentCanvas.transform)
            popupInstance.transform.SetParent(currentCanvas.transform, false);

        // 4) 최상단 정렬만 보장 (레이아웃은 절대 건드리지 않음)
        foreach (var c in popupInstance.GetComponentsInChildren<Canvas>(true))
        {
            c.enabled = true;
            c.overrideSorting = true;
            c.sortingOrder = 5000;
            if (!c.TryGetComponent<GraphicRaycaster>(out _))
                c.gameObject.AddComponent<GraphicRaycaster>();
        }
        var cg = popupInstance.GetComponentInChildren<CanvasGroup>(true);
        if (cg) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }

        // 5) 오픈
        try
        {
            popupInstance.Open(afterClose);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SceneFlow] UseItemPopup.Open 예외: {e}");
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
        }

        if (!FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include))
        {
            var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            SceneManager.MoveGameObjectToScene(esGo, activeScene);
        }

        return target;
    }
}
