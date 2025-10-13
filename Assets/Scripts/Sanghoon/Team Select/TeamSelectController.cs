// FILE: TeamSelectController.cs
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TeamSelectController : MonoBehaviour
{
    [Header("DB & 확률")]
    public TeamDatabase db;
    [Range(0, 100)] public int commonWeight = 60;
    [Range(0, 100)] public int epicWeight = 30;
    [Range(0, 100)] public int legWeight = 10;

    [Header("카드 3장")]
    public CardView[] cards;

    [Header("슬롯/버튼/패널")]
    public TeamSlotView[] teamSlots;
    public Button saveButton;
    public ConfirmPanel confirmPanel;
    public TMP_Text titleText;

    [Header("동작 옵션")]
    public bool useMockData = true;
    [Range(3, 30)] public int mockTotal = 12;
    public int targetSlotIndex = 0;
    public bool lockSlotOnSave = false;

    // 내부 상태
    HashSet<string> _alreadyChosen = new();
    HashSet<string> _currentlyShown = new();
    TeamMemberSO _selectedCandidate;
    List<TeamMemberSO> _mockAll;

    void Start()
    {
        // ✅ PartyManager 자동 생성 (씬 루트에 생성)
        if (PartyManager.Instance == null)
        {
            var go = new GameObject("PartyManager");
            go.AddComponent<PartyManager>();
            go.transform.SetParent(null); // 🔥 부모 완전 해제 (루트로 이동)
            Debug.Log("<color=yellow>[TeamSelect] PartyManager가 없어서 자동 생성됨.</color>");
        }
        else
        {
            Debug.Log("<color=green>[TeamSelect] PartyManager.Instance 이미 존재함.</color>");
        }

        // 카드 버튼 이벤트 구독
        foreach (var c in cards)
        {
            c.OnRerollClicked += HandleRerollClicked;
            c.OnSelectClicked += HandleSelectClicked;
            c.OnInfoClicked += _ => { };
        }

        if (saveButton) saveButton.onClick.AddListener(OnSaveClicked);

        // 최초 3장 뽑기
        Deal3Cards();

        if (saveButton) saveButton.interactable = false;
    }

    // ---------- 생성/뽑기 ----------
    void Deal3Cards()
    {
        _currentlyShown.Clear();
        for (int i = 0; i < cards.Length; i++)
        {
            var member = DrawUniqueMember(_alreadyChosen.Union(_currentlyShown));
            _currentlyShown.Add(member.id);
            cards[i].Bind(member);
        }
    }

    void HandleRerollClicked(CardView card)
    {
        Debug.Log("[TeamSelect] Reroll clicked.");

        var exclude = _alreadyChosen.Union(_currentlyShown.Where(id => id != card.bound.id));
        var member = DrawUniqueMember(exclude);
        _currentlyShown.Remove(card.bound.id);
        _currentlyShown.Add(member.id);
        card.Bind(member);
    }

    void HandleSelectClicked(CardView card)
    {
        _selectedCandidate = card.bound;

        if (teamSlots != null && teamSlots.Length > 0)
        {
            int idx = Mathf.Clamp(targetSlotIndex, 0, teamSlots.Length - 1);
            teamSlots[idx].Set(_selectedCandidate);
        }

        if (saveButton != null)
            saveButton.interactable = true;

        Debug.Log($"[TeamSelect] Selected {_selectedCandidate.displayName}");
    }

    void OnSaveClicked()
    {
        if (_selectedCandidate == null)
        {
            Debug.LogWarning("[TeamSelect] Save clicked but no candidate selected.");
            confirmPanel.Show("선택된 팀원이 없습니다.", onYes: () => { });
            return;
        }

        confirmPanel.Show($"{_selectedCandidate.displayName} 을(를) 저장하시겠습니까?",
            onYes: () =>
            {
                _alreadyChosen.Add(_selectedCandidate.id);
                Debug.Log($"<color=cyan>[TeamSelect] Saved {_selectedCandidate.displayName}</color>");

                // ✅ PartyManager 등록
                if (PartyManager.Instance != null)
                {
                    var data = new PartyMemberData
                    {
                        id = _selectedCandidate.id,
                        portrait = _selectedCandidate.portrait,
                        prefab = _selectedCandidate.prefab
                    };
                    PartyManager.Instance.AddMember(data);
                    Debug.Log($"<color=lime>[TeamSelect] {_selectedCandidate.displayName} added to PartyManager.</color>");
                }

                Debug.Log($"[TeamSelect] PartyManager 파티원 수: {PartyManager.Instance?.GetCount()}");

                saveButton.interactable = false;

                // ✅ 씬 이동 전에 0.1초 대기 (중요)
                StartCoroutine(LoadNextSceneWithDelay());
            },
            onNo: () => Debug.Log("[TeamSelect] Save canceled.")
        );
    }

    System.Collections.IEnumerator LoadNextSceneWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("<color=yellow>[TeamSelect] test_ts 씬으로 이동 중...</color>");
        SceneManager.LoadScene("test_ts");
    }


    // ---------- 데이터 생성 ----------
    TeamMemberSO DrawUniqueMember(IEnumerable<string> exclude)
    {
        var pool = GetAllMembers();
        if (pool.Count == 0)
        {
            Debug.LogError("[TeamSelect] 후보 풀이 비었습니다. DB 연결 또는 useMockData 활성화 필요.");
            return CreateMock(1, Rarity.Common);
        }

        var excl = new HashSet<string>(exclude ?? Enumerable.Empty<string>());
        var filtered = pool.Where(m => !excl.Contains(m.id)).ToList();
        if (filtered.Count == 0) filtered = pool;

        var target = ChooseRarityByWeight();
        var byRarity = filtered.Where(m => m.rarity == target).ToList();
        if (byRarity.Count > 0) return byRarity[Random.Range(0, byRarity.Count)];
        return filtered[Random.Range(0, filtered.Count)];
    }

    Rarity ChooseRarityByWeight()
    {
        int c = Mathf.Max(0, commonWeight);
        int e = Mathf.Max(0, epicWeight);
        int l = Mathf.Max(0, legWeight);
        int total = c + e + l;
        if (total <= 0) return Rarity.Common;

        int roll = Random.Range(1, total + 1);
        if (roll <= c) return Rarity.Common;
        if (roll <= c + e) return Rarity.Epic;
        return Rarity.Legendary;
    }

    List<TeamMemberSO> GetAllMembers()
    {
        if (db != null && db.All != null && db.All.Count > 0)
            return db.All;

        if (useMockData)
        {
            BuildMockIfNeeded();
            return _mockAll;
        }
        return new List<TeamMemberSO>();
    }

    void BuildMockIfNeeded()
    {
        if (_mockAll != null && _mockAll.Count == mockTotal) return;

        _mockAll = new List<TeamMemberSO>(mockTotal);
        for (int i = 1; i <= mockTotal; i++)
        {
            var rarity = ChooseRarityByWeight();
            _mockAll.Add(CreateMock(i, rarity));
        }
    }

    TeamMemberSO CreateMock(int num, Rarity rarity)
    {
        var mock = ScriptableObject.CreateInstance<TeamMemberSO>();
        mock.id = num.ToString();
        mock.displayName = $"Mock #{num}";
        mock.rarity = rarity;
        mock.description = $"테스트용 팀원 #{num}";
        return mock;
    }
}
