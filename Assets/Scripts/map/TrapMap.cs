using System.Collections.Generic;
using Game.Player;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D))]
public class TrapMap : MonoBehaviour
{
    [Header("Damage & Knockback")]
    public int damage = 10;
    public float knockbackTiles = 0.5f; // 1타일=1유닛(PPU=32 가정)
    public float upwardBoost = 0.2f;    // 살짝 위로 튕김
    public float hitCooldown = 0.25f;   // 동일 대상 연속 히트 간격

    [Header("Filter")]
    public string targetTag = "Player"; // 비우면 태그 무시

    private TilemapCollider2D _col;
    private readonly Dictionary<GameObject, float> _lastHit = new();

    private void Reset()
    {
        // 트리거/정적 리짓바디 자동 세팅
        _col = GetComponent<TilemapCollider2D>();
        _col.isTrigger = true;

        var rb = GetComponent<Rigidbody2D>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
    }

    private void Awake()
    {
        _col = GetComponent<TilemapCollider2D>();
        if (_col) _col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    private void OnTriggerStay2D(Collider2D other)  => TryHit(other);

    private void TryHit(Collider2D other)
    {
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
            return;

        float now = Time.time;
        if (_lastHit.TryGetValue(other.gameObject, out float last) && now - last < hitCooldown)
            return;
        _lastHit[other.gameObject] = now;

        // 데미지
        var dmg = other.GetComponent<ICanTakeDamage>();
        if (dmg != null)
            dmg.ApplyDamage(damage);

        // 넉백
        var rb = other.attachedRigidbody;
        if (rb != null)
        {
            Vector2 dir = ((Vector2)other.transform.position - (Vector2)transform.position);
            if (dir.sqrMagnitude < 1e-4f) dir = Vector2.up;
            dir = (dir.normalized + Vector2.up * upwardBoost).normalized;

            float impulse = knockbackTiles; // 1타일=1유닛 기준
            // 아래로 떨어지는 중이면 Y속도 리셋해서 반응 업
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 0f));
            rb.AddForce(dir * impulse, ForceMode2D.Impulse);
        }
    }
}
