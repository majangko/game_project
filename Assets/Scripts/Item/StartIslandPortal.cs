using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartIslandPortal : MonoBehaviour
{
    [Header("Scene Name Guard")]
    [SerializeField] private string startIslandSceneName = "StartIsland-1";
    [Tooltip("씬명이 정확히 일치할 때만 이동/팝업 입력을 받게 하려면 켭니다.")]
    [SerializeField] private bool requireSceneNameMatch = true;

    [Header("Debug Overrides")]
    [Tooltip("체크 시, 범위 밖/씬 불일치여도 이동 입력 및 팝업 테스트를 허용합니다.")]
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
    [SerializeField] private KeyCode openKey = KeyCode.UpArrow; // ↑
    [SerializeField] private bool alsoAllowWKey = true;          // W 키 허용

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
        // ─────────────────────────────────────────────────────────
        // ① 어떤 키든 눌리면 무조건 로그 (리턴 전에 둠)
        if (Input.anyKeyDown)
        {
            Debug.Log("[StartIslandPortal] anyKeyDown 감지");
        }
        // 전역 진단키
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
            Debug.Log("[StartIslandPortal] (F8) 강제 팝업 테스트 (씬/범위 무시)");
            TryOpenPopup(() => Debug.Log("[StartIslandPortal] (F8) 팝업 종료 콜백"), force: true);
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            Debug.Log($"[StartIslandPortal] (F9) scene='{SceneManager.GetActiveScene().name}', " +
                      $"requireSceneNameMatch={requireSceneNameMatch}, IsOnStartIslandScene={IsOnStartIslandScene}, " +
                      $"isPlayerInRange={isPlayerInRange}, allowEverywhereForDebug={allowEverywhereForDebug}");
        }
        // ─────────────────────────────────────────────────────────

        // ② 이동 입력 처리 조건
        bool pressed = Input.GetKeyDown(openKey) || (alsoAllowWKey && Input.GetKeyDown(KeyCode.W));
        if (!pressed) return;

        // 디버그 허용이면 조건 우회
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
        // 1) SceneFlow 경유
        if (!force && SceneFlow.Instance != null && SceneFlow.Instance.TryOpenUseItemPopup(afterClose))
        {
            Debug.Log("[StartIslandPortal] SceneFlow 경유로 UseItemPopup 오픈");
            return;
        }

        // 2) 인스펙터 프리팹
        if (useItemPopupPrefab != null)
        {
            var popup = InstantiateToCanvas(useItemPopupPrefab);
            popup.Open(afterClose);
            Debug.Log("[StartIslandPortal] Prefab 슬롯 경유로 UseItemPopup 오픈");
            return;
        }

        // 3) 씬에서 존재(비활성 포함) 찾기
        var existing = FindAnyObjectByType<UseItemPopup>(FindObjectsInactive.Include);
        if (existing != null)
        {
            existing.gameObject.SetActive(false); // Open에서 켜짐
            existing.Open(afterClose);
            Debug.Log("[StartIslandPortal] 씬 내 UseItemPopup 발견 → 오픈");
            return;
        }

        // 4) Resources 로드
        var loaded = Resources.Load<UseItemPopup>("UseItemPopup");
        if (loaded == null) loaded = Resources.Load<UseItemPopup>("UI/UseItemPopup");
        if (loaded != null)
        {
            var popup2 = InstantiateToCanvas(loaded);
            popup2.Open(afterClose);
            Debug.Log("[StartIslandPortal] Resources 로드로 UseItemPopup 오픈");
            return;
        }

        // 5) 실패: 이동만 진행
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
        popup.gameObject.SetActive(false); // Open에서 활성화
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
