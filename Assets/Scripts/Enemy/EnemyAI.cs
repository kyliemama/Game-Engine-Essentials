
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Pumpkin enemy: patrols around where it started, chases and shoots fireballs at the Player
/// when the Player comes close, and returns to patrolling when the Player leaves its territory.
///
/// Uses the NavMeshAgent when the scene has a baked NavMesh. If there is no NavMesh under the
/// enemy, it still patrols and chases by moving directly (and warns you to bake the NavMesh).
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Optional. Left empty, the enemy finds the Player by its 'Player' tag.")]
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
    public float turnSpeed = 8f;

    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 2f;

    [Header("Animation")]
    public Animator animator;

    [Header("Fireball Attack")]
    public GameObject fireballPrefab;
    public Transform fireballSpawnPoint;

    [Header("Debug")]
    public bool logState = false;

    private NavMeshAgent agent;
    private bool useNavMesh;
    private Vector3 patrolCentre;
    private Vector3 patrolTarget;
    private bool hasPatrolTarget;

    private float nextAttackTime;
    private float nextPatrolTime;
    private bool wasChasing = false;

    void Start()
    {
        patrolCentre = transform.position;
        agent = GetComponent<NavMeshAgent>();

        if (agent != null && !agent.isOnNavMesh)
        {
            // The enemy may be placed slightly above or beside the NavMesh. Snap it on if it's close.
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }

        useNavMesh = agent != null && agent.isOnNavMesh;

        if (useNavMesh)
        {
            agent.speed = patrolSpeed;
            agent.stoppingDistance = Mathf.Max(0f, attackRange * 0.8f);
        }
        else
        {
            // Without a NavMesh the agent would fight our movement, so switch it off.
            if (agent != null) agent.enabled = false;

            Debug.LogWarning($"{name}: no baked NavMesh under this enemy, so it is moving without pathfinding " +
                             "(it may walk through walls). Select the object with the NavMeshSurface component " +
                             "and click Bake.", this);
        }
    }

    void OnDisable()
    {
        EnemyMusic.Track(this, false);
    }

    void Update()
    {
        // Find the Player if it isn't assigned (or was destroyed / replaced).
        player = PlayerLocator.Resolve(player);

        if (player == null)
        {
            Patrol();
            return;
        }

        float distanceToPlayer = FlatDistance(transform.position, player.position);
        float playerDistanceFromCentre = FlatDistance(player.position, patrolCentre);

        bool playerInTerritory = playerDistanceFromCentre <= territoryRadius;
        bool playerDetected = playerInTerritory && distanceToPlayer <= detectionRange;

        EnemyMusic.Track(this, playerDetected);

        if (!playerDetected)
        {
            if (wasChasing)
            {
                if (logState) Debug.Log($"{name}: lost the Player, going back to patrol", this);
                StopMoving();
                wasChasing = false;
                hasPatrolTarget = false;
                nextPatrolTime = Time.time;
            }

            Patrol();
            return;
        }

        if (!wasChasing && logState) Debug.Log($"{name}: spotted the Player", this);

        // Shoot repeatedly while the Player is detected.
        ShootFireball();

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            ChasePlayer();
        }

        wasChasing = true;
    }

    // ------------------------------------------------------------ states

    void Patrol()
    {
        bool arrived = !hasPatrolTarget || ReachedDestination(patrolTarget);

        if (arrived)
        {
            if (hasPatrolTarget)
            {
                // Just arrived: wait before picking the next point.
                StopMoving();
                hasPatrolTarget = false;
                nextPatrolTime = Time.time + patrolWaitTime;
            }

            if (Time.time >= nextPatrolTime && PickPatrolPoint(out patrolTarget))
                hasPatrolTarget = true;
        }

        if (hasPatrolTarget)
            MoveTowards(patrolTarget, patrolSpeed);
    }

    void ChasePlayer()
    {
        MoveTowards(player.position, chaseSpeed);
    }

    void AttackPlayer()
    {
        // Stop moving while attacking, and face the Player.
        StopMoving();
        FaceTowards(player.position);

        if (animator != null)
            animator.SetTrigger("Attack");
    }

    // ------------------------------------------------------------ movement

    bool PickPatrolPoint(out Vector3 point)
    {
        Vector2 random = Random.insideUnitCircle * patrolRadius;
        Vector3 candidate = patrolCentre + new Vector3(random.x, 0f, random.y);

        if (useNavMesh)
        {
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, patrolRadius + 2f, NavMesh.AllAreas))
            {
                point = hit.position;
                return true;
            }

            point = transform.position;
            nextPatrolTime = Time.time + 0.5f; // try again shortly
            return false;
        }

        candidate.y = transform.position.y;
        point = candidate;
        return true;
    }

    void MoveTowards(Vector3 target, float speed)
    {
        if (useNavMesh)
        {
            if (!agent.isOnNavMesh) return;
            agent.isStopped = false;
            agent.speed = speed;
            agent.SetDestination(target);
            return;
        }

        Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
        transform.position = Vector3.MoveTowards(transform.position, flatTarget, speed * Time.deltaTime);
        FaceTowards(target);
    }

    void StopMoving()
    {
        if (useNavMesh && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }

    bool ReachedDestination(Vector3 target)
    {
        if (useNavMesh)
        {
            if (agent.pathPending) return false;
            return !agent.hasPath || agent.remainingDistance <= Mathf.Max(0.5f, agent.stoppingDistance + 0.1f);
        }

        return FlatDistance(transform.position, target) <= 0.5f;
    }

    void FaceTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // ------------------------------------------------------------ attack

    void ShootFireball()
    {
        if (Time.time < nextAttackTime)
            return;

        if (fireballPrefab == null || fireballSpawnPoint == null)
        {
            Debug.LogError("Fireball references are missing!", this);
            nextAttackTime = Time.time + attackCooldown;
            return;
        }

        // Aim towards the Player.
        Vector3 targetPosition = player.position + Vector3.up * 0.5f;

        Vector3 fireballDirection =
            (targetPosition - fireballSpawnPoint.position).normalized;

        GameObject fireball = Instantiate(
            fireballPrefab,
            fireballSpawnPoint.position,
            Quaternion.LookRotation(fireballDirection)
        );

        // Play shooting sound.
        AudioSource audio = fireball.GetComponent<AudioSource>();

        if (audio != null && audio.clip != null)
        {
            AudioSource.PlayClipAtPoint(audio.clip, transform.position, audio.volume);
        }

        FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();

        if (projectile != null)
            projectile.SetDirection(fireballDirection);
        else
            Debug.LogError("FireballProjectile component is missing!", this);

        nextAttackTime = Time.time + attackCooldown;
    }

    // ------------------------------------------------------------ gizmos

    void OnDrawGizmosSelected()
    {
        Vector3 centre = Application.isPlaying ? patrolCentre : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centre, patrolRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(centre, territoryRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
