using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TagPanelUI : MonoBehaviour
{
    [System.Serializable]
    public class TagSlot
    {
        public Image portrait;       // 초상화 이미지
        public Image cooldownMask;   // 쿨타임 마스크
        public TMP_Text keyLabel;    // "1", "2" 등 키 표시
        public Button tagButton;     // 태그 버튼
    }

    [Header("슬롯 (태그 캐릭터용)")]
    public TagSlot[] slots = new TagSlot[2];

    void Start()
    {
        // 키 설정 및 버튼 연결
        for (int i = 0; i < slots.Length; i++)
        {
            int tagIndex = i + 1; // TagManager.characters[1]부터 표시
            if (slots[i].keyLabel != null)
                slots[i].keyLabel.text = (tagIndex + 1).ToString();

            if (slots[i].tagButton != null)
                slots[i].tagButton.onClick.AddListener(() =>
                {
                    if (TagManager.Instance != null)
                        TagManager.Instance.TryTag(tagIndex);
                });
        }

        LoadCharacterPortraits();

        if (TagManager.Instance != null)
            TagManager.Instance.OnCharacterSwitched.AddListener(OnTagSwitched);

        UpdateHighlight(TagManager.Instance?.GetCurrentIndex() ?? 0);
    }

    void Update()
    {
        if (TagManager.Instance == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) TagManager.Instance.TryTag(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TagManager.Instance.TryTag(2);
    }

    // -------------------------------
    // 초상화 로드 (비활성화 캐릭터 표시)
    // -------------------------------
    public void LoadCharacterPortraits()
    {
        var manager = TagManager.Instance;
        if (manager == null) return;

        string listNames = string.Join(", ", manager.characters.Select(c => c != null ? c.name : "null"));
        Debug.Log($"[TagPanelUI] 현재 TagManager.characters: {listNames}");

        int activeIndex = manager.GetCurrentIndex();
        int slot = 0;

        for (int i = 0; i < manager.characters.Count; i++)
        {
            if (i == activeIndex) continue; // 현재 캐릭터는 HUD에서 표시 중
            if (slot >= slots.Length) break;

            var player = manager.characters[i];
            var stats = player?.GetComponent<PlayerStats>();
            if (slots[slot].portrait == null) continue;

            if (stats != null && stats.portrait != null)
            {
                slots[slot].portrait.sprite = stats.portrait;
                slots[slot].portrait.color = Color.white;
                Debug.Log($"[TagPanelUI] 슬롯 {slot} ← {player.name} 초상화 적용");
            }
            else
            {
                slots[slot].portrait.sprite = null;
                slots[slot].portrait.color = new Color(1, 1, 1, 0.25f);
            }

            slot++;
        }
    }

    private void OnTagSwitched(int activeIndex)
    {
        if (TagManager.Instance == null) return;

        Debug.Log($"[TagPanelUI] OnTagSwitched 호출됨 (activeIndex = {activeIndex})");

        TagManager.Instance.RefreshCharacterOrder();
        LoadCharacterPortraits();
        UpdateHighlight(0);
    }

    public void UpdateHighlight(int activeIndex)
    {
        if (slots == null || slots.Length == 0) return;

        for (int i = 0; i < slots.Length; i++)
        {
            int characterIndex = i + 1;
            if (slots[i].portrait == null) continue;

            if (activeIndex == characterIndex)
                slots[i].portrait.color = Color.white;
            else
                slots[i].portrait.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        }
    }
}
