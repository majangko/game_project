using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string shopSceneName = "Shop";

    [Header("Data")]
    [SerializeField] private ShopInventorySO shopInventory;

    [Header("Header UI")]
    [SerializeField] private TMP_Text txtShopTitle;
    [SerializeField] private TMP_Text txtPlayerGold;
    [SerializeField] private Button btnClose;

    [Header("Left List (Items)")]
    [SerializeField] private Transform itemListParent;   // ScrollView/Viewport/Content
    [SerializeField] private GameObject itemSlotPrefab;  // (root Button 권장)

    [Header("Right Detail")]
    [SerializeField] private Image selectedItemImage;
    [SerializeField] private TMP_Text selectedItemName;
    [SerializeField] private TMP_Text selectedItemDesc;
    [SerializeField] private Button btnAddToCart;      // "담기"

    [Header("Cart")]
    [SerializeField] private Transform cartParent;      // ScrollView/Viewport/Content
    [SerializeField] private GameObject cartItemPrefab;
    [SerializeField] private TMP_Text txtTotalPrice;
    [SerializeField] private Button btnBuy;

    [Header("Debug / Visual Aid")]
    [SerializeField] private bool debugSlotBackgrounds = false;

    private ShopItemSO _selectedItem;
    private readonly Dictionary<ShopItemSO, int> _cart = new();

    // ============================================================
    // Unity
    // ============================================================
    void Start()
    {
        EnsureVLG(itemListParent as RectTransform);
        EnsureVLG(cartParent as RectTransform);

        if (txtShopTitle) txtShopTitle.text = shopInventory ? shopInventory.shopTitle : "상점";
        RefreshGoldLabel(); // ✅ 누락되었던 메서드가 아래에 정의됨

        BuildItemList();

        if (btnAddToCart) btnAddToCart.onClick.AddListener(AddToCart);
        if (btnBuy) btnBuy.onClick.AddListener(() => BuyAll()); // ✅ 안전하게 람다로
        if (btnClose) btnClose.onClick.AddListener(OnClickClose);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) OnClickClose();
    }

    // ============================================================
    // Layout helpers
    // ============================================================
    private void EnsureVLG(RectTransform content)
    {
        if (!content) return;
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.offsetMin = new Vector2(0f, content.offsetMin.y);
        content.offsetMax = new Vector2(0f, content.offsetMax.y);

        var vlg = content.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var csf = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void RebuildLayout(Transform content)
    {
        if (!content) return;
        Canvas.ForceUpdateCanvases();
        var rect = content as RectTransform;
        if (rect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            var parentRect = rect.parent as RectTransform;
            if (parentRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        }
    }

    private Transform CreateChild(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        (go.transform as RectTransform).localScale = Vector3.one;
        return go.transform;
    }

    // ============================================================
    // Left: Item list
    // ============================================================
    private void BuildItemList()
    {
        if (!itemListParent || !itemSlotPrefab) { Debug.LogError("[Shop] itemListParent/itemSlotPrefab 누락"); return; }
        foreach (Transform c in itemListParent) Destroy(c.gameObject);

        if (shopInventory == null || shopInventory.items == null || shopInventory.items.Count == 0)
        { Debug.LogWarning("[Shop] shopInventory 비었음"); return; }

        var contentRT = (RectTransform)itemListParent;
        float parentW = Mathf.Max(1f, contentRT.rect.width);

        for (int i = 0; i < shopInventory.items.Count; i++)
        {
            var item = shopInventory.items[i];
            var slot = Instantiate(itemSlotPrefab, itemListParent);

            var rt = slot.transform as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            var le = slot.GetComponent<LayoutElement>() ?? slot.AddComponent<LayoutElement>();
            le.preferredHeight = 80f;
            le.flexibleWidth = 1f;
            le.minWidth = parentW - 1f;

            if (debugSlotBackgrounds)
            {
                var imgBg = slot.GetComponent<Image>() ?? slot.AddComponent<Image>();
                imgBg.color = new Color(1f, 0.2f, 0.2f, 0.2f);
            }

            var refs = EnsureShopItemSlotStructure(slot);

            if (refs.icon) refs.icon.sprite = item.Icon;
            if (refs.name) refs.name.text = item.DisplayName;
            if (refs.price) refs.price.text = Price(item.basePrice);

            var btn = slot.GetComponent<Button>() ?? slot.AddComponent<Button>();
            btn.onClick.AddListener(() => SelectItem(item));
        }

        SelectItem(shopInventory.items[0]);
        RebuildLayout(itemListParent);
    }

    private struct ShopItemSlotRefs
    {
        public Image icon;
        public TMP_Text name;
        public TMP_Text price;
    }

    private ShopItemSlotRefs EnsureShopItemSlotStructure(GameObject slotGO)
    {
        var hlg = slotGO.GetComponent<HorizontalLayoutGroup>() ?? slotGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        var iconTr = slotGO.transform.Find("Icon") ?? CreateChild(slotGO.transform, "Icon");
        var nameTr = slotGO.transform.Find("Name") ?? CreateChild(slotGO.transform, "Name");
        var priceTr = slotGO.transform.Find("Price") ?? CreateChild(slotGO.transform, "Price");

        var iconRT = iconTr as RectTransform;
        iconRT.anchorMin = new Vector2(0, 0.5f);
        iconRT.anchorMax = new Vector2(0, 0.5f);
        iconRT.pivot = new Vector2(0, 0.5f);
        iconRT.anchoredPosition = Vector2.zero;

        var iconLE = iconTr.GetComponent<LayoutElement>() ?? iconTr.gameObject.AddComponent<LayoutElement>();
        iconLE.preferredWidth = 56f; iconLE.preferredHeight = 56f; iconLE.minWidth = 56f; iconLE.minHeight = 56f;

        var iconImg = iconTr.GetComponent<Image>() ?? iconTr.gameObject.AddComponent<Image>();
        iconImg.preserveAspect = true; var ic = iconImg.color; ic.a = 1f; iconImg.color = ic; iconImg.raycastTarget = false;

        var nameRT = nameTr as RectTransform;
        nameRT.anchorMin = new Vector2(0, 0.5f);
        nameRT.anchorMax = new Vector2(1, 0.5f);
        nameRT.pivot = new Vector2(0.5f, 0.5f);
        nameRT.anchoredPosition = Vector2.zero;

        var nameLE = nameTr.GetComponent<LayoutElement>() ?? nameTr.gameObject.AddComponent<LayoutElement>();
        nameLE.flexibleWidth = 1f;

        var nameTxt = nameTr.GetComponent<TMP_Text>() ?? nameTr.gameObject.AddComponent<TextMeshProUGUI>();
        nameTxt.enableWordWrapping = false; nameTxt.overflowMode = TextOverflowModes.Ellipsis;
        nameTxt.alignment = TextAlignmentOptions.MidlineLeft; var nc = nameTxt.color; nc.a = 1f; nameTxt.color = nc;
        if (string.IsNullOrWhiteSpace(nameTxt.text)) nameTxt.text = " ";

        var priceRT = priceTr as RectTransform;
        priceRT.anchorMin = new Vector2(1, 0.5f);
        priceRT.anchorMax = new Vector2(1, 0.5f);
        priceRT.pivot = new Vector2(1, 0.5f);
        priceRT.anchoredPosition = Vector2.zero;

        var priceLE = priceTr.GetComponent<LayoutElement>() ?? priceTr.gameObject.AddComponent<LayoutElement>();
        priceLE.preferredWidth = 100f; priceLE.minWidth = 80f;

        var priceTxt = priceTr.GetComponent<TMP_Text>() ?? priceTr.gameObject.AddComponent<TextMeshProUGUI>();
        priceTxt.enableWordWrapping = false; priceTxt.overflowMode = TextOverflowModes.Truncate;
        priceTxt.alignment = TextAlignmentOptions.MidlineRight; var pc = priceTxt.color; pc.a = 1f; priceTxt.color = pc;
        if (string.IsNullOrWhiteSpace(priceTxt.text)) priceTxt.text = " ";

        return new ShopItemSlotRefs { icon = iconImg, name = nameTxt, price = priceTxt };
    }

    private void SelectItem(ShopItemSO item)
    {
        _selectedItem = item;
        if (selectedItemImage) selectedItemImage.sprite = item.Icon;
        if (selectedItemName) selectedItemName.text = item.DisplayName;
        if (selectedItemDesc) selectedItemDesc.text = item.Description;
    }

    // ============================================================
    // Cart
    // ============================================================
    private void AddToCart()
    {
        if (_selectedItem == null) { Debug.LogWarning("[Shop] _selectedItem is NULL"); return; }
        if (_cart.ContainsKey(_selectedItem)) _cart[_selectedItem]++;
        else _cart[_selectedItem] = 1;
        RefreshCart();
    }

    private void RefreshCart()
    {
        if (!cartParent || !cartItemPrefab) { Debug.LogError("[Shop] cartParent/cartItemPrefab 누락"); return; }
        foreach (Transform c in cartParent) Destroy(c.gameObject);

        int total = 0;
        var contentRT = (RectTransform)cartParent;
        float parentW = Mathf.Max(1f, contentRT.rect.width);

        foreach (var kvp in _cart.ToList())
        {
            var item = kvp.Key;
            int qty = Mathf.Max(0, kvp.Value);
            if (qty == 0) { _cart.Remove(item); continue; }

            int cost = item.basePrice * qty;
            total += cost;

            var slot = Instantiate(cartItemPrefab, cartParent);

            var rt = slot.transform as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            var le = slot.GetComponent<LayoutElement>() ?? slot.AddComponent<LayoutElement>();
            le.preferredHeight = 60f;
            le.flexibleWidth = 1f;
            le.minWidth = parentW - 1f;

            if (debugSlotBackgrounds)
            {
                var imgBg = slot.GetComponent<Image>() ?? slot.AddComponent<Image>();
                imgBg.color = new Color(0.2f, 0.6f, 1f, 0.2f);
            }

            ConfigureCartItemSlotChildren(slot);

            var nameT = slot.transform.Find("Name")?.GetComponent<TMP_Text>();
            var priceT = slot.transform.Find("Price")?.GetComponent<TMP_Text>();
            var qtyT = slot.transform.Find("Qty")?.GetComponent<TMP_Text>();
            var btnM = slot.transform.Find("Btn_Minus")?.GetComponent<Button>();
            var btnP = slot.transform.Find("Btn_Plus")?.GetComponent<Button>();

            if (nameT) nameT.text = item.DisplayName;
            if (priceT) priceT.text = Price(cost);
            if (qtyT) qtyT.text = qty.ToString();

            if (btnM) btnM.onClick.AddListener(() =>
            {
                _cart[item] = Mathf.Max(0, _cart[item] - 1);
                if (_cart[item] == 0) _cart.Remove(item);
                RefreshCart();
            });
            if (btnP) btnP.onClick.AddListener(() =>
            {
                _cart[item] = _cart[item] + 1;
                RefreshCart();
            });
        }

        if (txtTotalPrice) txtTotalPrice.text = $"총 가격: {PriceRaw(total)}";
        RebuildLayout(cartParent);
    }

    // 🔧 장바구니 슬롯 구성(누락되면 컴파일 에러)
    private void ConfigureCartItemSlotChildren(GameObject slotGO)
    {
        var hlg = slotGO.GetComponent<HorizontalLayoutGroup>() ?? slotGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        var nameTr = slotGO.transform.Find("Name") ?? CreateChild(slotGO.transform, "Name");
        var priceTr = slotGO.transform.Find("Price") ?? CreateChild(slotGO.transform, "Price");
        var qtyTr = slotGO.transform.Find("Qty") ?? CreateChild(slotGO.transform, "Qty");
        var minusTr = slotGO.transform.Find("Btn_Minus") ?? CreateChild(slotGO.transform, "Btn_Minus");
        var plusTr = slotGO.transform.Find("Btn_Plus") ?? CreateChild(slotGO.transform, "Btn_Plus");

        var nameLE = nameTr.GetComponent<LayoutElement>() ?? nameTr.gameObject.AddComponent<LayoutElement>();
        nameLE.flexibleWidth = 1f;
        var nameTxt = nameTr.GetComponent<TMP_Text>() ?? nameTr.gameObject.AddComponent<TextMeshProUGUI>();
        nameTxt.enableWordWrapping = false;
        nameTxt.alignment = TextAlignmentOptions.MidlineLeft;

        var priceLE = priceTr.GetComponent<LayoutElement>() ?? priceTr.gameObject.AddComponent<LayoutElement>();
        priceLE.preferredWidth = 120f;
        var priceTxt = priceTr.GetComponent<TMP_Text>() ?? priceTr.gameObject.AddComponent<TextMeshProUGUI>();
        priceTxt.enableWordWrapping = false;
        priceTxt.alignment = TextAlignmentOptions.MidlineRight;

        var qtyLE = qtyTr.GetComponent<LayoutElement>() ?? qtyTr.gameObject.AddComponent<LayoutElement>();
        qtyLE.preferredWidth = 48f;
        var qtyTxt = qtyTr.GetComponent<TMP_Text>() ?? qtyTr.gameObject.AddComponent<TextMeshProUGUI>();
        qtyTxt.alignment = TextAlignmentOptions.MidlineRight;

        var btnMinus = minusTr.GetComponent<Button>() ?? minusTr.gameObject.AddComponent<Button>();
        var minusImg = minusTr.GetComponent<Image>() ?? minusTr.gameObject.AddComponent<Image>();
        var minusLE = minusTr.GetComponent<LayoutElement>() ?? minusTr.gameObject.AddComponent<LayoutElement>();
        minusLE.preferredWidth = 28f; minusLE.preferredHeight = 28f;
        var minusLabel = minusTr.GetComponentInChildren<TMP_Text>();
        if (!minusLabel) { var t = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)); t.transform.SetParent(minusTr, false); minusLabel = t.GetComponent<TMP_Text>(); }
        minusLabel.text = "-"; minusLabel.alignment = TextAlignmentOptions.Center;

        var btnPlus = plusTr.GetComponent<Button>() ?? plusTr.gameObject.AddComponent<Button>();
        var plusImg = plusTr.GetComponent<Image>() ?? plusTr.gameObject.AddComponent<Image>();
        var plusLE = plusTr.GetComponent<LayoutElement>() ?? plusTr.gameObject.AddComponent<LayoutElement>();
        plusLE.preferredWidth = 28f; plusLE.preferredHeight = 28f;
        var plusLabel = plusTr.GetComponentInChildren<TMP_Text>();
        if (!plusLabel) { var t = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)); t.transform.SetParent(plusTr, false); plusLabel = t.GetComponent<TMP_Text>(); }
        plusLabel.text = "+"; plusLabel.alignment = TextAlignmentOptions.Center;
    }

    // ============================================================
    // Purchase
    // ============================================================
    // (기존 ShopManager 상단 using들은 그대로)
    private void BuyAll()
    {
        int total = _cart.Sum(kv => kv.Key.basePrice * kv.Value);
        if (total <= 0) return;

        if (GoldManager.Instance == null)
        {
            Debug.LogError("[Shop] GoldManager.Instance 없음! 구매 불가.");
            return;
        }

        if (!GoldManager.Instance.SpendGold(total))
        {
            Debug.Log("[Shop] 골드 부족.");
            RefreshGoldLabel();
            return;
        }

        // ✅ 인벤토리 보장
        var inv = PlayerInventoryLocator.Ensure();
        if (inv == null)
        {
            Debug.LogError("[Shop] PlayerInventory 생성 실패! 구매 적재 중단.");
            return;
        }

        int addedKinds = 0, addedCount = 0;

        foreach (var kv in _cart)
        {
            var item = kv.Key;
            int qty = Mathf.Max(0, kv.Value);
            if (qty <= 0) continue;

            string invId = item.ItemId;
            if (string.IsNullOrEmpty(invId))
            {
                Debug.LogWarning($"[Shop] ItemId 없음: {item.name} (itemRef 미연결?) → 적재 스킵");
                continue;
            }

            inv.Add(invId, qty);
            addedKinds++;
            addedCount += qty;

            Debug.Log($"[Shop] 구매→적재: {item.DisplayName} x{qty} (Id={invId})");
        }

        Debug.Log($"[Shop] 적재 완료: 종류 {addedKinds}, 개수 {addedCount}");

        _cart.Clear();
        RefreshCart();
        RefreshGoldLabel();
    }

    // =====================================
// ▼ 가격 표기 및 골드 갱신 함수 추가 ▼
// =====================================

// 가격 포맷터: 숫자에 , 찍고 G 붙임
private static string Price(int p)
{
    return $"{p:#,0}G";
}

// 원시 골드 표기: 공백 + G
private static string PriceRaw(int p)
{
    return $"{p:#,0} G";
}

// 골드 라벨 새로고침
private void RefreshGoldLabel()
{
    if (!txtPlayerGold) return;
    int g = GoldManager.Instance != null ? GoldManager.Instance.CurrentGold : 0;
    txtPlayerGold.text = $"보유골드: {PriceRaw(g)}";
}
// 상점 닫기 버튼 핸들러
private void OnClickClose()
{
    // shopSceneName이 지정되어 있고, 그 씬이 로드되어 있다면 그 씬만 언로드
    if (!string.IsNullOrEmpty(shopSceneName))
    {
        var s = UnityEngine.SceneManagement.SceneManager.GetSceneByName(shopSceneName);
        if (s.isLoaded)
        {
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(shopSceneName);
            return;
        }
    }

    // 그렇지 않으면 현재 이 컴포넌트가 있는 씬을 언로드
    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(gameObject.scene);
}

}