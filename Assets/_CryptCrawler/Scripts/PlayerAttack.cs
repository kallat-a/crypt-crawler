using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 3f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                attackRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth =
                    hit.GetComponent<EnemyHealth>();

                if (enemyHealth)
                {
                    enemyHealth.TakeDamage(1);
                }
            }
        }
    }
}