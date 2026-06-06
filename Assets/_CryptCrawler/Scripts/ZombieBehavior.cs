using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

// Zombie enemy for Crypt Crawler. NavMeshAgent chase, interval contact damage,
// floating health bar, death animation (falls back to a shrink tween if no
// "die" state exists), and kill notification to the DungeonManager so the key
// can spawn once the room is cleared.
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieBehavior : MonoBehaviour
{
    [Header("General Settings")]
    public int maxHealth = 50;
    public float repathInterval = 0.25f;

    [Header("Contact Damage")]
    public int damage = 10;
    public float damageInterval = 1f;

    [Header("Health Bar")]
    public Slider healthBar;             // world-space slider above the head

    [Header("Death")]
    public float deathAnimTime = 2f;     // match the Dying clip length
    public float deathShrinkTime = 0.35f; // fallback tween if no death anim
    public AudioClip deathSFX;

    [Header("Drops")]
    public GameObject[] possibleDrops;
    [Range(0f, 1f)]
    public float dropChance = 0.35f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private int currentHealth;
    private float repathTimer = 0f;
    private float damageTimer = 0f;
    private bool isDying = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (isDying || player == null) return;

        if (!DungeonManager.IsPlaying || !PlayerVitals.IsAlive)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            agent.SetDestination(player.position);
            repathTimer = repathInterval;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DealContactDamage(other);
            damageTimer = 0f;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            DealContactDamage(other);
            damageTimer = 0f;
        }
    }

    void DealContactDamage(Collider playerCollider)
    {
        if (isDying) return;

        PlayerVitals vitals = playerCollider.GetComponent<PlayerVitals>();
        if (vitals != null)
        {
            vitals.TakeDamage(damage);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDying) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDying) return;
        isDying = true;

        agent.isStopped = true;

        if (deathSFX != null)
        {
            AudioSource.PlayClipAtPoint(deathSFX, transform.position);
        }

        if (possibleDrops != null && possibleDrops.Length > 0 && Random.value <= dropChance)
        {
            GameObject drop = possibleDrops[Random.Range(0, possibleDrops.Length)];
            Instantiate(drop, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        // Tell the manager so it can track remaining zombies / spawn the key.
        DungeonManager manager = FindAnyObjectByType<DungeonManager>();
        if (manager != null)
        {
            manager.ZombieKilled();
        }

        // Stop interacting with the world while dying.
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Hide the health bar on death.
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        // Play the death animation if the Animator has a "die" trigger;
        // otherwise fall back to the shrink tween.
        if (animator != null && HasParameter(animator, "die"))
        {
            animator.SetTrigger("die");
            Destroy(gameObject, deathAnimTime);
        }
        else
        {
            StartCoroutine(ShrinkAndDestroy());
        }
    }

    bool HasParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.name == paramName) return true;
        }
        return false;
    }

    IEnumerator ShrinkAndDestroy()
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < deathShrinkTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deathShrinkTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
