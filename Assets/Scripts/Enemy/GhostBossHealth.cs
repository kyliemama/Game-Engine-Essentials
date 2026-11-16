
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GhostBossHealth : MonoBehaviour, IDamageable
{
    [Header("Ghost Health")]
    public int maxHealth = 100;
    private int currentHealth;

    // Allow other scripts to read the Ghost's HP.
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

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

    void Awake()
    {
        // The MiniBoss_Ghost prefab has a second GhostBossHealth on its health bar canvas. That copy
        // never gets hit, so the bar never moved. Hand its bar to the real one (on the ghost) and remove it.
        GhostBossHealth owner = transform.parent
            ? transform.parent.GetComponentInParent<GhostBossHealth>()
            : null;

        if (owner != null)
        {
            if (owner.healthFill == null) owner.healthFill = healthFill;
            if (owner.healthText == null) owner.healthText = healthText;
            Destroy(this);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
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
        if (isDead) return;

        isDead = true;

        Debug.Log("Ghost Mini-Boss defeated!");

        GameObject effect = null;

        if (deathEffect != null)
        {
            effect = Instantiate(
                deathEffect,
                transform.position,
                Quaternion.identity
            );
        }

        Vector3 spawnPosition =
            transform.position + Vector3.up * 1f;

        if (lollipopPrefab != null)
        {
            GameObject lollipop = Instantiate(
                lollipopPrefab,
                spawnPosition,
                Quaternion.identity
            );

            if (lollipopSpawnSound != null)
            {
                AudioSource.PlayClipAtPoint(
                    lollipopSpawnSound,
                    spawnPosition,
                    lollipopSoundVolume
                );
            }

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

        Destroy(gameObject);
    }
}
