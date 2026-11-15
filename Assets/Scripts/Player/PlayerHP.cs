
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log("Player HP: " + currentHealth);
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
