using UnityEngine;

public class BossDeathPortalSpawner : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private GameObject portalPrefab;  // 포탈 프리팹
    [SerializeField] private Transform spawnPoint;     // 포탈 생성 위치
    [SerializeField] private float delay = 2f;         // 보스 사망 후 대기 시간

    [Header("Next Scene Settings")]
    [SerializeField] private bool goToTeamSelect = true; // ✅ 보스 클리어 후 팀선택 씬으로 갈지 여부
    [SerializeField] private int nextStageIndex = 2;     // 다음 스테이지 번호 (TeamSelect 아닐 경우)
    [SerializeField] private bool isBossStage = false;   // 다음 씬이 또 보스전인지 여부

    private bool _hasSpawned = false;

    // 보스 사망 시 호출
    public void OnBossDeath()
    {
        if (_hasSpawned) return;
        _hasSpawned = true;
        Invoke(nameof(SpawnPortal), delay);
    }

    private void SpawnPortal()
    {
        if (portalPrefab == null)
        {
            Debug.LogError("[BossDeathPortalSpawner] Portal prefab not assigned!");
            return;
        }

        GameObject portal = Instantiate(portalPrefab, spawnPoint.position, Quaternion.identity);

        // ✅ Portal 스크립트 세팅
        var portalScript = portal.GetComponent<Portal>();
        if (portalScript != null)
        {
            if (goToTeamSelect)
            {
                portalScript.SetGoToTeamSelect(true);  // 🎯 팀 선택 씬으로 이동
            }
            else
            {
                portalScript.SetNextStage(nextStageIndex, isBossStage); // 🎯 일반 or 보스 스테이지로 이동
            }
        }

        Debug.Log(goToTeamSelect
            ? "[BossDeathPortalSpawner] 팀 선택 씬용 포탈 생성 완료 ✅"
            : $"[BossDeathPortalSpawner] 일반 포탈 생성 완료 → Stage {nextStageIndex}");
    }
}
