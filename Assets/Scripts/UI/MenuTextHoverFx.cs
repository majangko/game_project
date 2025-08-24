using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Button))]
public class MenuTextHoverFX : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Refs")]
    public TMP_Text label;                // 버튼의 Text (TMP)

    [Header("Sizes")]
    public float normalSize = 64f;
    public float hoverSize  = 76f;

    [Header("Colors")]
    public Color normalColor    = new Color32(0x44,0x44,0x44,0xFF);
    public Color highlightColor = new Color32(0xF6,0xE8,0xC6,0xFF);

    [Header("Prefix ▷ settings")]
    public bool showPrefixOnHover = true;
    public string prefixGlyph = "▷";      // U+25B7
    public float reservedPrefixPixels = 30f; // 항상 확보할 가로폭(px). 26~34 사이로 조절.

    [Header("Tween")]
    public float duration = 0.12f;        // 변화 속도(초)

    string raw; bool over; float t;

    void Reset()
    {
        label = GetComponentInChildren<TMP_Text>();
        // 버튼 배경은 투명, 색 변화 타깃은 라벨
        var img = GetComponent<Image>(); if (img) img.color = new Color(0,0,0,0);
        var btn = GetComponent<Button>(); if (btn && label) btn.targetGraphic = label;
    }

    void Awake()
    {
        if (!label) label = GetComponentInChildren<TMP_Text>();
        if (label)
        {
            raw = label.text.Replace(prefixGlyph + " ", string.Empty);
            // 항상 동일 폭 예약(비호버 상태)
            label.text = $"<space={reservedPrefixPixels}>{raw}";
            label.enableAutoSizing = false;
            label.raycastTarget = false;
            label.fontSize = normalSize;
            label.color = normalColor;
        }
    }

    void OnEnable()
    {
        // 켜질 때 상태 동기화
        ApplyPrefix(false);
        t = 0f; over = false;
        if (label)
        {
            label.fontSize = normalSize;
            label.color = normalColor;
        }
    }

    void Update()
    {
        if (!label) return;
        float target = over ? 1f : 0f;
        t = Mathf.MoveTowards(t, target, Time.unscaledDeltaTime / Mathf.Max(0.0001f, duration));
        label.fontSize = Mathf.Lerp(normalSize, hoverSize, t);
        label.color    = Color.Lerp(normalColor, highlightColor, t);
    }

    void ApplyPrefix(bool on)
    {
        if (!label || !showPrefixOnHover) return;
        label.text = on ? $"{prefixGlyph} {raw}" : $"<space={reservedPrefixPixels}>{raw}";
    }

    public void OnPointerEnter(PointerEventData e){ over = true;  ApplyPrefix(true);  }
    public void OnPointerExit (PointerEventData e){ over = false; ApplyPrefix(false); }
    public void OnSelect     (BaseEventData e)    { over = true;  ApplyPrefix(true);  } // 키보드/패드 포커스
    public void OnDeselect   (BaseEventData e)    { over = false; ApplyPrefix(false); }
}
