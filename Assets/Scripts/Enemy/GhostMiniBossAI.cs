
using UnityEngine;

public class GhostMiniBossAI : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 10f;
    public float stoppingDistance = 2f;

    [Header("Patrol (when the Player is not detected)")]
    public float patrolRadius = 4f;
    public float patrolSpeed = 1.5f;
    public float patrolWaitTime = 2f;

    [Header("Floating")]
    public float floatHeight = 1.5f;
    public float floatSpeed = 2f;
    public float floatAmount = 0.25f;

    private Vector3 startPosition;
    private Vector3 patrolTarget;
    private bool hasPatrolTarget;
    private float nextPatrolTime;

    void Start()
    {
        startPosition = transform.position;
    }

    void OnDisable()
    {
        EnemyMusic.Track(this, false);
    }

    void Update()
    {
        // Find the Player if it isn't assigned (or was destroyed / replaced).
        if (player == null)
            player = PlayerLocator.Find();

        float distance = player != null
            ? Vector3.Distance(transform.position, player.position)
            : float.MaxValue;

        EnemyMusic.Track(this, distance <= detectionRange);

        if (distance > detectionRange)
            Patrol();
        else
            hasPatrolTarget = false;

        // Move towards the Player when detected.
        if (distance <= detectionRange &&
            distance > stoppingDistance)
        {
            Vector3 direction =
                player.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Vector3 horizontalMovement =
                    direction.normalized * moveSpeed * Time.deltaTime;

                transform.position += horizontalMovement;
            }
        }

        // Float gently above the ground.
        float floatingOffset =
            Mathf.Sin(Time.time * floatSpeed) * floatAmount;

        float targetY = startPosition.y + floatHeight + floatingOffset;

        Vector3 position = transform.position;
        position.y = Mathf.Lerp(
            position.y,
            targetY,
            3f * Time.deltaTime
        );

        transform.position = position;
    }

    // Drift between random points around where the ghost started.
    void Patrol()
    {
        if (!hasPatrolTarget)
        {
            if (Time.time < nextPatrolTime) return;

            Vector2 random = Random.insideUnitCircle * patrolRadius;
            patrolTarget = startPosition + new Vector3(random.x, 0f, random.y);
            hasPatrolTarget = true;
        }

        Vector3 toTarget = patrolTarget - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= 0.3f)
        {
            hasPatrolTarget = false;
            nextPatrolTime = Time.time + patrolWaitTime;
            return;
        }

        transform.position += Vector3.ClampMagnitude(toTarget.normalized * patrolSpeed * Time.deltaTime, toTarget.magnitude);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 home = Application.isPlaying ? startPosition : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(home, patrolRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
