using System.Collections;
using UnityEngine;
using static SealProjectile;

public class PurifySealDamage : MonoBehaviour, ISealEffect
{
    [Header("Visual Effects")]
    public GameObject visualEffect;      // 지속 결계 이펙트
    public GameObject explosionEffect;   // 폭발 이펙트 (Animator 기반)

    [Header("Damage Settings")]
    public float tickInterval = 0.5f;    // 지속 피해 간격
    public float tickDamage = 20f;       // 틱당 피해
    public float explosionDamage = 50f;  // 폭발 피해
    public float explosionRadius = 2.5f; // 폭발 범위

    [Header("Explosion Settings")]
    public float explosionLifetime = 0.6f; // 폭발 애니메이션 길이 (초 단위)

    public void Activate(Damageable target, float duration)
    {
        StartCoroutine(SealRoutine(target, duration));
    }

    private IEnumerator SealRoutine(Damageable target, float duration)
    {
        // 🔹 결계(지속) 이펙트 활성화
        if (visualEffect != null)
            visualEffect.SetActive(true);

        float elapsed = 0f;

        // 🔹 지속 대미지 루프
        while (elapsed < duration)
        {
            if (target != null)
                target.TakeHit(tickDamage);

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        // 🔹 지속 이펙트 종료 및 삭제
        if (visualEffect != null)
        {
            Destroy(visualEffect);
        }

        // 🔹 폭발 이펙트 생성 (Animator 기반)
        if (explosionEffect != null)
        {
            GameObject boom = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(boom, explosionLifetime); // 애니메이션 길이에 맞춰 자동 삭제
        }

        // 🔹 폭발 대미지 처리
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var h in hits)
        {
            Damageable d = h.GetComponent<Damageable>();
            if (d)
                d.TakeHit(explosionDamage);
        }

        // 🔹 결계 본체 삭제
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
