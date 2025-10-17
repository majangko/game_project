using UnityEngine;

public class EnergyBolt : MonoBehaviour
{
    [Header("Bolt Settings")]
    [SerializeField] private int damage = 12;              // 데미지
    [SerializeField] private float speed = 7f;             // 이동 속도
    [SerializeField] private float lifeTime = 4f;          // 수명 (Inspector에서 조정 가능)
    [SerializeField] private float splitDistance = 6f;     // 분열 거리
    [SerializeField] private int splitCount = 5;           // 분열 개수
    [SerializeField] private float splitAngle = 15f;       // 분열 시 각 볼트 간 각도 차이
    [SerializeField] private GameObject boltPrefab;        // 분열 시 생성될 볼트 (자기 자신 or 동일 프리팹)
    [SerializeField] private string targetTag = "Player";  // 🎯 공격 대상 태그

    private Rigidbody2D rb;
    private Vector2 direction;
    private Vector3 startPos;
    private bool hasSplit = false;
    private Transform target; // 자동 탐색용

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPos = transform.position;

        // 🎯 Player 자동 탐색
        GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);
        if (playerObj != null)
            target = playerObj.transform;

        // 이동 시작
        if (rb != null && direction != Vector2.zero)
            rb.linearVelocity = direction * speed;

        // 일정 시간 후 자동 소멸
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// 외부(MageBossAI)에서 방향 지정
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        if (rb != null)
            rb.linearVelocity = direction * speed;

        // 방향에 맞춰 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        // 일정 거리 이상 이동 시 분열 (1회만)
        if (!hasSplit && Vector2.Distance(startPos, transform.position) >= splitDistance)
        {
            hasSplit = true;
            Split();
        }
    }

    /// <summary>
    /// 분열 처리
    /// </summary>
    private void Split()
    {
        if (boltPrefab == null) return;

        for (int i = 0; i < splitCount; i++)
        {
            float baseAngle = -((splitCount - 1) / 2f) * splitAngle;
            float angle = baseAngle + (i * splitAngle);

            Vector2 newDir = Quaternion.Euler(0, 0, angle) * direction;

            GameObject newBolt = Instantiate(boltPrefab, transform.position, Quaternion.identity);

            EnergyBolt bolt = newBolt.GetComponent<EnergyBolt>();
            if (bolt != null)
            {
                bolt.SetDirection(newDir);

                // ⚠️ 분열된 볼트는 다시 분열하지 않게 설정
                bolt.splitDistance = 0f;
                bolt.hasSplit = true;
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 🎯 Player 자동 감지
        if (collision.CompareTag(targetTag))
        {
            Damageable dmg = collision.GetComponent<Damageable>();
            if (dmg != null)
            {
                dmg.TakeHit(damage, Vector2.zero, transform.position);
                Debug.Log($"[EnergyBolt] Player hit! Damage: {damage}");
            }

            // 💥 맞으면 즉시 제거
            Destroy(gameObject);
        }
    }
}
