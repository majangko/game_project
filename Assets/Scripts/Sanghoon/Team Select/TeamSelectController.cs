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
    private HashSet<string> _alreadyChosen = new();
    private HashSet<string> _currentlyShown = new();
    private Dictionary<CardView, bool> _rerolledCards = new(); // ✅ 리롤 여부 추적
    private TeamMemberSO _selectedCandidate;
    private List<TeamMemberSO> _mockAll;

    void Start()
    {
        // ✅ PartyManager 확인 (중복 생성 방지)
        if (PartyManager.Instance == null)
        {
            // DontDestroyOnLoad 오브젝트가 없는 경우에만 새로 생성
            GameObject go = new GameObject("PartyManager");
            go.AddComponent<PartyManager>();
            go.transform.SetParent(null);
            Debug.Log("<color=yellow>[TeamSelect] PartyManager가 없어서 새로 생성됨.</color>");
        }
        else
        {
            Debug.Log("<color=green>[TeamSelect] PartyManager.Instance 이미 존재 → 기존 인스턴스 사용</color>");
        }

        // ✅ 카드 이벤트 연결
        foreach (var c in cards)
        {
            c.OnRerollClicked += HandleRerollClicked;
            c.OnSelectClicked += HandleSelectClicked;
            c.OnInfoClicked += _ => { };
        }

        // ✅ 저장 버튼 이벤트
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveClicked);

        // ✅ 첫 카드 세팅
        Deal3Cards();
        if (saveButton != null)
            saveButton.interactable = false;

        // ✅ 현재 PartyManager 멤버 로그 출력
        if (PartyManager.Instance != null)
        {
            var count = PartyManager.Instance.GetCount();
            Debug.Log($"<color=cyan>[TeamSelect] 현재 PartyManager 멤버 수: {count}</color>");
        }
    }


    // ----------------- 카드 생성 / 초기화 -----------------
    void Deal3Cards()
    {
        _currentlyShown.Clear();
        _rerolledCards.Clear();

        for (int i = 0; i < cards.Length; i++)
        {
            var member = DrawUniqueMember(_alreadyChosen.Union(_currentlyShown));
            _currentlyShown.Add(member.id);
            cards[i].Bind(member);
            _rerolledCards[cards[i]] = false; // ✅ 리롤 가능 상태로 초기화
            cards[i].SetRerollButtonInteractable(true); // 리롤 버튼 활성화
        }
    }

    // ----------------- 리롤 클릭 -----------------
    void HandleRerollClicked(CardView card)
    {
        // ✅ 이미 리롤한 카드면 무시
        if (_rerolledCards.TryGetValue(card, out bool alreadyRerolled) && alreadyRerolled)
        {
            Debug.Log($"[TeamSelect] {card.bound.displayName} 카드는 이미 리롤됨, 무시");
            return;
        }

        Debug.Log("[TeamSelect] Reroll clicked.");

        // ✅ 중복 방지 목록 만들기 (현재 카드 포함)
        var exclude = _alreadyChosen
            .Union(_currentlyShown)
            .Append(card.bound.id)
            .ToHashSet();

        // ✅ 새 멤버 뽑기
        var member = DrawUniqueMember(exclude);

        // 같은 캐릭터가 나오면 다시 뽑기 (안 바뀌는 문제 방지)
        int safety = 0;
        while (member.id == card.bound.id && safety < 5)
        {
            member = DrawUniqueMember(exclude);
            safety++;
        }

        _currentlyShown.Remove(card.bound.id);
        _currentlyShown.Add(member.id);
        _alreadyChosen.Add(member.id); // 중복 방지 목록에 추가

        // 새 캐릭터 적용
        card.Bind(member);

        // ✅ 리롤 완료 표시 및 버튼 비활성화
        _rerolledCards[card] = true;
        card.SetRerollButtonInteractable(false);

        Debug.Log($"[TeamSelect] {card.bound.displayName} → {member.displayName} (리롤 완료)");
    }

    // ----------------- 카드 선택 -----------------
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

    // ----------------- 저장 처리 -----------------
    // ----------------- 저장 처리 -----------------
    void OnSaveClicked()
    {
        if (_selectedCandidate == null)
        {
            confirmPanel.Show("선택된 팀원이 없습니다.", onYes: () => { });
            return;
        }

        confirmPanel.Show($"{_selectedCandidate.displayName} 을(를) 저장하시겠습니까?",
            onYes: () =>
            {
                _alreadyChosen.Add(_selectedCandidate.id);
                Debug.Log($"<color=cyan>[TeamSelect] Saved {_selectedCandidate.displayName}</color>");

                // ✅ PartyManager에 캐릭터 추가
                if (PartyManager.Instance != null)
                {
                    var pm = PartyManager.Instance;
                    var data = new PartyMemberData
                    {
                        id = _selectedCandidate.id,
                        portrait = _selectedCandidate.portrait,
                        prefab = _selectedCandidate.prefab
                    };

                    // 🔒 중복 방지
                    if (pm.currentMembers.Any(m => m.id == data.id))
                    {
                        Debug.Log($"[TeamSelect] 이미 존재하는 멤버 {data.id}, 추가 생략.");
                    }
                    else
                    {
                        pm.ClearParty();     // ✅ 기존 멤버 초기화 (단일 선택 게임 구조일 경우)
                        pm.AddMember(data);
                        Debug.Log($"<color=lime>[TeamSelect] {_selectedCandidate.displayName} added to PartyManager.</color>");
                    }

                    Debug.Log($"[TeamSelect] 현재 파티 수: {pm.GetCount()}");

                    // ✅ 추가: TagManager에도 즉시 반영 (씬 내 전투 테스트 시 필요)
                    var tag = FindObjectOfType<TagManager>();
                    if (tag != null)
                    {
                        pm.AssignToTagManager(tag, keepExisting: false);
                        Debug.Log($"<color=orange>[TeamSelect] TagManager 동기화 완료</color>");
                    }
                }

                saveButton.interactable = false;
                StartCoroutine(LoadNextSceneWithDelay());
            },
            onNo: () => Debug.Log("[TeamSelect] Save canceled.")
        );
    }


    // ----------------- 다음 스테이지 계산 -----------------
    System.Collections.IEnumerator LoadNextSceneWithDelay()
    {
        yield return new WaitForSeconds(0.1f);

        string prevScene = PlayerPrefs.GetString("LastScene", "Stage01");
        Debug.Log($"<color=yellow>[TeamSelect] 이전 씬: {prevScene} → 다음 스테이지 계산 중...</color>");

        int nextIndex = 1;

        if (prevScene.StartsWith("Boss"))
        {
            string numStr = prevScene.Replace("Boss", "");
            if (int.TryParse(numStr, out int num))
                nextIndex = num + 1;
        }
        else if (prevScene.StartsWith("Stage"))
        {
            string numStr = prevScene.Replace("Stage", "");
            if (int.TryParse(numStr, out int num))
                nextIndex = num + 1;
        }

        Debug.Log($"[TeamSelect] 다음 스테이지 인덱스 → Stage0{nextIndex}");

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadStage(nextIndex);
        else
            SceneManager.LoadScene($"Stage0{nextIndex}");
    }

    // ----------------- 카드 뽑기 로직 -----------------
    TeamMemberSO DrawUniqueMember(IEnumerable<string> exclude)
    {
        var pool = GetAllMembers();
        if (pool.Count == 0)
        {
            Debug.LogError("[TeamSelect] 후보 풀이 비었습니다. DB 연결 또는 useMockData 필요.");
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
