using UnityEngine;

// Projectile fired by the boss. Launched toward a target point; flies straight,
// damages the player on contact, and self-destructs on any hit or after a
// lifetime. Put this on a small glowing orb prefab with a Rigidbody (useGravity
// off) and a trigger SphereCollider.
[RequireComponent(typeof(Rigidbody))]
public class BossProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 6f;
    public GameObject hitEffect;     // optional particle burst on impact

    private int damage = 15;
    private Rigidbody rb;
    private bool spent = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    // Called by BossBehavior right after instantiation.
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
        if (spent) return;

        // Ignore the boss itself and other enemies/projectiles.
        if (other.CompareTag("Enemy")) return;

        if (other.CompareTag("Player"))
        {
            PlayerVitals vitals = other.GetComponent<PlayerVitals>();
            if (vitals != null) vitals.TakeDamage(damage);
        }

        // Hit a wall or the player — burst and die.
        spent = true;
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
