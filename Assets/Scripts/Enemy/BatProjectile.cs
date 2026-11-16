
using UnityEngine;

public class BatProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 8f;
    public int damage = 10;
    public float lifeTime = 5f;

    [Header("Bouncing Movement")]
    public float bounceHeight = 0.6f;
    public float bounceSpeed = 3f;

    private Vector3 startPosition;
    private Vector3 direction;
    private float targetDistance;
    private float elapsedTime;
    private bool hasHit;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(Vector3 targetPosition)
    {
        startPosition = transform.position;

        Vector3 difference = targetPosition - startPosition;

        targetDistance = difference.magnitude;
        direction = difference.normalized;
    }

    void Update()
    {
        if (hasHit || targetDistance <= 0f)
            return;

        elapsedTime += Time.deltaTime;

        float travelled = speed * elapsedTime;

        float progress = Mathf.Clamp01(
            travelled / targetDistance
        );

        Vector3 forwardPosition =
            startPosition + direction *
            Mathf.Min(travelled, targetDistance);

        float bounce =
            Mathf.Abs(Mathf.Sin(elapsedTime * bounceSpeed))
            * bounceHeight
            * Mathf.Sin(progress * Mathf.PI);

        transform.position =
            forwardPosition + Vector3.up * bounce;

        if (progress >= 1f)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            hasHit = true;

            playerHealth.TakeDamage(damage);

            Debug.Log("Bat hit Player!");
            Debug.Log("Player received damage: " + damage);
            Debug.Log("Remaining Player HP: " +
                      playerHealth.currentHealth);

            Destroy(gameObject);
        }
    }
}
