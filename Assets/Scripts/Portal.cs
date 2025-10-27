using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Next Stage Settings")]
    [SerializeField] private int nextStageIndex = 1;       // 예: 1 → Stage01, Boss01
    [SerializeField] private bool isBossStage = false;     // ✅ 보스 스테이지 여부
    [SerializeField] private bool goToTeamSelect = false;  // ✅ 팀선택 씬으로 이동 여부
    [SerializeField] private GameObject playerPrefab;      // guma_test 프리팹
    [SerializeField] private Sprite portrait;              // 초상화 이미지

    private bool isPlayerInRange = false;

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log($"↑ Key Pressed! Portal activated → Stage:{nextStageIndex}, Boss:{isBossStage}, TeamSelect:{goToTeamSelect}");
            PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name); // ✅ 최근 씬 저장

            // ✅ 플레이어 파티 등록 (중복 방지, 초기화 금지)
            RegisterPlayerParty();

            // ✅ 세이브 데이터 저장
            PlayerData.SavePlayerData();

            // ✅ 이동 처리
            if (goToTeamSelect)
            {
                Debug.Log("<color=cyan>[Portal] Loading TeamSelect scene...</color>");
                SceneManager.LoadScene("TeamSelect UI");
                return;
            }

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

    /// <summary>
    /// 현재 플레이어(guma_test)를 PartyManager에 등록 (중복 방지, ClearParty 제거)
    /// </summary>
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

        // ✅ ClearParty() 제거 → 기존 파티 유지
        // ✅ 이미 같은 id가 있으면 중복 추가하지 않음
        var members = PartyManager.Instance.GetAllMembers();
        bool alreadyInParty = members.Exists(m => m != null && m.id == data.id);

        if (!alreadyInParty)
        {
            PartyManager.Instance.AddMember(data);
            Debug.Log($"[Portal] guma_test 파티에 새로 등록됨 ✅");
        }
        else
        {
            Debug.Log($"[Portal] guma_test 이미 파티에 존재 → 중복 추가 생략 ✅");
        }

        Debug.Log($"[Portal] 현재 파티 인원 수: {PartyManager.Instance.GetCount()}");
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

    // ---------------- Setter 함수 ----------------
    public void SetNextStage(int index, bool boss)
    {
        nextStageIndex = index;
        isBossStage = boss;
    }

    public void SetGoToTeamSelect(bool value)
    {
        goToTeamSelect = value;
    }
}
