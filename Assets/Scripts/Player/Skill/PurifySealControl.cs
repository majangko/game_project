using System.Collections;
using UnityEngine;
using static SealProjectile;

public class PurifySealControl : MonoBehaviour, ISealEffect
{
    public GameObject visualEffect;
    public float slowAmount = 0.4f;
    public bool immobilizeSmallEnemy = true;

    public void Activate(Damageable target, float duration)
    {
        StartCoroutine(SealRoutine(target, duration));
    }

    private IEnumerator SealRoutine(Damageable target, float duration)
    {
        if (visualEffect) visualEffect.SetActive(true);

        EnemyAI ai = target.GetComponent<EnemyAI>();
        if (ai)
        {
            if (ai.isBoss)
                ai.ApplySlow(slowAmount, duration);
            else if (immobilizeSmallEnemy)
                ai.LockMovement(duration);
        }

        yield return new WaitForSeconds(duration);

        if (visualEffect) visualEffect.SetActive(false);
        Destroy(gameObject);
    }
}
