using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class LightningField : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 20;
    public float duration = 1.5f;
    public float tickInterval = 0.3f;
    public Vector2 knockback = Vector2.zero;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    private BoxCollider2D box;
    private bool isActive = false;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (!isActive)
            StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return null; // 한 프레임 대기 (Collider 등록 보장)
        StartCoroutine(DamageRoutine());
    }

    private IEnumerator DamageRoutine()
    {
        isActive = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            ApplyDamageOnce();
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        Destroy(gameObject);
    }

    private void ApplyDamageOnce()
    {
        Vector2 boxCenter = (Vector2)transform.position + box.offset;
        Vector2 boxSize = box.size;

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);

        Debug.Log($"[LightningField] 감지된 Collider 수: {hits.Length}");

        foreach (var hit in hits)
        {
            Debug.Log($" - 감지됨: {hit.name}, Layer: {LayerMask.LayerToName(hit.gameObject.layer)}, Tag: {hit.tag}");

            // ✅ Player만 감전 피해 받도록
            if (hit.CompareTag("Player"))
            {
                Damageable dmg = hit.GetComponentInParent<Damageable>();
                if (dmg != null && !dmg.IsDead())
                {
                    Vector2 hitPoint = hit.ClosestPoint(transform.position);
                    dmg.TakeHit(damageAmount, knockback, hitPoint);
                    Debug.Log($"⚡ Player에게 {damageAmount} 감전 피해 적용됨 ({hit.name})");
                }
                else
                {
                    Debug.LogWarning($"⚠️ {hit.name} 에서 Damageable 찾지 못함!");
                }
            }
        }
    }

    // 🔹 Scene 뷰 디버그 시각화
    private void OnDrawGizmos()
    {
        if (box == null)
            box = GetComponent<BoxCollider2D>();

        // 기본 Cyan 박스 (감전 범위)
        Gizmos.color = Color.cyan;
        Vector2 center = (Vector2)transform.position + box.offset;
        Gizmos.DrawWireCube(center, box.size);

        // 실제 충돌 체크용 빨간 박스
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawCube(center, box.size);
    }
}
