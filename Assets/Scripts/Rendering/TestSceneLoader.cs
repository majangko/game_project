using UnityEngine;

public class TestSceneLoader : MonoBehaviour
{
    void Start()
    {
        // PartyManager가 살아있는지 확인
        if (PartyManager.Instance == null)
        {
            Debug.LogError("❌ PartyManager.Instance가 없습니다!");
            return;
        }

        // TagManager 찾기
        var tagManager = FindObjectOfType<TagManager>();
        if (tagManager == null)
        {
            Debug.LogError("❌ TagManager를 찾을 수 없습니다!");
            return;
        }

        // 파티 구성원 로그 출력
        var list = PartyManager.Instance.GetAllMembers();
        Debug.Log($"[test_ts] 현재 파티원 수: {list.Count}");
        foreach (var m in list)
            Debug.Log($"[test_ts] 파티원: {m.id}");

    }
}
