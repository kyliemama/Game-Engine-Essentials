
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GhostBossHealth : MonoBehaviour
{
    [Header("Ghost Health")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Health Bar UI")]
    public Image healthFill;
    public TMP_Text healthText;

    [Header("Death Rewards")]
    public GameObject deathEffect;
    public GameObject lollipopPrefab;

    [Header("Lollipop Spawn Sound")]
    public AudioClip lollipopSpawnSound;
    [Range(0f, 1f)]
    public float lollipopSoundVolume = 1f;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHealthBar();

        Debug.Log("Ghost HP: " + currentHealth + "/" + maxHealth);

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
                "HP " + currentHealth + "/" + maxHealth;
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log("Ghost Mini-Boss defeated!");

        // Spawn blue death effect
        GameObject effect = null;

        if (deathEffect != null)
        {
            effect = Instantiate(
                deathEffect,
                transform.position,
                Quaternion.identity
            );
        }

        // Spawn lollipop in the centre of the effect
        Vector3 spawnPosition =
            transform.position + Vector3.up * 1f;

        if (lollipopPrefab != null)
        {
            GameObject lollipop = Instantiate(
                lollipopPrefab,
                spawnPosition,
                Quaternion.identity
            );

            // Play magical sound when lollipop appears
            if (lollipopSpawnSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    lollipopSpawnSound,
                    spawnPosition,
                    lollipopSoundVolume
                );
            }

            // Connect lollipop to the death effect
            LollipopKeyPickup pickup =
                lollipop.GetComponent<LollipopKeyPickup>();

            if (pickup != null)
            {
                pickup.SetDeathEffect(effect);
            }
        }
        else
        {
            Debug.LogWarning("Lollipop prefab is not assigned!");
        }

        // Remove Ghost
        Destroy(gameObject);
    }
}
