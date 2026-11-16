
using UnityEngine;

public class TreeBossAttack : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Tree Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f;
    public int attackDamage = 20;

    [Header("Projectile Settings")]
    public float projectileSpeed = 12f;

    private float nextAttackTime;

    void Start()
    {
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

        ShootProjectile();
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

            Debug.Log("Tree Boss fired a fireball!");
        }
        else
        {
            Debug.LogError("TreeProjectile component is missing!");
            Destroy(projectile);
        }
    }
}
