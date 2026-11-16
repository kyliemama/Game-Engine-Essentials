
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Enemy Health")]
    public int maxHealth = 50;
    private int currentHealth;
    private bool isDead = false;

    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log(gameObject.name + " HP: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log(gameObject.name + " HP: " + currentHealth);

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

        Debug.Log(gameObject.name + " has been defeated!");

        CandyDrop candyDrop = GetComponent<CandyDrop>();

        if (candyDrop != null)
        {
            candyDrop.DropCandies();
        }

        Destroy(gameObject);
    }
}
