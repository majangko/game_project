using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UseItemPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform itemListParent;   // ScrollView/Viewport/Content
    [SerializeField] private GameObject itemRowPrefab;   // ItemRow(버튼+TMP) 또는 ItemRowView
    [SerializeField] private Button btnUse;
    [SerializeField] private Button btnSkip;

    private System.Action onFinished;
    private string _selectedItemId;
    private PlayerInventory _inv;

    private void Awake()
    {
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    public void Open(System.Action afterClose)
    {
        gameObject.SetActive(true);
        onFinished = afterClose;

        if (btnUse)  { btnUse.onClick.RemoveAllListeners();  btnUse.onClick.AddListener(OnClickUse); }
        if (btnSkip) { btnSkip.onClick.RemoveAllListeners(); btnSkip.onClick.AddListener(OnClickSkip); }

        BuildList();
        StartCoroutine(CoLateRebuild());
    }

    private IEnumerator CoLateRebuild()
    {
        yield return null;
        foreach (var rt in GetComponentsInChildren<RectTransform>(true))
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    private void BuildList()
    {
        if (!itemListParent || !itemRowPrefab)
        {
            Debug.LogError("[UseItemPopup] itemListParent / itemRowPrefab 미할당 → 팝업을 닫고 진행합니다.");
            Close();
            return;
        }

        EnsureListLayout(itemListParent);

        foreach (Transform c in itemListParent) Destroy(c.gameObject);

        _inv = PlayerInventoryExtensions.FindWithItemsOrSingleton();
        if (_inv == null)
        {
            Debug.LogError("[UseItemPopup] PlayerInventory 인스턴스를 찾지 못했습니다. 팝업을 닫습니다.");
            Close();
            return;
        }

        Dictionary<string, int> snap = null;
        try
        {
            var all = _inv.All();
            if (all != null && all.Count > 0) snap = new Dictionary<string, int>(all);
        }
        catch { }

        if (snap == null || snap.Count == 0)
            snap = PlayerInventoryExtensions.Snapshot(_inv);

        Debug.Log($"[UseItemPopup] 인벤 선택: {_inv.name}  (아이템 종류={snap.Count})");

        if (snap.Count == 0)
        {
            CreatePlainRow("보유한 아이템이 없습니다.");
            return;
        }

        ShopCatalog.WarmupFromManagers();

        int index = 0;
        int created = 0;
        foreach (var kv in snap)
        {
            var id = kv.Key;
            var qty = kv.Value;

            string dispName = id;
            string desc = "";
            Sprite icon = null;

            var so = ShopCatalog.GetById(id);
            if (so != null)
            {
                var t = so.GetType();
                dispName = GetStringFieldOrProp(t, so, new[] { "displayName", "Name", "title", "itemName" }) ?? id;
                desc     = GetStringFieldOrProp(t, so, new[] { "description", "desc", "tooltip" }) ?? "";
                icon     = GetSpriteFieldOrProp(t, so, new[] { "icon", "sprite", "art", "image" });
            }

            var row = Instantiate(itemRowPrefab, itemListParent);
            row.SetActive(true);
            var view = row.GetComponent<ItemRowView>();
            if (view != null)
            {
                view.Bind(id, dispName, desc, icon, qty, OnClickSelect);
                view.SetSelected(index == 0);
            }
            else
            {
                var txt = row.GetComponentInChildren<TextMeshProUGUI>(true);
                if (!txt)
                {
                    var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                    go.transform.SetParent(row.transform, false);
                    txt = go.GetComponent<TextMeshProUGUI>();
                    txt.fontSize = 22;
                }
                txt.text = $"{dispName}   x{qty}";

                var btn = row.GetComponentInChildren<Button>(true) ?? row.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnClickSelect(id));
            }

            var le = row.GetComponent<LayoutElement>() ?? row.AddComponent<LayoutElement>();
            if (le.minHeight < 48) le.minHeight = 48;

            if (index == 0) _selectedItemId = id;
            index++;
            created++;
        }

        Debug.Log($"[UseItemPopup] 생성된 행 수 = {created} (Content children = {itemListParent.childCount})");
    }

    private void OnClickSelect(string id)
    {
        _selectedItemId = id;

        foreach (Transform c in itemListParent)
        {
            var v = c.GetComponent<ItemRowView>();
            if (v != null) v.SetSelected(false);
        }

        foreach (Transform c in itemListParent)
        {
            var v = c.GetComponent<ItemRowView>();
            if (v == null) continue;
            // 단순 하이라이트만 필요하므로 id 매칭은 생략(선택 이벤트에서 이미 지정됨)
            v.SetSelected(GetFirstSelectedIdGuess(c.gameObject) == _selectedItemId);
        }
    }

    private void OnClickUse()
    {
        if (string.IsNullOrEmpty(_selectedItemId))
        {
            Debug.Log("[UseItemPopup] 선택된 아이템 없음");
            return;
        }

        ItemBuffRuntime.Instance?.ApplyByItemId(_selectedItemId);

        bool consumed = PlayerInventoryExtensions.TryConsume(_inv, _selectedItemId, 1);
        Debug.Log($"[UseItemPopup] Use '{_selectedItemId}' → Consumed={consumed}");

        Close();
    }

    private void OnClickSkip()
    {
        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        onFinished?.Invoke();
    }

    // ───────────── 유틸 ─────────────

    private void EnsureListLayout(Transform content)
    {
        var vlg = content.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.spacing = 6;

        var csf = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var rt = content as RectTransform;
        if (rt)
        {
            rt.pivot = new Vector2(0, 1);
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
        }
    }

    private void CreatePlainRow(string message)
    {
        var row = Instantiate(itemRowPrefab, itemListParent);
        row.SetActive(true);
        var txt = row.GetComponentInChildren<TextMeshProUGUI>(true);
        if (!txt)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(row.transform, false);
            txt = go.GetComponent<TextMeshProUGUI>();
            txt.fontSize = 22;
        }
        txt.text = message;

        var le = row.GetComponent<LayoutElement>() ?? row.AddComponent<LayoutElement>();
        if (le.minHeight < 48) le.minHeight = 48;
    }

    private static string GetFirstSelectedIdGuess(GameObject row)
    {
        // 선택 하이라이트만 필요하므로 구현 생략(필요 시 Text 파싱해서 id 반환)
        return null;
    }

    private static string GetStringFieldOrProp(System.Type t, object obj, string[] candidates)
    {
        foreach (var name in candidates)
        {
            var p = t.GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);
            if (p != null) { try { var v = p.GetValue(obj); if (v != null) return v.ToString(); } catch { } }
            var f = t.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.IgnoreCase);
            if (f != null) { try { var v = f.GetValue(obj); if (v != null) return v.ToString(); } catch { } }
        }
        return null;
    }

    private static Sprite GetSpriteFieldOrProp(System.Type t, object obj, string[] candidates)
    {
        foreach (var name in candidates)
        {
            var p = t.GetProperty(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);
            if (p != null) { try { var v = p.GetValue(obj) as Sprite; if (v != null) return v; } catch { } }
            var f = t.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.IgnoreCase);
            if (f != null) { try { var v = f.GetValue(obj) as Sprite; if (v != null) return v; } catch { } }
        }
        return null;
    }
}
