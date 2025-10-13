using UnityEngine;
using System.Collections;

public class Skill_SpearRainSpawner : MonoBehaviour
{
    [Header("Spear Settings")]
    public GameObject spearPrefab;        // 낙하할 창 프리팹
    public GameObject warningPrefab;      // ⚠️ 경고 표시 프리팹
    public float spawnHeight = 10f;       // 창이 생성되는 높이
    public int spearCount = 5;            // 한 번에 떨어질 창 개수
    public float repeatInterval = 5f;     // 반복 주기
    public float spawnAreaWidth = 12f;    // 낙하 범위
    public float startDelay = 2f;         // 첫 시작 딜레이
    public float spearAngle = 0f;         // 창 회전 각도

    [Header("Warning Settings")]
    public float warningDuration = 0.5f;  // ⚠️ 경고 표시 유지 시간 (초)

    [Header("Boss Reference")]
    public Damageable bossDamageable;

    private bool isStopped = false;

    private void Start()
    {
        StartCoroutine(SpearRainRoutine());
    }

    private void Update()
    {
        if (bossDamageable != null && bossDamageable.IsDead() && !isStopped)
        {
            StopAllCoroutines();
            isStopped = true;
            Debug.Log("[SpearRainSpawner] Boss dead — stop raining spears.");
        }
    }

    private IEnumerator SpearRainRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            if (bossDamageable != null && bossDamageable.IsDead())
                yield break;

            for (int i = 0; i < spearCount; i++)
            {
                // 🎯 랜덤 위치 계산
                float randomX = transform.position.x + Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
                Vector3 warningPos = new Vector3(randomX, transform.position.y, 0);
                Vector3 spearPos = new Vector3(randomX, transform.position.y + spawnHeight, 0);

                // ⚠️ 경고 프리팹 생성
                if (warningPrefab != null)
                {
                    GameObject warn = Instantiate(warningPrefab, warningPos, Quaternion.identity);
                    Destroy(warn, warningDuration); // 지정 시간 뒤 제거
                }

                // 🕑 일정 시간 뒤 창 생성
                StartCoroutine(SpawnSpearAfterDelay(spearPos, warningDuration));

                yield return new WaitForSeconds(0.15f); // 창 간격
            }

            yield return new WaitForSeconds(repeatInterval);
        }
    }

    private IEnumerator SpawnSpearAfterDelay(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 💥 창 소환
        GameObject spear = Instantiate(spearPrefab, position, Quaternion.identity);
        SpearProjectile spearComp = spear.GetComponent<SpearProjectile>();
        if (spearComp != null)
            spearComp.rotationAngle = spearAngle;
    }

#if UNITY_EDITOR
    // 🎨 Scene 뷰에서 낙하 범위 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Vector3 leftEdge = new Vector3(transform.position.x - spawnAreaWidth / 2f, transform.position.y + spawnHeight, 0);
        Vector3 rightEdge = new Vector3(transform.position.x + spawnAreaWidth / 2f, transform.position.y + spawnHeight, 0);

        Gizmos.DrawLine(leftEdge, rightEdge);
        Gizmos.DrawLine(leftEdge, new Vector3(leftEdge.x, leftEdge.y - 15, 0));
        Gizmos.DrawLine(rightEdge, new Vector3(rightEdge.x, rightEdge.y - 15, 0));
    }
#endif
}
