
using UnityEngine;

public class GhostBossAttack : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Ghost Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Attack Settings")]
    public float attackRange = 10f;
    public float attackCooldown = 2f;
    public int attackDamage = 15;

    [Header("Rotation Settings")]
    public float rotationSpeed = 180f;
    public float modelYawOffset = 90f;

    [Header("Projectile Settings")]
    public float horizontalSpacing = 0.15f;

    private float nextAttackTime;
    private Vector3 originalRotation;

    void Start()
    {
        originalRotation = transform.eulerAngles;
    }

    void Update()
    {
        if (player == null)
            return;

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distanceToPlayer <= attackRange)
        {
            RotateTowardsPlayer();
            ShootProjectile();
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        float targetYaw = Mathf.Atan2(
            direction.x,
            direction.z
        ) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(
            originalRotation.x,
            targetYaw + modelYawOffset,
            originalRotation.z
        );

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void ShootProjectile()
    {
        if (Time.time < nextAttackTime)
            return;

        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Ghost projectile references are missing!");
            return;
        }

        Vector3 targetPosition = player.position + Vector3.up * 0.5f;

        // Create two horizontal spawn positions.
        Vector3 leftSpawn = firePoint.position -
                            firePoint.right * horizontalSpacing;

        Vector3 rightSpawn = firePoint.position +
                             firePoint.right * horizontalSpacing;

        // Both projectiles aim directly at the Player.
        SpawnProjectile(leftSpawn, targetPosition);
        SpawnProjectile(rightSpawn, targetPosition);

        nextAttackTime = Time.time + attackCooldown;

        Debug.Log("Ghost fired two projectiles towards the Player!");
    }

    void SpawnProjectile(Vector3 spawnPosition, Vector3 targetPosition)
    {
        Vector3 direction = (
            targetPosition - spawnPosition
        ).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        GhostProjectile movement =
            projectile.GetComponent<GhostProjectile>();

        if (movement != null)
        {
            movement.damage = attackDamage;
            movement.SetDirection(direction);
        }
        else
        {
            Debug.LogError("GhostProjectile component is missing!");
            Destroy(projectile);
        }
    }
}
