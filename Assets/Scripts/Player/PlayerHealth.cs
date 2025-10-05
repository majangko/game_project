using UnityEngine;
using Game.Player;
public class PlayerHealth : MonoBehaviour, ICanTakeDamage
{
    public int maxHealth = 100;

    [SerializeField] private int currentHealth;

    // (선택) 무적 시간 등 필요 시 확장 가능
    // [SerializeField] private float invincibleTime = 0f;
    // private float lastHitTime = -999f;

    void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// TrapDamageTilemap 등에서 호출하는 표준 데미지 엔트리
    /// </summary>
    public void ApplyDamage(int amount)
    {
        TakeDamage(amount);
    }

    public void TakeDamage(int amount)
    {
        // (선택) 무적 처리 예시
        // if (Time.time - lastHitTime < invincibleTime) return;
        // lastHitTime = Time.time;

        currentHealth -= Mathf.Max(0, amount);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // 싱글턴이 null일 경우 방어
            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerDeath();

            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadGameOver();
            return;
        }

        // (선택) 피격 시 연출/이펙트/사운드 트리거 자리
        // e.g., animator.SetTrigger("Hit");
        //       Audio.Play("HitSfx");
    }

    public void ReviveFull()
    {
        currentHealth = maxHealth;
        // (선택) 부활 연출 자리
    }

    // (선택) 현재 체력 외부 노출
    public int CurrentHealth => currentHealth;

    // (선택) 회복 함수
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + Mathf.Max(0, amount), 0, maxHealth);
    }
}
