
using UnityEngine;

public class TreeBossHealth : MonoBehaviour
{
    [Header("Tree Boss Health")]
    public int maxHealth = 150;

    private int currentHealth;
    private bool isDead = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Start()
    {
        currentHealth = maxHealth;

        Debug.Log("Tree Boss HP: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Tree Boss HP: " + currentHealth);

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

        Debug.Log("Tree Final Boss has been defeated!");

        Destroy(gameObject);
    }
}
