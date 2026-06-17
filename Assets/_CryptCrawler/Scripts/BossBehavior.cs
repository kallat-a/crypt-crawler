using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;

// Boss for Crypt Crawler — the guardian of the Doors of Heaven.
//
// Fight design (always-aggressive):
//  - Immediately walks toward the player (slow, lumbering) for the whole fight.
// Fight design (ranged chaser):
//  - Walks toward the player the whole fight to close distance for clean shots.
//  - Fires a projectile on a cooldown. No melee, no contact damage — touching
//    the boss is safe; the projectiles are the only direct threat.
//  - Summons waves of weak zombies: a new wave only starts after the previous
//    wave is fully cleared AND a cooldown passes.
//  - On death it tells the DungeonManager, which drops the key and unlocks the
//    exit gate (same win flow as Level 1).
//
// Needs: NavMeshAgent, Animator with bool "moving" and triggers
// "cast", "summon", "die". A floating health bar (world-space slider).
[RequireComponent(typeof(NavMeshAgent))]
public class BossBehavior : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 150;
    public Slider healthBar;

    [Header("Movement")]
    public float walkSpeed = 1.8f;      // slow, lumbering chase
    public float repathInterval = 0.3f;

    [Header("Projectile (ranged attack)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public int projectileDamage = 15;
    public float castCooldown = 3f;
    public float castWindup = 0.7f;

    [Header("Summon (wave-based)")]
    public GameObject weakZombiePrefab;
    public int waveSize = 3;             // zombies per wave
    public float waveCooldown = 6f;      // delay AFTER a wave is fully cleared
    public float summonWindup = 1f;
    public bool summonFirstWaveImmediately = true;

    [Header("Death")]
    public float deathAnimTime = 3.5f;
    public AudioClip deathSFX;
    public AudioClip castSFX;
    public AudioClip summonSFX;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private int currentHealth;

    private bool isDying = false;
    private bool busy = false;           // mid cast/summon (rooted)

    private float repathTimer = 0f;
    private float castTimer = 0f;

    // Wave state: a new wave only starts after the previous wave is fully
    // cleared AND a cooldown elapses.
    private bool waveActive = false;     // a wave is currently alive
    private float waveCooldownTimer = 0f;

    private readonly List<GameObject> summoned = new List<GameObject>();

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        agent.speed = walkSpeed;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;

        // Stagger cast so it doesn't fire on the very first frame.
        castTimer = castCooldown;
        // First wave can fire almost immediately if enabled.
        waveCooldownTimer = summonFirstWaveImmediately ? 1f : waveCooldown;
    }

    void Update()
    {
        if (isDying || player == null) return;

        if (!DungeonManager.IsPlaying || !PlayerVitals.IsAlive)
        {
            agent.isStopped = true;
            if (animator != null) animator.SetBool("moving", false);
            return;
        }

        FacePlayer();

        // Tick cooldowns.
        castTimer -= Time.deltaTime;

        // Wave tracking: if a wave is active, check whether it's been cleared.
        if (waveActive && CountSummoned() == 0)
        {
            waveActive = false;
            waveCooldownTimer = waveCooldown; // start the post-clear cooldown
        }
        if (!waveActive)
        {
            waveCooldownTimer -= Time.deltaTime;
        }

        if (busy)
        {
            agent.isStopped = true;
            if (animator != null) animator.SetBool("moving", false);
            return;
        }

        // Cast on cooldown; otherwise summon a wave when ready; otherwise chase.
        if (castTimer <= 0f)
        {
            StartCoroutine(CastRoutine());
        }
        else if (!waveActive && waveCooldownTimer <= 0f && weakZombiePrefab != null)
        {
            StartCoroutine(SummonRoutine());
        }
        else
        {
            Chase();
        }
    }

    void Chase()
    {
        agent.isStopped = false;
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            agent.SetDestination(player.position);
            repathTimer = repathInterval;
        }
        if (animator != null) animator.SetBool("moving", true);
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 6f * Time.deltaTime);
        }
    }

    System.Collections.IEnumerator CastRoutine()
    {
        busy = true;
        castTimer = castCooldown;
        agent.isStopped = true;

        if (animator != null)
        {
            animator.SetBool("moving", false);
            animator.SetTrigger("cast");
        }
        if (castSFX != null) AudioSource.PlayClipAtPoint(castSFX, transform.position);

        yield return new WaitForSeconds(castWindup);

        if (!isDying && player != null && projectilePrefab != null && firePoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            BossProjectile bp = proj.GetComponent<BossProjectile>();
            if (bp != null)
            {
                Vector3 target = player.position + Vector3.up * 1f;
                bp.Launch(target, projectileDamage);
            }
        }

        yield return new WaitForSeconds(0.3f);
        busy = false;
    }

    System.Collections.IEnumerator SummonRoutine()
    {
        busy = true;
        agent.isStopped = true;

        if (animator != null)
        {
            animator.SetBool("moving", false);
            animator.SetTrigger("summon");
        }
        if (summonSFX != null) AudioSource.PlayClipAtPoint(summonSFX, transform.position);

        yield return new WaitForSeconds(summonWindup);

        if (!isDying && weakZombiePrefab != null)
        {
            summoned.Clear();
            for (int i = 0; i < waveSize; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position + offset, out hit, 4f, NavMesh.AllAreas))
                {
                    summoned.Add(Instantiate(weakZombiePrefab, hit.position, Quaternion.identity));
                }
            }
            waveActive = true;
        }

        yield return new WaitForSeconds(0.4f);
        busy = false;
    }

    int CountSummoned()
    {
        summoned.RemoveAll(z => z == null);
        return summoned.Count;
    }

    public void TakeDamage(int amount)
    {
        if (isDying) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (healthBar != null) healthBar.value = currentHealth;

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDying) return;
        isDying = true;

        agent.isStopped = true;
        StopAllCoroutines();

        if (deathSFX != null) AudioSource.PlayClipAtPoint(deathSFX, transform.position);
        if (animator != null)
        {
            animator.SetBool("moving", false);
            animator.SetTrigger("die");
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        if (healthBar != null) healthBar.gameObject.SetActive(false);

        DungeonManager manager = FindAnyObjectByType<DungeonManager>();
        if (manager != null) manager.BossDefeated(transform.position);

        Destroy(gameObject, deathAnimTime);
    }
}