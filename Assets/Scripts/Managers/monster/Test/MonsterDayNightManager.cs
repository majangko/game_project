using System.Collections.Generic;
using UnityEngine;

public class DayNightMonsterManager : MonoBehaviour
{
    [System.Serializable]
    public class MonsterPair
    {
        public string pairName;               // 관리용 이름
        public GameObject humanPrefab;        // 낮 몬스터
        public GameObject orkPrefab;          // 밤 몬스터
        public Transform spawnPoint;          // 소환 위치

        [HideInInspector] public GameObject currentMonster;
        [HideInInspector] public bool isDead = false;
    }

    [Header("Settings")]
    [Tooltip("낮/밤 전환 주기 (초 단위)")]
    public float switchInterval = 10f;
    private float timer = 0f;
    private bool isDay = true; // true = 낮, false = 밤

    [Header("Monster Pairs")]
    public List<MonsterPair> monsters = new List<MonsterPair>();

    void Start()
    {
        // 시작 시 낮 몬스터 소환
        foreach (var pair in monsters)
        {
            if (pair.spawnPoint == null || pair.humanPrefab == null || pair.orkPrefab == null)
            {
                Debug.LogWarning($"⚠️ {pair.pairName} 프리팹 또는 스폰 위치가 비어 있습니다!");
                continue;
            }

            SpawnMonster(pair, isDay);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            timer = 0f;
            isDay = !isDay;
            SwitchAllMonsters(isDay);
            Debug.Log("낮/밤 전환됨 → " + (isDay ? "낮" : "밤"));
        }
    }

    void SpawnMonster(MonsterPair pair, bool spawnAsDay)
    {
        if (pair.spawnPoint == null) return;

        // 기존 몬스터 제거
        if (pair.currentMonster != null)
            Destroy(pair.currentMonster);

        GameObject prefabToSpawn = spawnAsDay ? pair.humanPrefab : pair.orkPrefab;
        pair.currentMonster = Instantiate(prefabToSpawn, pair.spawnPoint.position, Quaternion.identity);
        pair.isDead = false;

        // Damageable 이벤트 구독
        Damageable dmg = pair.currentMonster.GetComponent<Damageable>();
        if (dmg != null)
        {
            dmg.OnDeath += () =>
            {
                pair.isDead = true;
            };
        }
    }

    void SwitchAllMonsters(bool toDay)
    {
        foreach (var pair in monsters)
        {
            if (pair.isDead) continue; // 죽은 몬스터는 교체 안 함
            SwitchMonster(pair, toDay);
        }
    }

    void SwitchMonster(MonsterPair pair, bool toDay)
    {
        if (pair.currentMonster == null) return;

        Vector3 pos = pair.currentMonster.transform.position;
        Quaternion rot = pair.currentMonster.transform.rotation;

        Destroy(pair.currentMonster);

        GameObject prefabToSpawn = toDay ? pair.humanPrefab : pair.orkPrefab;
        pair.currentMonster = Instantiate(prefabToSpawn, pos, rot);

        // Damageable 이벤트 구독
        Damageable dmg = pair.currentMonster.GetComponent<Damageable>();
        if (dmg != null)
        {
            dmg.OnDeath += () =>
            {
                pair.isDead = true;
            };
        }
    }
}
