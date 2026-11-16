
using UnityEngine;

// Put on the Player root (the object with PlayerMovement). Enemy projectiles damage this,
// PlayerHealthUI shows it, and reaching 0 HP loses the game.
public class PlayerHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public bool logDamage = true;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0) return;

        currentHealth -= amount;   // minus damage
        if (currentHealth < 0) currentHealth = 0;

        if (logDamage)
            Debug.Log($"Player took {amount} damage, HP {currentHealth}/{maxHealth}", this);

        if (currentHealth <= 0)
        {
            GameEnding.LoseGame();
        }
    }

    // Restores HP, up to maxHealth. Used by eating candy.
    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}
