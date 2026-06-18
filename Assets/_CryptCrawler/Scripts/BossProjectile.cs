using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 6f;
    public GameObject hitEffect;

    private int damage = 15;
    private Rigidbody rb;
    private bool spent = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    public void Launch(Vector3 targetPoint, int damageAmount)
    {
        damage = damageAmount;

        Vector3 direction = (targetPoint - transform.position).normalized;
        transform.forward = direction;
        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (spent)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerVitals vitals = other.GetComponent<PlayerVitals>();
            if (vitals != null)
            {
                vitals.TakeDamage(damage);
            }
        }

        spent = true;
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
