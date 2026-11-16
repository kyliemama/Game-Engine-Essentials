
using UnityEngine;

public class GhostMiniBossAI : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 10f;
    public float stoppingDistance = 2f;

    [Header("Floating")]
    public float floatHeight = 1.5f;
    public float floatSpeed = 2f;
    public float floatAmount = 0.25f;

    private Vector3 startPosition;

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
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        EnemyMusic.Track(this, distance <= detectionRange);

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
}
