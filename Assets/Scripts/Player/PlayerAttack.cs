
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackRange = 6f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    void Attack()
    {
        Collider[] enemies = Physics.OverlapSphere(
            transform.position,
            attackRange
        );

        foreach (Collider enemy in enemies)
        {
            // Damage Pumpkin enemies.
            EnemyHealth health =
                enemy.GetComponentInParent<EnemyHealth>();

            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }

            // Damage Ghost Mini-Boss.
            GhostBossHealth ghostHealth =
                enemy.GetComponentInParent<GhostBossHealth>();

            if (ghostHealth != null)
            {
                ghostHealth.TakeDamage(attackDamage);
            }
        }
    }

    // Show attack range in Scene view.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
