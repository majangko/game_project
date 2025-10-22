using UnityEngine;
using System.Collections;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    public int hp;
    public int mp;
    public Vector3 lastPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🔹 현재 플레이어 정보 저장 (씬 이동 직전 호출)
    public static void SavePlayerData()
    {
        if (Instance == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Instance.lastPosition = player.transform.position;

            var stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                Instance.hp = stats.HP;
                Instance.mp = stats.MP;
            }

            Debug.Log($"[PlayerData] Save: pos={Instance.lastPosition}, HP={Instance.hp}, MP={Instance.mp}");
        }
    }

    // 🔹 새 씬 로드 후 데이터 복원
    public static void LoadPlayerData()
    {
        if (Instance == null)
        {
            Debug.LogWarning("[PlayerData] Instance not found!");
            return;
        }

        // 씬 로드 후 일정 시간 대기 (씬 안정화)
        Instance.StartCoroutine(Instance.LoadPlayerDataDelayed());
    }

    // 🔸 일정 시간 지연 후 스폰포인트로 이동
    private IEnumerator LoadPlayerDataDelayed()
    {
        yield return new WaitForSeconds(0.1f); // 씬 로딩 안정화 대기

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[PlayerData] Player not found in scene!");
            yield break;
        }

        // 🔹 PlayerSpawnPoints 탐색
        GameObject spawnPoint = GameObject.Find("PlayerSpawnPoints");
        if (spawnPoint != null)
        {
            player.transform.position = spawnPoint.transform.position;
            Debug.Log($"[PlayerData] Player moved to spawn point: {spawnPoint.transform.position}");
        }
        else
        {
            // 스폰포인트 없으면 이전 위치 사용
            player.transform.position = Instance.lastPosition;
            Debug.Log($"[PlayerData] No spawn point found, restored last position: {Instance.lastPosition}");
        }

        // 🔹 HP / MP 복원
        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.SetHPMP(Instance.hp, Instance.mp);
            Debug.Log($"[PlayerData] Player stats restored (HP: {Instance.hp}, MP: {Instance.mp})");
        }
        else
        {
            Debug.LogWarning("[PlayerData] PlayerStats component not found!");
        }
    }
}
