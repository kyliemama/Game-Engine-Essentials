
using UnityEngine;

public class GhostProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 45f;
    public int damage = 15;
    public float lifeTime = 5f;

    private Vector3 direction;
    [HideInInspector] public Transform owner;   // the enemy that fired this

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
        {
            // Hit a wall, floor or ceiling (anything solid that isn't the shooter): stop here.
            if (!other.isTrigger && !(owner && other.transform.IsChildOf(owner.root)))
            {
                hasHit = true;
                Destroy(gameObject);
            }
            return;
        }

        hasHit = true;

        health.TakeDamage(damage);

        Debug.Log("Ghost projectile hit Player! Damage: " + damage);

        Destroy(gameObject);
    }
}
