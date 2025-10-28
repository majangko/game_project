using UnityEngine;

public class YellowLightningBolt : MonoBehaviour
{
    [Header("Bolt Settings")]
    [SerializeField] private int damage = 15;
    [SerializeField] private float speed = 9f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float splitDistance = 5f;
    [SerializeField] private int splitCount = 3;
    [SerializeField] private float splitAngle = 20f;
    [SerializeField] private GameObject boltPrefab; // 자기 자신 프리팹 연결
    [SerializeField] private string targetTag = "Player";

    private Rigidbody2D rb;
    private Vector2 direction;
    private Vector3 startPos;
    private bool hasSplit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPos = transform.position;
        rb.linearVelocity = direction * speed;
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        if (!hasSplit && Vector2.Distance(startPos, transform.position) >= splitDistance)
        {
            hasSplit = true;
            Split();
        }
    }

    private void Split()
    {
        if (boltPrefab == null) return;

        for (int i = 0; i < splitCount; i++)
        {
            float baseAngle = -((splitCount - 1) / 2f) * splitAngle;
            float angle = baseAngle + (i * splitAngle);

            Vector2 newDir = Quaternion.Euler(0, 0, angle) * direction;

            GameObject newBolt = Instantiate(boltPrefab, transform.position, Quaternion.identity);
            YellowLightningBolt bolt = newBolt.GetComponent<YellowLightningBolt>();
            if (bolt != null)
            {
                bolt.SetDirection(newDir);
                bolt.hasSplit = true;
                bolt.splitDistance = 0; // 분열된 건 추가 분열 금지
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            Damageable dmg = collision.GetComponent<Damageable>();
            if (dmg != null)
            {
                dmg.TakeHit(damage, Vector2.zero, transform.position);
                Debug.Log($"⚡ Player hit by Yellow Lightning Bolt! Damage: {damage}");
            }

            Destroy(gameObject);
        }
    }
}
