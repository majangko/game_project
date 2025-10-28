using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UseItemPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform itemListParent;  // ScrollView/Viewport/Content
    [SerializeField] private GameObject itemRowPrefab;  // 한 줄 프리팹
    [SerializeField] private Button btnUse;
    [SerializeField] private Button btnSkip;

    private System.Action onFinished;

    private void Awake()
    {
        Debug.Log($"[UseItemPopup] Awake (activeSelf={gameObject.activeSelf})");
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    private void OnEnable()  { Debug.Log("[UseItemPopup] OnEnable"); }
    private void OnDisable() { Debug.Log("[UseItemPopup] OnDisable"); }

    public void Open(System.Action afterClose)
    {
        Debug.Log("[UseItemPopup] Open() 진입");
        gameObject.SetActive(true);
        onFinished = afterClose;

        // 안전: 내부 CanvasGroup이 있으면 보이게
        var cg = GetComponentInChildren<CanvasGroup>(true);
        if (cg) { cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true; }

        // 버튼 이벤트 연결(중복 방지)
        if (btnUse)
        {
            btnUse.onClick.RemoveAllListeners();
            btnUse.onClick.AddListener(OnClickUse);
        }
        else Debug.LogWarning("[UseItemPopup] btnUse 미할당");

        if (btnSkip)
        {
            btnSkip.onClick.RemoveAllListeners();
            btnSkip.onClick.AddListener(OnClickSkip);
        }
        else Debug.LogWarning("[UseItemPopup] btnSkip 미할당");

        BuildItemList();
        Debug.Log("[UseItemPopup] Open() 완료 - 팝업 활성화됨");
    }

    private void BuildItemList()
    {
        if (!itemListParent || !itemRowPrefab)
        {
            Debug.LogWarning("[UseItemPopup] itemListParent/itemRowPrefab 미할당");
            return;
        }

        foreach (Transform c in itemListParent) Destroy(c.gameObject);

        var inv = PlayerInventory.Instance?.All();
        if (inv == null || inv.Count == 0)
        {
            var row = Instantiate(itemRowPrefab, itemListParent);
            var label = row.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label) label.text = "사용 가능한 아이템이 없습니다.";
            Debug.Log("[UseItemPopup] 인벤토리 비어있음 → 안내 한 줄 표시");
            return;
        }

        foreach (var kv in inv)
        {
            var row = Instantiate(itemRowPrefab, itemListParent);
            var label = row.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label) label.text = $"{kv.Key}  x{kv.Value}";
        }
    }

    private void OnClickUse()
    {
        Debug.Log("[UseItemPopup] 아이템 사용");
        // TODO: 실제 사용 로직 연결
        Close();
    }

    private void OnClickSkip()
    {
        Debug.Log("[UseItemPopup] 사용 안 함");
        Close();
    }

    public void Close()
    {
        Debug.Log("[UseItemPopup] Close()");
        gameObject.SetActive(false);
        onFinished?.Invoke();
    }
}
