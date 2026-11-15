
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;

    private int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;   // minus damage
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Die();
        }
    }   
}
