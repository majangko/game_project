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
        // ✅ PartyManager 자동 생성
        if (PartyManager.Instance == null)
        {
            var go = new GameObject("PartyManager");
            go.AddComponent<PartyManager>();
            go.transform.SetParent(null);
            Debug.Log("<color=yellow>[TeamSelect] PartyManager가 없어서 자동 생성됨.</color>");
        }
        else
        {
            Debug.Log("<color=green>[TeamSelect] PartyManager.Instance 이미 존재함.</color>");
        }

        foreach (var c in cards)
        {
            c.OnRerollClicked += HandleRerollClicked;
            c.OnSelectClicked += HandleSelectClicked;
            c.OnInfoClicked += _ => { };
        }

        if (saveButton) saveButton.onClick.AddListener(OnSaveClicked);

        Deal3Cards();
        if (saveButton) saveButton.interactable = false;
    }

    // ----------------- 카드 생성 / 선택 -----------------
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

                // ✅ PartyManager에 캐릭터 추가 (기존 파티 유지)
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
                        pm.AddMember(data);
                        Debug.Log($"<color=lime>[TeamSelect] {_selectedCandidate.displayName} added to PartyManager.</color>");
                    }

                    Debug.Log($"[TeamSelect] 현재 파티 수: {pm.GetCount()}");
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
