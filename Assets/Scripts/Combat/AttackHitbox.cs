using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AttackHitbox : MonoBehaviour
{
    [SerializeField] LayerMask targetMask;       // Player
    [SerializeField] int damage = 10;
    [SerializeField] Vector2 knockback = new Vector2(2, 2);

    BoxCollider2D col;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;                    // 반드시 트리거
        col.enabled = false;                     // 기본은 꺼두기
    }

    // << 없어서 에러났던 메서드
    public void EnableOnce(float seconds)
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(EnableRoutine(seconds));
    }

    IEnumerator EnableRoutine(float seconds)
    {
        col.enabled = true;
        yield return new WaitForSeconds(seconds);
        col.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetMask) == 0) return;

        var dmg = other.GetComponent<Damageable>();
        if (dmg != null)
        {
            float dir = Mathf.Sign(transform.lossyScale.x);
            Vector2 kb = new Vector2(knockback.x * dir, knockback.y);
            dmg.TakeHit(damage, kb, transform.position);
        }
    }

    // 필요시 인스펙터에서 바꿔 끼우기 쉽게
    public void SetTargetMask(LayerMask mask) => targetMask = mask;
}
