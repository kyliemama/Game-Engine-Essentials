
using UnityEngine;

public class GhostProjectile : MonoBehaviour
{
    [Header("Ghost Projectile Settings")]
    public float speed = 12f;
    public int damage = 15;
    public float lifeTime = 5f;

    private Vector3 direction;
    private bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (direction == Vector3.zero)
            return;

        transform.position += direction * speed * Time.deltaTime;
    }

    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        hasHit = true;

        health.TakeDamage(damage);

        Debug.Log("Ghost projectile hit the Player!");

        Destroy(gameObject);
    }
}
