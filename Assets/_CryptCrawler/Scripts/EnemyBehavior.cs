using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBehavior : MonoBehaviour
{
    public Transform target;
    public float speed = 3f;
    public int damage = 20;

    Rigidbody rb;
    LevelManager levelManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        levelManager = FindAnyObjectByType<LevelManager>();

        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player)
            {
                target = player.transform;
            }
        }
    }

    void FixedUpdate()
    {
        if (!LevelManager.IsPlaying)
            return;

        if (target)
        {
            FollowTarget();
        }
    }

    void FollowTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0;

        rb.MovePosition(
            transform.position + direction.normalized * speed * Time.deltaTime
        );

        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth =
                collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }

    public void DestroyEnemy()
    {
        levelManager.EnemyKilled();
        Destroy(gameObject);
    }
}