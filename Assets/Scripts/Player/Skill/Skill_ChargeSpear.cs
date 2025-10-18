using UnityEngine;
using System.Collections;

/// <summary>
/// 에드가 리안의 돌진 공격 스킬 (Charge Spear)
/// 플레이어가 바라보는 방향으로 짧게 돌진하며,
/// 이동 경로상의 적에게 피해를 주는 근거리 돌진형 스킬.
/// </summary>
public class Skill_ChargeSpear : SkillBase
{
    [Header("Charge Settings")]
    [Tooltip("돌진 거리 (미터 단위)")]
    public float chargeDistance = 4.5f;

    [Tooltip("돌진 속도 (거리/초)")]
    public float chargeSpeed = 10f;

    [Tooltip("돌진 중 공격 범위 반경")]
    public float hitRadius = 0.6f;

    [Tooltip("돌진 중 적에게 입히는 피해량")]
    public float damage = 40f;

    [Tooltip("공격 판정 대상 레이어")]
    public LayerMask enemyMask;

    [Tooltip("돌진 중 충돌 감지 레이어 (벽, 장애물 등)")]
    public LayerMask obstacleMask;

    [Header("Charge Visual Effect")]
    [Tooltip("캐릭터 몸 주위에 따라다니는 이펙트")]
    public GameObject chargeEffectPrefab;

    [Header("Hitbox Settings")]
    [Tooltip("공격 중심 오프셋 (플레이어 기준 전방/후방 거리)")]
    public float hitOffset = 0.5f;

    [Tooltip("피격 시 표시되는 이펙트 (선택 사항)")]
    public GameObject hitEffectPrefab; // ✅ 추가됨

    private bool isCharging = false;

    protected override void OnActivate()
    {
        if (!isCharging)
            StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        isCharging = true;
        TriggerAnimation();

        int dir = GetFacingDir();
        float moved = 0f;

        if (ctrl != null) ctrl.canMove = false;

        // 🔹 몸 주위 이펙트 생성 및 캐릭터에 붙이기
        GameObject fx = null;
        if (chargeEffectPrefab != null)
        {
            fx = Instantiate(chargeEffectPrefab, transform.position, Quaternion.identity);
            fx.transform.SetParent(transform);
        }

        while (moved < chargeDistance)
        {
            float step = chargeSpeed * Time.deltaTime;

            // 벽 충돌 감지
            RaycastHit2D wallHit = Physics2D.Raycast(transform.position, Vector2.right * dir, step + 0.2f, obstacleMask);
            if (wallHit.collider != null)
            {
                Debug.Log($"[ChargeSpear] 벽 충돌: {wallHit.collider.name}");
                break;
            }

            // 이동
            transform.position += new Vector3(step * dir, 0f, 0f);
            moved += step;

            // ✅ 공격 중심을 오프셋 적용
            Vector2 hitCenter = (Vector2)transform.position + Vector2.right * hitOffset * dir;

            // 🔹 적 타격 판정
            Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, hitRadius, enemyMask);
            foreach (var hit in hits)
            {
                Damageable d = hit.GetComponent<Damageable>();
                if (d != null)
                {
                    d.TakeHit(damage);

                    // ✅ 피격 이펙트 생성
                    if (hitEffectPrefab != null)
                    {
                        GameObject impact = Instantiate(hitEffectPrefab, hit.transform.position, Quaternion.identity);
                        Destroy(impact, 0.4f); // 0.4초 후 자동 제거
                    }
                }
            }

            yield return null;
        }

        // 🔹 이동 복구
        if (ctrl != null) ctrl.canMove = true;

        // 🔹 몸 이펙트 제거
        if (fx != null)
            Destroy(fx, 0.3f);

        isCharging = false;
    }

    private void OnDrawGizmosSelected()
    {
        int dir = Application.isPlaying ? GetFacingDir() : 1;
        Vector3 hitCenter = transform.position + Vector3.right * hitOffset * dir;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitCenter, hitRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * chargeDistance);
    }
}
