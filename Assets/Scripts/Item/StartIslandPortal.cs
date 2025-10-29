using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartIslandPortal : MonoBehaviour
{
    [Header("Scene Name Guard")]
    [SerializeField] private string startIslandSceneName = "StartIsland-1";
    [SerializeField] private bool requireSceneNameMatch = true;

    [Header("Debug Overrides")]
    [SerializeField] private bool allowEverywhereForDebug = false;

    [Header("Next Stage Settings")]
    [SerializeField] private int  nextStageIndex = 1;
    [SerializeField] private bool isBossStage = false;
    [SerializeField] private bool goToTeamSelect = false;

    [Header("Party Register")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Sprite portrait;

    [Header("Popup Prefab (fallback)")]
    [SerializeField] private UseItemPopup useItemPopupPrefab;

    [Header("Open Key")]
    [SerializeField] private KeyCode openKey = KeyCode.UpArrow;
    [SerializeField] private bool alsoAllowWKey = true;

    private bool isPlayerInRange = false;
    private Portal existingPortal;

    private bool IsOnStartIslandScene =>
        string.Equals(SceneManager.GetActiveScene().name, startIslandSceneName, System.StringComparison.Ordinal);

    private void Awake()
    {
        existingPortal = GetComponent<Portal>();
        if ((!requireSceneNameMatch || IsOnStartIslandScene) && existingPortal != null)
        {
            existingPortal.enabled = false;
            Debug.Log($"[StartIslandPortal] 기존 Portal 비활성화 (scene='{SceneManager.GetActiveScene().name}').");
        }
    }

    private void Update()
    {
        // 진단 키
        if (Input.GetKeyDown(KeyCode.F6))
        {
            var inv = PlayerInventory.Instance?.All();
            Debug.Log($"[StartIslandPortal] (F6) 인벤토리 보유? {(inv != null && inv.Count > 0 ? "YES" : "NO")}");
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            var inv = PlayerInventory.Instance?.All();
            if (inv == null || inv.Count == 0) Debug.Log("[StartIslandPortal] (F7) 인벤토리 비어있음");
            else foreach (var kv in inv) Debug.Log($"[INV] {kv.Key} x{kv.Value}");
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Debug.Log("[StartIslandPortal] (F8) 강제 팝업 테스트");
            TryOpenPopup(() => Debug.Log("[StartIslandPortal] (F8) 팝업 종료 콜백"), force: true);
        }

        bool pressed = Input.GetKeyDown(openKey) || (alsoAllowWKey && Input.GetKeyDown(KeyCode.W));
        if (!pressed) return;

        if (!allowEverywhereForDebug)
        {
            if (!isPlayerInRange) return;
            if (requireSceneNameMatch && !IsOnStartIslandScene) return;
        }

        Debug.Log("[StartIslandPortal] 이동 입력 감지 → 팝업 시도");
        var inv2 = PlayerInventory.Instance?.All();
        if (inv2 == null || inv2.Count == 0)
        {
            Debug.Log("[StartIslandPortal] 인벤토리 비어있음 → 팝업 생략하고 이동");
            DoTravel();
        }
        else
        {
            TryOpenPopup(DoTravel, force: false);
        }
    }

    private void TryOpenPopup(System.Action afterClose, bool force)
    {
        if (!force && SceneFlow.Instance != null && SceneFlow.Instance.TryOpenUseItemPopup(afterClose))
        {
            Debug.Log("[StartIslandPortal] SceneFlow 경유로 UseItemPopup 오픈");
            return;
        }

        if (useItemPopupPrefab != null)
        {
            var popup = InstantiateToCanvas(useItemPopupPrefab);
            popup.Open(afterClose);
            Debug.Log("[StartIslandPortal] Prefab 슬롯 경유로 UseItemPopup 오픈");
            return;
        }

        var existing = FindAnyObjectByType<UseItemPopup>(FindObjectsInactive.Include);
        if (existing != null)
        {
            existing.gameObject.SetActive(false);
            existing.Open(afterClose);
            Debug.Log("[StartIslandPortal] 씬 내 UseItemPopup 발견 → 오픈");
            return;
        }

        var loaded = Resources.Load<UseItemPopup>("UseItemPopup");
        if (loaded == null) loaded = Resources.Load<UseItemPopup>("UI/UseItemPopup");
        if (loaded != null)
        {
            var popup2 = InstantiateToCanvas(loaded);
            popup2.Open(afterClose);
            Debug.Log("[StartIslandPortal] Resources 로드로 UseItemPopup 오픈");
            return;
        }

        Debug.LogError("[StartIslandPortal] UseItemPopup을 찾지 못해 이동만 진행");
        afterClose?.Invoke();
    }

    private UseItemPopup InstantiateToCanvas(UseItemPopup prefab)
    {
        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var go = new GameObject("AutoCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas = c;
            Debug.Log("[StartIslandPortal] Canvas 없음 → AutoCanvas 생성");
        }
        var popup = Instantiate(prefab, canvas.transform);
        popup.gameObject.SetActive(false);
        return popup;
    }

    private void DoTravel()
    {
        RegisterPlayerParty();
        PlayerData.SavePlayerData();

        if (goToTeamSelect)
        {
            Debug.Log("[StartIslandPortal] 팀 선택 씬으로 이동");
            SceneManager.LoadScene("TeamSelect UI");
            return;
        }

        if (SceneLoader.Instance != null)
        {
            if (isBossStage) SceneLoader.Instance.LoadBoss(nextStageIndex);
            else             SceneLoader.Instance.LoadStage(nextStageIndex);
        }
        else
        {
            Debug.LogError("[StartIslandPortal] SceneLoader.Instance 없음!");
        }
    }

    private void RegisterPlayerParty()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogError("[StartIslandPortal] PartyManager 없음!");
            return;
        }

        var data = new PartyMemberData { id = "guma_test", portrait = portrait, prefab = playerPrefab };
        var members = PartyManager.Instance.GetAllMembers();
        bool already = members.Exists(m => m != null && m.id == data.id);
        if (!already) { PartyManager.Instance.AddMember(data); Debug.Log("[StartIslandPortal] guma_test 파티 등록"); }
        else          { Debug.Log("[StartIslandPortal] guma_test 이미 파티에 존재"); }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("[StartIslandPortal] Player 범위 진입");
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("[StartIslandPortal] Player 범위 이탈");
        }
    }
}
