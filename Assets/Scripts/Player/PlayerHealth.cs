using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start() => currentHealth = maxHealth;

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            GameManager.Instance.OnPlayerDeath();
            SceneLoader.Instance.LoadGameOver();
        }
    }

    public void ReviveFull()
    {
        currentHealth = maxHealth;
    }
}
