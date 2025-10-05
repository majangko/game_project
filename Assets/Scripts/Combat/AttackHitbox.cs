using UnityEngine;
using System.Collections;

public class AttackHitbox : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private LayerMask targetMask; // 공격 대상 (예: Player, Enemy)

    [Header("Attack Stats")]
    [SerializeField] private int damage = 10;
    [SerializeField] private Vector2 knockback = new Vector2(2, 2);
    [SerializeField] private float activeTime = 0.1f;
    [SerializeField] private Vector2 boxSize = new Vector2(1.5f, 1f);
    [SerializeField] private Vector2 boxOffset = new Vector2(1f, 0f);

    private Transform owner;
    private float lastFacingDir = 1f;

    private void Awake()
    {
        // 부모 중 "Enemy" 또는 "Player" 태그 가진 객체 탐색
        Transform t = transform;
        while (t != null && t.parent != null)
        {
            if (t.CompareTag("Player") || t.CompareTag("Enemy"))
            {
                owner = t;
                break;
            }
            t = t.parent;
        }

        if (owner == null)
        {
            Debug.LogWarning($"[Hitbox] 부모에 Player/Enemy 태그가 없어 root로 설정됨 ({name})");
            owner = transform.root;
        }
    }

    private void Update()
    {
        // localScale.x 기준 방향 추적
        if (owner != null)
        {
            lastFacingDir = Mathf.Sign(owner.localScale.x);
        }
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출 (공격 타이밍)
    /// </summary>
    public void DoAttack()
    {
        Debug.Log($"[Hitbox] 공격 시작 → {gameObject.name}");
        StartCoroutine(PerformAttack());
    }

    private IEnumerator PerformAttack()
    {
        DetectAndHitTargets();
        yield return new WaitForSeconds(activeTime);
    }

    private void DetectAndHitTargets()
    {
        if (owner == null) return;

        // 🧭 방향 계산
        float facingDir = Mathf.Sign(owner.localScale.x);

        // ✅ 로컬 기준 offset을 월드 좌표로 변환
        Vector2 localOffset = new Vector2(boxOffset.x * facingDir, boxOffset.y);
        Vector2 hitCenter = transform.TransformPoint(localOffset);

        // 🎯 공격 판정
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitCenter, boxSize, 0f, targetMask);
        Debug.Log($"[Hitbox] 감지된 대상 수: {hits.Length}");

        foreach (Collider2D hit in hits)
        {
            if (hit.transform.root == owner) continue;

            Damageable dmg = hit.GetComponentInParent<Damageable>();
            if (dmg == null) continue;

            Vector2 hitPoint = hit.ClosestPoint(hitCenter);
            Vector2 dir = (hit.transform.position - owner.position).normalized;
            Vector2 finalKnockback = new Vector2(knockback.x * dir.x, knockback.y);

            dmg.TakeHit(damage, finalKnockback, hitPoint);
            Debug.Log($"[Hitbox] ✅ {hit.name}에게 {damage} 데미지 적용!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 에디터에서도 방향 인식 유지
        if (owner == null)
        {
            Transform t = transform;
            while (t != null && t.parent != null)
            {
                if (t.CompareTag("Player") || t.CompareTag("Enemy"))
                {
                    owner = t;
                    break;
                }
                t = t.parent;
            }
            if (owner == null) owner = transform.root;
        }

        // 🧭 Flip 방향 계산
        float facingDir = owner ? Mathf.Sign(owner.localScale.x) : lastFacingDir;

        // ✅ TransformPoint를 사용해 로컬 offset을 월드 좌표로 변환
        Vector2 localOffset = new Vector2(boxOffset.x * facingDir, boxOffset.y);
        Vector2 hitCenter = transform.TransformPoint(localOffset);

        // 🟥 실제 공격 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(hitCenter, boxSize);
    }
}
