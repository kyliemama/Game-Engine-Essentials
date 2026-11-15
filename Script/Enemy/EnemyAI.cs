
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Patrol Settings")]
    public float patrolRadius = 4f;
    public float patrolSpeed = 1.5f;
    public float patrolWaitTime = 2f;

    [Header("Territory Settings")]
    public float territoryRadius = 8f;
    public float detectionRange = 6f;
    public float attackRange = 1.5f;

    [Header("Chase Settings")]
    public float chaseSpeed = 2f;

    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 2f;

    [Header("Animation")]
    public Animator animator;

    [Header("Fireball Attack")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    private NavMeshAgent agent;
    private Vector3 patrolCentre;

    private float nextAttackTime;
    private float nextPatrolTime;
    private bool wasChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent missing or not on NavMesh!");
            enabled = false;
            return;
        }

        patrolCentre = transform.position;
        agent.speed = patrolSpeed;
    }

    void Update()
    {
        if (player == null)
        {
            Patrol();
            return;
        }

        if (!agent.isOnNavMesh)
            return;

        float distanceToPlayer = Vector3.Distance(
            transform.position,
            player.position
        );

        float playerDistanceFromCentre = Vector3.Distance(
            new Vector3(player.position.x, 0f, player.position.z),
            new Vector3(patrolCentre.x, 0f, patrolCentre.z)
        );

        // Player is outside the Pumpkin's territory.
        if (playerDistanceFromCentre > territoryRadius)
        {
            if (wasChasing)
            {
                agent.ResetPath();
                wasChasing = false;
                nextPatrolTime = Time.time;
            }

            Patrol();
            return;
        }

        // Player is outside detection range.
        if (distanceToPlayer > detectionRange)
        {
            if (wasChasing)
            {
                agent.ResetPath();
                wasChasing = false;
                nextPatrolTime = Time.time;
            }

            Patrol();
            return;
        }

        // Shoot repeatedly while the Player is detected.
        ShootFireball();

        // Keep the original movement behaviour.
        if (distanceToPlayer <= attackRange)
        {
            wasChasing = false;
            AttackPlayer();
        }
        else
        {
            wasChasing = true;
            ChasePlayer();
        }
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending &&
            (!agent.hasPath || agent.remainingDistance <= 0.5f) &&
            Time.time >= nextPatrolTime)
        {
            Vector2 randomPoint =
                Random.insideUnitCircle * patrolRadius;

            Vector3 randomPosition = patrolCentre +
                new Vector3(randomPoint.x, 0f, randomPoint.y);

            if (NavMesh.SamplePosition(
                randomPosition,
                out NavMeshHit hit,
                patrolRadius + 2f,
                NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

            nextPatrolTime = Time.time + patrolWaitTime;
        }
    }

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;

        bool destinationSet = agent.SetDestination(player.position);

        Debug.Log("Chasing Player: " + destinationSet);
    }

    void AttackPlayer()
    {
        // Stop moving while attacking.
        agent.ResetPath();

        // Face the Player.
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Play attack animation if assigned.
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    void ShootFireball()
    {
        if (Time.time < nextAttackTime)
            return;

        if (fireballPrefab == null || fireballSpawnPoint == null)
        {
            Debug.LogError("Fireball references are missing!");
            return;
        }

        // Aim towards the Player.
        Vector3 targetPosition = player.position + Vector3.up * 0.5f;

        Vector3 fireballDirection =
            (targetPosition - fireballSpawnPoint.position).normalized;

        // Spawn the fireball.
        GameObject fireball = Instantiate(
            fireballPrefab,
            fireballSpawnPoint.position,
            Quaternion.LookRotation(fireballDirection)
        );

        // Play shooting sound.
        AudioSource audio = fireball.GetComponent<AudioSource>();

        if (audio != null && audio.clip != null)
        {
            AudioSource.PlayClipAtPoint(
                audio.clip,
                transform.position,
                audio.volume
            );

            Debug.Log("Fireball shooting sound played!");
        }

        // Set projectile direction.
        FireballProjectile projectile =
            fireball.GetComponent<FireballProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(fireballDirection);
            Debug.Log("Pumpkin fired a fireball!");
        }
        else
        {
            Debug.LogError("FireballProjectile component is missing!");
        }

        // Set next shooting time.
        nextAttackTime = Time.time + attackCooldown;
    }
}
