using UnityEngine;

public class AttackZone : MonoBehaviour
{
    public GameObject hitEffectPrefab;  // Inspector���� ����

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Ÿ�� ����Ʈ ����
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, other.transform.position, Quaternion.identity);
            }

            // ����׿� �α�
            Debug.Log("������ ���� ����!");
        }
    }
}
