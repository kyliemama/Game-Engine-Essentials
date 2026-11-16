
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

    [Header("Line of Sight")]
    [Tooltip("Only attack when nothing solid (walls, floors) is between the boss and the Player.")]
    public bool requireLineOfSight = true;
    [Tooltip("Ignore the Player if they are more than this far above or below (on another floor).")]
    public float maxHeightDifference = 3f;

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
                PlayerLocator.Find()?.gameObject;

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
        // Find the Player if it isn't assigned (or was destroyed / replaced).
        player = PlayerLocator.Resolve(player);

        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        Vector3 eye = firePoint ? firePoint.position : transform.position + Vector3.up;

        if (distance <= attackRange &&
            (!requireLineOfSight ||
             PlayerLocator.HasLineOfSight(eye, player, transform, maxHeightDifference)))
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

            if (AudioManager.instance != null) AudioManager.instance.PlayClip(attackSound, attackSoundVolume);

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
            movement.owner = transform;
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
