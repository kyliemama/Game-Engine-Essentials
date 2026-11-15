
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    // Reference to the Pumpkin's health script
    public EnemyHealth enemyHealth;

    // Reference to the orange health bar image
    public Image healthFill;

    // Reference to the HP text (e.g. HP 50/50)
    public TMP_Text healthText;

    void Update()
    {
        // Check that the health and fill references are assigned
        if (enemyHealth == null || healthFill == null)
            return;

        // Calculate the remaining health as a percentage
        // Example: 25 HP / 50 HP = 0.5 (50%)
        float healthPercent =
            (float)enemyHealth.CurrentHealth / enemyHealth.maxHealth;

        // Update the orange bar based on the health percentage
        healthFill.fillAmount = healthPercent;

        // Update the text to show the current and maximum HP
        if (healthText != null)
        {
            healthText.text = "HP " +
                enemyHealth.CurrentHealth + "/" +
                enemyHealth.maxHealth;
        }
    }
}
