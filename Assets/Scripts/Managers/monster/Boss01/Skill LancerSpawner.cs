using UnityEngine;
using System.Collections;

public class Skill_LancerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject lancerPrefab;
    public int lancerCount = 3;
    public float spawnDelay = 0.3f;
    public Vector2 spawnOffset = new Vector2(0, 1f);

    [Header("Auto Activation")]
    public bool autoActivate = true;
    public float startDelay = 3f;
    public float repeatInterval = 8f;

    [Header("Warning Settings")]
    public GameObject warningPrefab;
    public float warningDuration = 1.2f;

    [Header("Custom Positions")]
    public Transform warningPoint;  // 경고 표시 위치
    public Transform spawnPoint;    // 창기병 생성 위치

    [Header("Boss Reference")]
    public Damageable bossDamageable; // 💀 보스 Damageable 연결 (죽음 감시용)

    private bool isStopped = false;

    private void Start()
    {
        if (autoActivate)
            StartCoroutine(AutoActivateRoutine());
    }

    private void Update()
    {
        // 💀 보스 사망 시 스킬 중단
        if (bossDamageable != null && bossDamageable.IsDead() && !isStopped)
        {
            Debug.Log("[LancerSpawner] Boss is dead — stopping all skill activity.");
            StopAllCoroutines();
            isStopped = true;
        }
    }

    private IEnumerator AutoActivateRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // 💀 보스 사망 시 루프 중단
            if (bossDamageable != null && bossDamageable.IsDead()) yield break;

            yield return StartCoroutine(ShowWarningAndSpawn());
            yield return new WaitForSeconds(repeatInterval);
        }
    }

    private IEnumerator ShowWarningAndSpawn()
    {
        // 💀 보스 사망 시 즉시 종료
        if (bossDamageable != null && bossDamageable.IsDead()) yield break;

        // 1️⃣ 경고 표시
        if (warningPrefab != null)
        {
            Vector3 warnPos = warningPoint != null ? warningPoint.position : transform.position;
            GameObject warn = Instantiate(warningPrefab, warnPos, Quaternion.identity);
            Destroy(warn, warningDuration);
        }

        // 2️⃣ 경고 시간 대기
        yield return new WaitForSeconds(warningDuration);

        // 3️⃣ 창기병 소환
        if (lancerPrefab != null)
            yield return StartCoroutine(SpawnLancers());
    }

    private IEnumerator SpawnLancers()
    {
        Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;

        for (int i = 0; i < lancerCount; i++)
        {
            // 💀 보스가 죽으면 소환 도중이라도 즉시 중단
            if (bossDamageable != null && bossDamageable.IsDead()) yield break;

            Vector3 pos = basePos + new Vector3(spawnOffset.x, spawnOffset.y * i, 0);
            GameObject lancer = Instantiate(lancerPrefab, pos, Quaternion.identity);

            // 왼쪽으로 돌진
            var skill = lancer.GetComponent<LancerSkill>();
            if (skill != null)
                skill.moveLeft = true;

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
