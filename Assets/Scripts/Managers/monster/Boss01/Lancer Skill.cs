using UnityEngine;

public class LancerSkill : MonoBehaviour
{
    [Header("Lancer Settings")]
    public float moveSpeed = 6f;
    public float lifeTime = 3f;
    public int damage = 10;
    public bool moveLeft = true;   // true면 왼쪽으로 이동

    private bool hasHit = false;

    private void Start()
    {
        // 일정 시간 지나면 자동 제거
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 이동
        Vector2 dir = moveLeft ? Vector2.left : Vector2.right;
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ⚔️ 플레이어와 부딪히면 데미지
        if (other.CompareTag("Player") && !hasHit)
        {
            hasHit = true;

            Damageable dmg = other.GetComponent<Damageable>();
            if (dmg != null)
            {
                Debug.Log($"[LancerSkill] Player hit for {damage} damage!");
                dmg.TakeHit(damage);
            }

            // 💥 데미지 준 뒤 바로 삭제
            Destroy(gameObject);
        }

        // ⚠️ 혹시 벽이나 맵 바깥 닿으면 제거
        if (other.CompareTag("Wall") || other.CompareTag("Boundary"))
        {
            Destroy(gameObject);
        }
    }
}
