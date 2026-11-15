
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;
    private bool isDead = false;

    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log("Pumpkin Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Pumpkin has been defeated!");

        // Drop candies before destroying the Pumpkin
        CandyDrop candyDrop = GetComponent<CandyDrop>();

        if (candyDrop != null)
        {
            candyDrop.DropCandies();
        }
        else
        {
            Debug.LogWarning("CandyDrop component is missing!");
        }

        Destroy(gameObject);
    }
}
