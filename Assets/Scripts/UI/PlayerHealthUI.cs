using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Get HP info from PlayerHP.cs
public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] Image healthFill;       
    [SerializeField] TMP_Text healthText;    
    [SerializeField] string playerTag = "Player";

    PlayerHealth playerHealth;

    void Update()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null) playerHealth = player.GetComponent<PlayerHealth>();
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
}
