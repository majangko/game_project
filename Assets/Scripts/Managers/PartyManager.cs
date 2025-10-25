using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 플레이어 파티(선택된 동료 목록)를 관리하는 전역 매니저.
/// - TeamSelectController에서 AddMember()로 등록.
/// - 다음 스테이지 진입 시 TagManager.AssignToTagManager()로 전달.
/// - DontDestroyOnLoad로 씬 이동 간 유지됨.
/// </summary>
[System.Serializable]
public class PartyMemberData
{
    public string id;               // 고유 ID (TeamMemberSO.id)
    public Sprite portrait;         // 초상화
    public GameObject prefab;       // 실제 전투용 프리팹
}

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance;

    [Header("현재 파티 구성원")]
    public List<PartyMemberData> currentMembers = new List<PartyMemberData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // 루트 보장
            DontDestroyOnLoad(gameObject);
            Debug.Log("<color=lime>[PartyManager] DontDestroyOnLoad 적용 완료</color>");
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[PartyManager] 중복 인스턴스 발견 → 삭제됨");
            Destroy(gameObject);
        }
    }

    // -------------------- 파티 구성 제어 --------------------

    /// <summary>
    /// 파티 전체 초기화 (스테이지 전환 전 전체 삭제 등)
    /// </summary>
    public void ClearParty()
    {
        currentMembers.Clear();
        Debug.Log("[PartyManager] 파티 초기화 완료");
    }

    /// <summary>
    /// 새 멤버를 추가 (중복 방지)
    /// </summary>
    public void AddMember(PartyMemberData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[PartyManager] 잘못된 멤버 데이터(null)입니다.");
            return;
        }

        if (data.prefab == null)
        {
            Debug.LogWarning($"[PartyManager] {data.id}의 prefab이 null입니다.");
        }

        if (currentMembers.Exists(m => m.id == data.id))
        {
            Debug.Log($"[PartyManager] 이미 존재하는 멤버 {data.id}, 추가 생략.");
            return;
        }

        currentMembers.Add(data);
        Debug.Log($"[PartyManager] {data.id} 추가됨. 현재 파티 수: {currentMembers.Count}");
    }

    /// <summary>
    /// 현재 파티 구성원 리스트 반환 (읽기 전용 복제본)
    /// </summary>
    public List<PartyMemberData> GetAllMembers()
    {
        return new List<PartyMemberData>(currentMembers);
    }

    // -------------------- TagManager 연동 --------------------

    /// <summary>
    /// 현재 파티 데이터를 TagManager에 전달하여 전투 캐릭터 생성
    /// </summary>
    /// <param name="tagManager">연결 대상 TagManager</param>
    /// <param name="keepExisting">
    /// true일 경우, TagManager 내 기존 캐릭터(Guma_B 등)를 유지한 채 멤버 추가.
    /// false일 경우, 기존 캐릭터 리스트를 모두 초기화하고 새로 구성.
    /// </param>
    public void AssignToTagManager(TagManager tagManager, bool keepExisting = true)
    {
        if (tagManager == null)
        {
            Debug.LogError("[PartyManager] TagManager가 존재하지 않습니다.");
            return;
        }

        if (!keepExisting)
        {
            tagManager.characters.Clear(); // 기존 캐릭터 전부 삭제
            Debug.Log("[PartyManager] TagManager 캐릭터 리스트 초기화됨.");
        }

        foreach (var member in currentMembers)
        {
            if (member.prefab == null)
            {
                Debug.LogWarning($"[PartyManager] {member.id} 프리팹이 없습니다. 건너뜀.");
                continue;
            }

            // ✅ 중복 방지: 이미 동일 이름/ID의 캐릭터가 존재한다면 추가 생략
            if (tagManager.characters.Exists(c => c != null && c.name == member.prefab.name))
            {
                Debug.Log($"[PartyManager] {member.id}는 이미 TagManager에 존재, 중복 추가 생략.");
                continue;
            }

            // 파티 구성원 프리팹을 인스턴스화하여 TagManager에 전달
            GameObject obj = Object.Instantiate(member.prefab);
            obj.name = member.prefab.name;

            // ✅ Player 태그 자동 지정 (PlayerData와 연동 위해)
            obj.tag = "Player";
            obj.SetActive(false); // 기본은 비활성 상태

            // ✅ 스폰 시점 보정: PlayerSpawnPoints가 있다면 그 위치로 생성
            GameObject spawn = GameObject.Find("PlayerSpawnPoints");
            if (spawn != null)
            {
                obj.transform.position = spawn.transform.position;
                Debug.Log($"[PartyManager] {member.id} 스폰포인트에서 생성됨 ({spawn.transform.position})");
            }

            var ctrl = obj.GetComponent<SpumPlatformerController>();
            if (ctrl != null)
            {
                tagManager.characters.Add(ctrl);
                Debug.Log($"[PartyManager] {member.id} 전투용으로 등록됨.");
            }
            else
            {
                Debug.LogWarning($"[PartyManager] {member.id} 프리팹에 SpumPlatformerController가 없습니다.");
            }
        }

        Debug.Log($"<color=cyan>[PartyManager] {currentMembers.Count}명의 멤버가 TagManager에 추가됨 (keepExisting={keepExisting})</color>");
    }

    /// <summary>
    /// 파티 구성원 수 반환
    /// </summary>
    public int GetCount()
    {
        return currentMembers.Count;
    }
}
