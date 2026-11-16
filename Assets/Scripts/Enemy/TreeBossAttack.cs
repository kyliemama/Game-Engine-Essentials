
using UnityEngine;

public class TreeBossAttack : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Tree Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Attack Settings")]
    public float attackRange = 10f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 15;

    [Header("Projectile Settings")]
    public float projectileSpeed = 12f;

    [Header("Attack Sound")]
    public AudioClip attackSound;
    [Range(0f, 1f)]
    public float attackSoundVolume = 0.7f;

    private float nextAttackTime;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("Player with tag 'Player' was not found!");
            }
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= attackRange)
        {
            ShootProjectile();
        }
    }

    void ShootProjectile()
    {
        if (Time.time < nextAttackTime)
            return;

        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Tree Boss projectile references are missing!");
            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        // Play fireball sound near the camera.
        if (attackSound != null)
        {
            Vector3 soundPosition = Camera.main != null
                ? Camera.main.transform.position
                : firePoint.position;

            AudioSource.PlayClipAtPoint(
                attackSound,
                soundPosition,
                attackSoundVolume
            );

            Debug.Log("Tree Boss attack sound played!");
        }
        else
        {
            Debug.LogWarning("Tree Boss attack sound is not assigned!");
        }

        Vector3 targetPosition =
            player.position + Vector3.up * 0.5f;

        Vector3 direction =
            (targetPosition - firePoint.position).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        TreeProjectile movement =
            projectile.GetComponent<TreeProjectile>();

        if (movement != null)
        {
            movement.damage = attackDamage;
            movement.speed = projectileSpeed;
            movement.SetTarget(player);

            Debug.Log("Tree Boss attacked Player!");
        }
        else
        {
            Debug.LogError("TreeProjectile component is missing!");
            Destroy(projectile);
        }
    }
}
