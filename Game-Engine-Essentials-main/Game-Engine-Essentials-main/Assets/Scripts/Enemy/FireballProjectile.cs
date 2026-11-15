
using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [Header("Fireball Settings")]
    public GameObject impactEffect;
    public float speed = 12f;
    public int damage = 10;
    public float lifeTime = 5f;

    private Vector3 direction;
    private bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);

        if (direction == Vector3.zero)
        {
            Debug.LogWarning("Fireball direction was not assigned!");
        }
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
        Debug.Log("Fireball direction: " + direction);
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

        Debug.Log("Fireball hit the Player!");

        health.TakeDamage(damage);

        if (impactEffect != null)
        {
            GameObject explosion = Instantiate(
                impactEffect,
                transform.position,
                Quaternion.identity
            );

            ParticleSystem[] particles =
                explosion.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem particle in particles)
                particle.Play(true);

            Destroy(explosion, 3f);
        }

        Destroy(gameObject);
    }
}
