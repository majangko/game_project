using UnityEngine;

public class LancerSkill : MonoBehaviour
{
    [Header("Lancer Settings")]
    public float moveSpeed = 6f;      // 이동 속도
    public float lifeTime = 3f;       // 유지 시간 (초)
    public int damage = 10;           // 데미지
    public bool moveLeft = true;      // true면 왼쪽으로 이동

    private bool hasHit = false;      // 중복 데미지 방지용

    private void Start()
    {
        // 일정 시간 후 자동 삭제
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 이동 처리
        Vector2 dir = moveLeft ? Vector2.left : Vector2.right;
        transform.Translate(dir * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 🎯 플레이어 충돌 시 데미지 적용
        if (other.CompareTag("Player") && !hasHit)
        {
            hasHit = true;

            Damageable dmg = other.GetComponent<Damageable>();
            if (dmg != null)
            {
                Debug.Log($"[LancerSkill] Player hit for {damage} damage!");
                dmg.TakeHit(damage);
            }

            // 💥 데미지 준 뒤 즉시 삭제
            Destroy(gameObject);
        }

        // ❌ 벽이나 바운더리 태그 제거
        // (시간 경과로만 삭제되도록)
    }
}
