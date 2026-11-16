
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

        bool hitEnemy = false;

        foreach (Collider enemy in enemies)
        {
            // Bat damage
            BatHealth batHealth =
                enemy.GetComponentInParent<BatHealth>();

            if (batHealth != null)
            {
                batHealth.TakeDamage(attackDamage);
                hitEnemy = true;
            }

            // Pumpkin and Tree Boss damage
            EnemyHealth enemyHealth =
                enemy.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
                hitEnemy = true;
            }

            // Ghost Mini-Boss damage
            GhostBossHealth ghostHealth =
                enemy.GetComponentInParent<GhostBossHealth>();

            if (ghostHealth != null)
            {
                ghostHealth.TakeDamage(attackDamage);
                hitEnemy = true;
            }
        }

        if (hitEnemy)
            Debug.Log("Player successfully attacked an enemy!");
        else
            Debug.Log("No enemy in attack range!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}
