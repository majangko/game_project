// Assets/Scripts/Shop/ShopManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string shopSceneName = "Shop";

    [Header("Data")]
    [SerializeField] private ShopInventorySO shopInventory;

    [Header("Header UI")]
    [SerializeField] private TMP_Text txtShopTitle;
    [SerializeField] private TMP_Text txtPlayerGold;
    [SerializeField] private Button   btnClose;

    [Header("Left List (Items)")]
    [SerializeField] private Transform itemListParent;   // MUST: ScrollView/Viewport/Content
    [SerializeField] private GameObject itemSlotPrefab;  // children: Icon/Name/Price , root has Button

    [Header("Right Detail")]
    [SerializeField] private Image    selectedItemImage;
    [SerializeField] private TMP_Text selectedItemName;
    [SerializeField] private TMP_Text selectedItemDesc;
    [SerializeField] private Button   btnAddToCart;

    [Header("Cart")]
    [SerializeField] private Transform  cartParent;      // MUST: ScrollView/Viewport/Content
    [SerializeField] private GameObject cartItemPrefab;  // children: Name/Price/Btn_Minus/Qty/Btn_Plus
    [SerializeField] private TMP_Text   txtTotalPrice;
    [SerializeField] private Button     btnBuy;

    private ShopItemSO _selectedItem;
    private readonly Dictionary<ShopItemSO, int> _cart = new();

    void Start()
    {
        // sanity logs
        Debug.Log($"[Shop] Start | inv={(shopInventory? shopInventory.name : "NULL")} " +
                  $"items={(shopInventory? shopInventory.items.Count : 0)} " +
                  $"itemListParent={(itemListParent? itemListParent.name : "NULL")} " +
                  $"itemSlotPrefab={(itemSlotPrefab? itemSlotPrefab.name : "NULL")} " +
                  $"btnAddToCart={(btnAddToCart? "OK":"NULL")} cartParent={(cartParent? cartParent.name:"NULL")} cartItemPrefab={(cartItemPrefab? cartItemPrefab.name:"NULL")}");

        if (txtShopTitle) txtShopTitle.text = shopInventory ? shopInventory.shopTitle : "상점";

        BuildItemList();
        RefreshGoldLabel();

        if (btnAddToCart) btnAddToCart.onClick.AddListener(AddToCart);
        if (btnBuy)       btnBuy.onClick.AddListener(BuyAll);
        if (btnClose)     btnClose.onClick.AddListener(OnClickClose);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) OnClickClose();
    }

    // ---------- Left List ----------
    private void BuildItemList()
    {
        if (!itemListParent) { Debug.LogError("[Shop] itemListParent is NULL (Content를 연결)"); return; }
        if (!itemSlotPrefab) { Debug.LogError("[Shop] itemSlotPrefab is NULL"); return; }
        foreach (Transform c in itemListParent) Destroy(c.gameObject);

        if (shopInventory == null || shopInventory.items == null || shopInventory.items.Count == 0)
        {
            Debug.LogWarning("[Shop] shopInventory가 비었거나 items가 없습니다.");
            return;
        }

        int idx = 0;
        foreach (var item in shopInventory.items)
        {
            var slot = Instantiate(itemSlotPrefab, itemListParent);

            var icon  = slot.transform.Find("Icon")?.GetComponent<Image>();
            var name  = slot.transform.Find("Name")?.GetComponent<TMP_Text>();
            var price = slot.transform.Find("Price")?.GetComponent<TMP_Text>();
            var btn   = slot.GetComponent<Button>();

            if (!btn) Debug.LogError("[Shop] itemSlotPrefab 루트에 Button이 없습니다.");

            if (icon)  icon.sprite = item.icon;
            if (name)  name.text   = item.displayName;
            if (price) price.text  = FormatPrice(item.basePrice);

            if (btn)   btn.onClick.AddListener(() => SelectItem(item));

            Debug.Log($"[Shop] List item[{idx++}] = {item.displayName}");
        }

        // 첫 항목 자동 선택 (왼쪽 클릭 전에 우상단에 표시되는 게 정상)
        SelectItem(shopInventory.items[0]);
    }

    // ---------- Right Detail ----------
    private void SelectItem(ShopItemSO item)
    {
        _selectedItem = item;
        if (selectedItemImage) selectedItemImage.sprite = item.icon;
        if (selectedItemName)  selectedItemName.text    = item.displayName;
        if (selectedItemDesc)  selectedItemDesc.text    = item.description;
        Debug.Log($"[Shop] SelectItem => {_selectedItem.displayName}");
    }

    private void AddToCart()
    {
        if (_selectedItem == null)
        {
            Debug.LogWarning("[Shop] _selectedItem is NULL (왼쪽에서 아이템 클릭 이벤트가 안 들어옴)"); 
            return;
        }
        if (_cart.ContainsKey(_selectedItem)) _cart[_selectedItem]++;
        else _cart[_selectedItem] = 1;

        Debug.Log($"[Shop] AddToCart => {_selectedItem.displayName}, qty={_cart[_selectedItem]}");
        RefreshCart();
    }

    // ---------- Cart ----------
    private void RefreshCart()
    {
        if (!cartParent) { Debug.LogError("[Shop] cartParent is NULL (장바구니 Content 연결)"); return; }
        if (!cartItemPrefab) { Debug.LogError("[Shop] cartItemPrefab is NULL"); return; }

        foreach (Transform c in cartParent) Destroy(c.gameObject);

        int total = 0;

        foreach (var kvp in _cart.ToList())
        {
            var item = kvp.Key;
            int qty  = Mathf.Max(0, kvp.Value);
            if (qty == 0) { _cart.Remove(item); continue; }

            int cost = item.basePrice * qty;
            total += cost;

            var slot = Instantiate(cartItemPrefab, cartParent);

            var nameTx = slot.transform.Find("Name")?.GetComponent<TMP_Text>();
            var priceTx= slot.transform.Find("Price")?.GetComponent<TMP_Text>();
            var qtyTx  = slot.transform.Find("Qty")?.GetComponent<TMP_Text>();
            var btnMin = slot.transform.Find("Btn_Minus")?.GetComponent<Button>();
            var btnPls = slot.transform.Find("Btn_Plus")?.GetComponent<Button>();

            if (!nameTx || !priceTx || !qtyTx || !btnMin || !btnPls)
                Debug.LogError("[Shop] cartItemPrefab의 자식 이름이 맞나요? (Name/Price/Btn_Minus/Qty/Btn_Plus)");

            if (nameTx)  nameTx.text  = item.displayName;
            if (priceTx) priceTx.text = FormatPrice(cost);
            if (qtyTx)   qtyTx.text   = qty.ToString();

            if (btnMin) btnMin.onClick.AddListener(() =>
            {
                _cart[item] = Mathf.Max(0, _cart[item] - 1);
                if (_cart[item] == 0) _cart.Remove(item);
                RefreshCart();
            });

            if (btnPls) btnPls.onClick.AddListener(() =>
            {
                _cart[item] = _cart[item] + 1;
                RefreshCart();
            });
        }

        if (txtTotalPrice) txtTotalPrice.text = $"총 가격: {FormatPriceRaw(total)}";
    }

    private void BuyAll()
    {
        int total = _cart.Sum(kv => kv.Key.basePrice * kv.Value);
        if (total <= 0) return;

        if (GoldManager.Instance == null)
        {
            Debug.LogWarning("[Shop] GoldManager 인스턴스가 없습니다.");
            return;
        }

        if (!GoldManager.Instance.SpendGold(total))
        {
            Debug.Log("[Shop] 골드 부족");
            RefreshGoldLabel();
            return;
        }

        // TODO: player_inventory로 지급
        foreach (var kv in _cart)
            Debug.Log($"[Shop] 구매: {kv.Key.displayName} x{kv.Value}");

        _cart.Clear();
        RefreshCart();
        RefreshGoldLabel();
    }

    // ---------- UI utils ----------
    private void RefreshGoldLabel()
    {
        if (!txtPlayerGold) return;
        int g = GoldManager.Instance != null ? GoldManager.Instance.CurrentGold : 0;
        txtPlayerGold.text = $"보유골드: {FormatPriceRaw(g)}";
    }

    private static string FormatPrice(int price)    => $"{price:#,0}G";
    private static string FormatPriceRaw(int price) => $"{price:#,0} G";

    private void OnClickClose()
    {
        if (!string.IsNullOrEmpty(shopSceneName))
        {
            var s = SceneManager.GetSceneByName(shopSceneName);
            if (s.isLoaded) { SceneManager.UnloadSceneAsync(shopSceneName); return; }
        }
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }
}
