using UnityEngine;

public class SpearProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 10f;        // 플레이어에게 줄 데미지
    public float lifetime = 3f;       // 몇 초 뒤 자동 삭제
    [Tooltip("창의 회전 각도 (0 = 기본 아래 방향)")]
    public float rotationAngle = 0f;  // Inspector에서 회전 조정 가능

    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true; // 물리 회전 고정
        }

        // 💡 설정된 각도 적용 (필요 시 -90, 90으로 테스트)
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle);

        // ⏳ lifetime 후 자동 제거
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ⚔️ 플레이어 피격 시 데미지
        if (other.CompareTag("Player"))
        {
            Damageable dmg = other.GetComponent<Damageable>();
            if (dmg != null)
            {
                Debug.Log($"[SpearProjectile] Player hit! {damage} damage.");
                dmg.TakeHit((int)damage);
            }

            Destroy(gameObject); // 플레이어 맞으면 바로 삭제
        }
    }
}
