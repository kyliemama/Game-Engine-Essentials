
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BatHealth : MonoBehaviour, IDamageable
{
    [Header("Bat Health")]
    public int maxHealth = 50;

    private int currentHealth;
    private bool isDead = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    [Header("Health Bar UI")]
    public Image healthFill;
    public TMP_Text healthText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        Debug.Log("Bat HP: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Bat HP: " + currentHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount =
                (float)currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text =
                currentHealth + " / " + maxHealth;
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Bat has been defeated!");

        // Drop candies before destroying the Bat.
        CandyDrop candyDrop = GetComponent<CandyDrop>();

        if (candyDrop != null)
        {
            candyDrop.DropCandies();
            Debug.Log("Bat dropped candies!");
        }
        else
        {
            Debug.LogWarning("CandyDrop component is missing!");
        }

        Destroy(gameObject);
    }
}
