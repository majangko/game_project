using UnityEngine;

public class AttackZone : MonoBehaviour
{
    public GameObject hitEffectPrefab;  // Inspector에서 연결

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 타격 이펙트 생성
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, other.transform.position, Quaternion.identity);
            }

            // 디버그용 로그
            Debug.Log("적에게 공격 성공!");
        }
    }
}
