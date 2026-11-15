
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Prevent health from going below zero.
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Player has been defeated!");
        }
    }
}
