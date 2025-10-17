using UnityEngine;

public class Skill_FireballZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public Vector2 areaSize = new Vector2(12f, 6f);      // Fireball이 돌아다닐 영역
    public GameObject fireballPrefab;                    // Fireball Prefab
    public float startDelay = 2f;                        // 시작 딜레이
    public Damageable bossReference;                     // 보스 Damageable
    public Transform[] spawnPoints;                      // 직접 지정한 스폰 포인트

    private void Start()
    {
        // startDelay 이후 Fireball 생성
        Invoke(nameof(SpawnFireballs), startDelay);
    }

    private void SpawnFireballs()
    {
        if (fireballPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[Skill_FireballZone] FireballPrefab 또는 SpawnPoint가 비어있음");
            return;
        }

        foreach (Transform spawn in spawnPoints)
        {
            GameObject fireball = Instantiate(fireballPrefab, spawn.position, Quaternion.identity);
            FireballBouncer fb = fireball.GetComponent<FireballBouncer>();
            if (fb != null)
            {
                fb.SetZoneArea(transform.position, areaSize);  // 🔹 가상 벽 범위 전달
                fb.SetBossReference(bossReference);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.25f);
        Gizmos.DrawCube(transform.position, areaSize);

        Gizmos.color = Color.yellow;
        if (spawnPoints != null)
        {
            foreach (Transform t in spawnPoints)
                Gizmos.DrawSphere(t.position, 0.2f);
        }
    }
}
