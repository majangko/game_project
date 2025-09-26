using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] LayerMask targetMask;
    [SerializeField] int damage = 10;
    [SerializeField] Vector2 knockback = new Vector2(2, 2);

    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false;
    }

    public void DoAttack()
    {
        StartCoroutine(EnableHitbox());
    }

    System.Collections.IEnumerator EnableHitbox()
    {
        col.enabled = true;
        yield return new WaitForSeconds(0.1f);
        col.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetMask) != 0)
        {
            Damageable dmg = other.GetComponent<Damageable>();
            if (dmg != null)
            {
                Vector2 hitPoint = other.ClosestPoint(transform.position);
                dmg.TakeHit(damage, knockback, hitPoint);
            }
        }
    }
}
