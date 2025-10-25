using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Next Scene Settings")]
    [SerializeField] private int nextStageIndex = 1;  // 예: Stage01 → 1, Stage02 → 2
    [SerializeField] private GameObject playerPrefab; // guma_test 프리팹
    [SerializeField] private Sprite portrait;         // 초상화 이미지

    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("↑ Key Pressed! Loading next scene...");

            // ✅ 현재 플레이어 파티 등록
            RegisterPlayerParty();

            // ✅ 기존 세이브 로직 호출 (필요시)
            PlayerData.SavePlayerData();

            // ✅ SceneLoader 통해 스테이지 로드
            SceneLoader.Instance.LoadStage(nextStageIndex);
        }
    }

    private void RegisterPlayerParty()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogError("[Portal] PartyManager가 존재하지 않습니다!");
            return;
        }

        var data = new PartyMemberData
        {
            id = "guma_test",
            portrait = portrait,
            prefab = playerPrefab
        };

        PartyManager.Instance.ClearParty();
        PartyManager.Instance.AddMember(data);

        Debug.Log($"[Portal] guma_test 등록 완료 → 다음 Stage: {nextStageIndex}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player entered portal zone");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player left portal zone");
        }
    }
}
