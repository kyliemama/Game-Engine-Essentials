
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreeBossHealth : MonoBehaviour
{
    [Header("Tree Boss Health")]
    public int maxHealth = 150;

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

        Debug.Log("Tree Boss HP: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Tree Boss HP: " + currentHealth);

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

        Debug.Log("Tree Final Boss has been defeated!");

        Destroy(gameObject);
    }
}
                                            