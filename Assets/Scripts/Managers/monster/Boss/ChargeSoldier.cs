using UnityEngine;

public class ChargeSoldierGroup : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;     // 이동 속도
    [SerializeField] private float lifeTime = 5f;      // 몇 초 뒤 사라짐
    [SerializeField] private float yOffset = 0.3f;     // 약간 위로 띄우기 (보스 발밑 보정)

    private void Start()
    {
        // 위치 보정
        transform.position += new Vector3(0, yOffset, 0);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // 방향 결정: 화면 중앙 기준
        float dir = 0f;
        float screenMid = Camera.main.transform.position.x;

        if (transform.position.x < screenMid)
        {
            dir = 1f; // 왼쪽 → 오른쪽 이동
        }
        else
        {
            dir = -1f; // 오른쪽 → 왼쪽 이동
            // Flip 처리 (자식 Sprite들이 반대 방향 보게)
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            {
                sr.flipX = true;
            }
        }

        // 이동 적용
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveSpeed * dir, 0);
        }
        else
        {
            foreach (var childRb in GetComponentsInChildren<Rigidbody2D>())
            {
                childRb.linearVelocity = new Vector2(moveSpeed * dir, 0);
            }
        }

        // 수명 끝나면 제거
        Destroy(gameObject, lifeTime);
    }
}
