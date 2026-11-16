using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shows the Player's HP from PlayerHealth.cs. Finds the Player automatically, including after a scene change.
public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] Image healthFill;
    [SerializeField] TMP_Text healthText;
    [SerializeField] string playerTag = "Player";   // kept for older scenes; PlayerLocator also checks this tag

    PlayerHealth playerHealth;
    float nextSearchTime;

    void Update()
    {
        if (playerHealth == null)
        {
            if (Time.unscaledTime < nextSearchTime) return;
            nextSearchTime = Time.unscaledTime + 0.5f;

            playerHealth = FindPlayerHealth();
            if (playerHealth == null) return;
        }

        if (healthFill != null && playerHealth.MaxHealth > 0)
        {
            healthFill.fillAmount = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
        }

        if (healthText != null)
        {
            healthText.text = "HP: " + playerHealth.CurrentHealth + "/" + playerHealth.MaxHealth;
        }
    }

    PlayerHealth FindPlayerHealth()
    {
        Transform player = PlayerLocator.Find();
        if (player == null)
        {
            GameObject tagged = GameObject.FindGameObjectWithTag(playerTag);
            if (tagged != null) player = tagged.transform;
        }

        PlayerHealth found = null;
        if (player != null)
        {
            found = player.GetComponentInParent<PlayerHealth>();
            if (found == null) found = player.GetComponentInChildren<PlayerHealth>();
        }

        if (found == null) found = Object.FindFirstObjectByType<PlayerHealth>();
        return found;
    }
}
