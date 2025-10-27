using System.Collections;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("기본 캐릭터 프리팹")]
    public GameObject defaultPlayerPrefab; // ← Project 폴더의 guma_test.prefab을 여기에 연결

    void Awake()
    {
        // PartyManager 없으면 새로 생성
        if (PartyManager.Instance == null)
        {
            var pm = new GameObject("PartyManager");
            pm.AddComponent<PartyManager>();
            DontDestroyOnLoad(pm);
            Debug.Log("[GameInitializer] PartyManager 자동 생성됨 ✅");
        }
    }

    void Start()
    {
        StartCoroutine(InitializeSequence());
    }

    private IEnumerator InitializeSequence()
    {
        Debug.Log("<color=cyan>[GameInitializer] 초기화 시퀀스 시작</color>");

        yield return null;
        yield return new WaitUntil(() =>
            PartyManager.Instance != null &&
            FindObjectOfType<HUDController>() != null &&
            FindObjectOfType<TagManager>() != null);

        yield return new WaitForSeconds(0.05f);

        InitializeDefaultParty();

        var hud = FindObjectOfType<HUDController>();
        if (hud != null)
        {
            hud.UpdatePartyPortraits();
            Debug.Log("[GameInitializer] HUD 파티 초상화 갱신 완료 ✅");
        }

        var tag = FindObjectOfType<TagManager>();
        if (tag != null)
        {
            tag.TryConnectPartyManager();
            Debug.Log("[GameInitializer] TagManager 연동 완료 ✅");
        }

        Debug.Log("<color=lime>[GameInitializer] 초기화 전체 완료 ✅</color>");
    }

    void InitializeDefaultParty()
    {
        if (GoldManager.Instance != null)
            Debug.Log($"[GameInitializer] 기존 골드 유지: {GoldManager.Instance.CurrentGold} G");

        if (PartyManager.Instance != null)
            PartyManager.Instance.ClearParty();

        // 🔹 Project Prefab 참조 사용
        if (defaultPlayerPrefab == null)
        {
            Debug.LogWarning("[GameInitializer] 기본 캐릭터 프리팹이 설정되지 않았습니다 ❌");
            return;
        }

        // 🔹 프리팹 안의 PlayerStats 정보 가져오기
        var stats = defaultPlayerPrefab.GetComponent<PlayerStats>();
        if (stats == null)
        {
            Debug.LogWarning("[GameInitializer] 기본 프리팹에 PlayerStats가 없습니다 ❌");
            return;
        }

        var data = new PartyMemberData
        {
            id = "guma_test",
            portrait = stats.portrait,    // ✅ prefab 안의 초상화 스프라이트 사용
            prefab = defaultPlayerPrefab  // ✅ Project prefab asset 참조
        };

        PartyManager.Instance.AddMember(data);
        Debug.Log("<color=lime>[GameInitializer] 기본 파티(guma_test) 등록 완료 ✅</color>");
    }
}
