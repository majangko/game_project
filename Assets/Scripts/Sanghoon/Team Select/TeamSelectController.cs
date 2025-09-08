// FILE: TeamSelectController.cs
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public ConfirmPanel confirmPanel;    // 저장에서만 사용
    public TMP_Text titleText;

    [Header("동작 옵션")]
    [Tooltip("목업 전용 데이터로 테스트")]
    public bool useMockData = true;
    [Range(3, 30)] public int mockTotal = 12;

    [Tooltip("선택 시 덮어쓸 슬롯 인덱스 (0 = 첫 슬롯)")]
    public int targetSlotIndex = 0;

    [Tooltip("저장 확정 후 선택 슬롯을 잠글지 여부(원하면 사용)")]
    public bool lockSlotOnSave = false;

    // 내부 상태
    HashSet<string> _alreadyChosen = new();  // 저장 확정된 아이디들
    HashSet<string> _currentlyShown = new(); // 현재 카드에 표시중인 아이디들
    TeamMemberSO _selectedCandidate;         // 최근에 Select로 고른 후보

    // 목업 캐시
    List<TeamMemberSO> _mockAll;

    void Start()
    {
        // 카드 버튼 이벤트 구독
        foreach (var c in cards)
        {
            c.OnRerollClicked += HandleRerollClicked;
            c.OnSelectClicked += HandleSelectClicked;
            c.OnInfoClicked += _ => { /* 필요 시 로깅 */ };
        }

        if (saveButton) saveButton.onClick.AddListener(OnSaveClicked);

        // 최초 3장
        Deal3Cards();

        // UI 상태
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
        Debug.Log("[TeamSelect] Save clicked. candidate=" + (_selectedCandidate ? _selectedCandidate.displayName : "null"));

        var exclude = _alreadyChosen.Union(_currentlyShown.Where(id => id != card.bound.id));
        var member = DrawUniqueMember(exclude);
        _currentlyShown.Remove(card.bound.id);
        _currentlyShown.Add(member.id);
        card.Bind(member);
    }

    void HandleSelectClicked(CardView card)
{
    // 선택된 후보 저장
    _selectedCandidate = card.bound;

    // 슬롯에 바로 반영 (TargetSlotIndex 사용)
    if (teamSlots != null && teamSlots.Length > 0)
    {
        int idx = Mathf.Clamp(targetSlotIndex, 0, teamSlots.Length - 1);
        teamSlots[idx].Set(_selectedCandidate);
    }

    // 저장 버튼 활성화
    if (saveButton != null)
        saveButton.interactable = true;

    Debug.Log($"[TeamSelect] Selected {_selectedCandidate.displayName}");
}

void OnSaveClicked()
{
    if (_selectedCandidate == null)
    {
        Debug.Log("[TeamSelect] Save clicked but no candidate selected.");
        confirmPanel.Show("선택된 팀원이 없습니다.", onYes: () => { });
        return;
    }

    // 확인창 띄우기
    confirmPanel.Show($"{_selectedCandidate.displayName} 을(를) 저장하시겠습니까?",
        onYes: () =>
        {
            _alreadyChosen.Add(_selectedCandidate.id);
            Debug.Log($"[TeamSelect] Saved {_selectedCandidate.displayName}");
            saveButton.interactable = false; // 저장 후 다시 비활성화
        },
        onNo: () =>
        {
            Debug.Log("[TeamSelect] Save canceled.");
        }
    );
}


    void FinalizeSave()
    {
        _alreadyChosen.Add(_selectedCandidate.id);

        if (lockSlotOnSave)
        {
            // 필요 시 선택 불가 처리 등 정책 추가
        }

        // 다음 선택을 위해 상태 정리
        _selectedCandidate = null;
        if (saveButton) saveButton.interactable = false;

        // 다음 장에서 다시 뽑고 싶다면:
        // Deal3Cards();
    }

    // ---------- 가중치 랜덤 & 중복 방지 ----------
    TeamMemberSO DrawUniqueMember(IEnumerable<string> exclude)
    {
        var pool = GetAllMembers(); // 괄호 필수
        if (pool.Count == 0)
        {
            Debug.LogError("[TeamSelect] 후보 풀이 비었습니다. DB를 연결하거나 useMockData를 켜세요.");
            return CreateMock(1, Rarity.Common);
        }

        var excl = new HashSet<string>(exclude ?? Enumerable.Empty<string>());
        var filtered = pool.Where(m => !excl.Contains(m.id)).ToList();  // ToList 필수

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

    // ---------- 데이터 소스 ----------
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
        mock.displayName = num.ToString();
        mock.rarity = rarity;
        mock.description = $"팀원 #{num} 설명 텍스트(목업).";
        mock.portrait = null; // 필요 시 테스트 스프라이트 할당
        mock.skillIconA = null;
        mock.skillIconB = null;
        return mock;
    }
}
