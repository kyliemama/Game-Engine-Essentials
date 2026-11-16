
using UnityEngine;

public class BatHealth : MonoBehaviour
{
    [Header("Bat Health")]
    public int maxHealth = 50;
    private int currentHealth;

    private bool isDead = false;

    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        Debug.Log("Bat HP: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Bat HP: " + currentHealth);

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

        Debug.Log("Bat has been defeated!");

        Destroy(gameObject);
    }
}
