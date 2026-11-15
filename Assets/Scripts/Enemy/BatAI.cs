
using UnityEngine;

public class BatAI : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Flying")]
    public float flySpeed = 4f;
    public float flyHeight = 0.5f;
    public float detectionRange = 15f;
    public float stoppingDistance = 3f;

    [Header("Natural Flying")]
    public float bobAmount = 0.25f;
    public float bobSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("Projectile Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackRange = 4f;
    public float attackCooldown = 2f;
    public float projectileSpeed = 8f;
    public int attackDamage = 10;

    [Header("Projectile Spawn")]
    public float forwardOffset = 0.25f;
    public float spawnHeight = 0f;

    private float nextAttackTime;
    private float randomOffset;

    void Start()
    {
        randomOffset = Random.Range(0f, 10f);

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
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

        if (distance > detectionRange)
            return;

        Vector3 direction =
            player.position - transform.position;

        FacePlayer(direction);

        float bob = Mathf.Sin(
            (Time.time + randomOffset) * bobSpeed
        ) * bobAmount;

        Vector3 horizontalDirection = direction;
        horizontalDirection.y = 0f;

        if (horizontalDirection.sqrMagnitude < 0.001f)
            horizontalDirection = transform.forward;

        Vector3 targetPosition =
            player.position
            - horizontalDirection.normalized * stoppingDistance
            + Vector3.up * flyHeight;

        targetPosition.y += bob;

        if (distance > stoppingDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                flySpeed * Time.deltaTime
            );
        }

        if (distance <= attackRange)
            ShootProjectile();
    }

    void FacePlayer(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
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
            Debug.LogError("Bat projectile references missing!");
            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        Vector3 targetPosition =
            player.position + Vector3.up * 0.5f;

        // Aim from the Fire Point towards the Player.
        Vector3 direction =
            (targetPosition - firePoint.position).normalized;

        // Spawn slightly towards the Player.
        Vector3 spawnPosition =
            firePoint.position + direction * forwardOffset;

        spawnPosition.y += spawnHeight;

        GameObject projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.LookRotation(direction)
        );

        BatProjectile movement =
            projectile.GetComponent<BatProjectile>();

        if (movement != null)
        {
            movement.speed = projectileSpeed;
            movement.damage = attackDamage;
            movement.SetTarget(targetPosition);

            Debug.Log("Bat fired projectile towards Player!");
        }
        else
        {
            Debug.LogError("BatProjectile component missing!");
            Destroy(projectile);
        }
    }
}
