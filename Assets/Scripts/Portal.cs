using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Next Stage Settings")]
    [SerializeField] private int nextStageIndex = 1;      // 예: 1 → Stage01, Boss01
    [SerializeField] private bool isBossStage = false;    // ✅ 보스 스테이지 여부
    [SerializeField] private GameObject playerPrefab;     // guma_test 프리팹
    [SerializeField] private Sprite portrait;             // 초상화 이미지

    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log($"↑ Key Pressed! Loading next scene (Stage {nextStageIndex}, Boss: {isBossStage})");

            // ✅ 현재 플레이어 파티 등록
            RegisterPlayerParty();

            // ✅ 세이브 데이터 저장
            PlayerData.SavePlayerData();

            // ✅ SceneLoader 통해 전환
            if (SceneLoader.Instance != null)
            {
                if (isBossStage)
                    SceneLoader.Instance.LoadBoss(nextStageIndex);
                else
                    SceneLoader.Instance.LoadStage(nextStageIndex);
            }
            else
            {
                Debug.LogError("[Portal] SceneLoader.Instance가 존재하지 않습니다!");
            }
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

        Debug.Log($"[Portal] guma_test 등록 완료 → 다음 Stage: {(isBossStage ? "Boss" : "Stage")} {nextStageIndex}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("[Portal] Player entered portal zone");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("[Portal] Player left portal zone");
        }
    }
}
