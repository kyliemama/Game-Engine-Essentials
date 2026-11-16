
using UnityEngine;

public class TreeProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 12f;
    public int damage = 20;
    public float lifeTime = 5f;

    private Transform player;
    [HideInInspector] public Transform owner;   // the enemy that fired this

    private bool hasHit;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetTarget(Transform target)
    {
        player = target;
    }

    void Update()
    {
        if (player == null || hasHit)
            return;

        Vector3 targetPosition =
            player.position + Vector3.up * 0.5f;

        Vector3 direction =
            (targetPosition - transform.position).normalized;

        transform.position +=
            direction * speed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
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

            Debug.Log("Tree Boss hit Player!");
            Debug.Log("Player received damage: " + damage);
            Debug.Log("Remaining Player HP: " +
                      playerHealth.CurrentHealth);

            Destroy(gameObject);
            return;
        }

        // Hit a wall, floor or ceiling (anything solid that isn't the shooter): stop here.
        if (!other.isTrigger && !(owner && other.transform.IsChildOf(owner.root)))
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }
}
