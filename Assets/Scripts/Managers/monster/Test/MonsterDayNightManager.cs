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
    [Tooltip("낮 → 밤 전환까지 걸리는 시간 (초 단위)")]
    public float switchInterval = 10f;
    private float timer = 0f;
    private bool isDay = true;       // true = 낮, false = 밤
    private bool hasSwitched = false; // 한 번만 전환할지 여부 제어

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
        // 이미 밤으로 전환이 끝났으면 더 이상 실행 안 함
        if (hasSwitched) return;

        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            timer = 0f;
            isDay = false; // 낮 → 밤으로 전환 (한 번만)
            hasSwitched = true; // 다시 전환 금지
            SwitchAllMonsters(isDay);
            Debug.Log("낮/밤 전환 완료 → 밤 상태로 고정됨");
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
        pair.currentMonster.tag = "Enemy";
        pair.isDead = false;
        if (TagManager.Instance != null)
            TagManager.Instance.StartCoroutine(TagManager.Instance.DelayedEnemyRegister());

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
