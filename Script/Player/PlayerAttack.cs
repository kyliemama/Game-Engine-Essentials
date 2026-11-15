
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int attackDamage = 10;
    public float attackRange = 2f;

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
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();

            if (health != null)
            {
                health.TakeDamage(attackDamage);
            }
        }
    }
}
