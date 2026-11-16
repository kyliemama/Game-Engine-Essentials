
using System.Collections.Generic;
using UnityEngine;

public class TestingAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 15;
    public float attackRange = 6f;

    [Header("Attack Position")]
    public Vector3 attackOffset = new Vector3(0f, 1f, 0f);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    void Attack()
    {
        Vector3 attackPosition = transform.position + attackOffset;

        Collider[] detectedObjects = Physics.OverlapSphere(
            attackPosition,
            attackRange,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide
        );

        HashSet<int> damagedEnemies = new HashSet<int>();
        bool hitEnemy = false;

        Debug.Log("Player attacking! Detected: " + detectedObjects.Length);

        foreach (Collider enemy in detectedObjects)
        {
            // Bat
            BatHealth batHealth =
                enemy.GetComponentInParent<BatHealth>();

            if (batHealth != null &&
                damagedEnemies.Add(batHealth.GetInstanceID()))
            {
                batHealth.TakeDamage(attackDamage);
                Debug.Log("Bat HP: " + batHealth.CurrentHealth);
                hitEnemy = true;
            }

            // Tree Final Boss
            TreeBossHealth treeHealth =
                enemy.GetComponentInParent<TreeBossHealth>();

            if (treeHealth != null &&
                damagedEnemies.Add(treeHealth.GetInstanceID()))
            {
                treeHealth.TakeDamage(attackDamage);
                Debug.Log("Tree Boss HP: " + treeHealth.CurrentHealth);
                hitEnemy = true;
            }

            // Pumpkin Enemy
            EnemyHealth enemyHealth =
                enemy.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null &&
                damagedEnemies.Add(enemyHealth.GetInstanceID()))
            {
                enemyHealth.TakeDamage(attackDamage);
                hitEnemy = true;
            }

            // Ghost Mini-Boss
            GhostBossHealth ghostHealth =
                enemy.GetComponentInParent<GhostBossHealth>();

            if (ghostHealth != null &&
                damagedEnemies.Add(ghostHealth.GetInstanceID()))
            {
                ghostHealth.TakeDamage(attackDamage);
                Debug.Log("Ghost Boss HP: " + ghostHealth.CurrentHealth);
                hitEnemy = true;
            }
        }

        if (hitEnemy)
        {
            Debug.Log("Player successfully attacked!");
        }
        else
        {
            Debug.Log("No enemy detected in attack range!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 attackPosition = transform.position + attackOffset;

        Gizmos.DrawWireSphere(attackPosition, attackRange);
    }
}
