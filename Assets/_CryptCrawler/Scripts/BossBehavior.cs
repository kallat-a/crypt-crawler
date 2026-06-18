using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class BossBehavior : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 150;
    public Slider healthBar;

    [Header("Movement")]
    public float walkSpeed = 1.8f;
    public float repathInterval = 0.3f;

    [Header("Projectile (ranged attack)")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public int projectileDamage = 15;
    public float castCooldown = 3f;
    public float castWindup = 0.7f;

    [Header("Summon (wave-based)")]
    public GameObject weakZombiePrefab;
    public int waveSize = 3;
    public float waveCooldown = 10f;
    public float summonWindup = 1f;
    public bool summonFirstWaveImmediately = true;

    [Header("Wave UI")]
    public TMP_Text waveText;

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
    private bool busy = false;
    private bool summoning = false;

    private float repathTimer = 0f;
    private float castTimer = 0f;

    private float waveCooldownTimer = 0f;
    private int wavesSummoned = 0;

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

        if (waveText == null)
        {
            GameObject waveTextObject = GameObject.Find("WaveText");
            if (waveTextObject != null)
            {
                waveText = waveTextObject.GetComponent<TMP_Text>();
            }
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // initialize a cast timer so the boss doesn't fire as soon as it spawns
        castTimer = castCooldown;

        if (summonFirstWaveImmediately)
        {
            // a short delay before first wave instead of waiting for the full cooldown
            waveCooldownTimer = 1f;
        }
        else
        {
            waveCooldownTimer = waveCooldown;
        }
    }

    void Update()
    {
        if (isDying || player == null)
        {
            return;
        }

        if (!DungeonManager.IsPlaying || !PlayerVitals.IsAlive)
        {
            agent.isStopped = true;
            if (animator != null)
            {
                animator.SetBool("moving", false);
            }
            UpdateWaveText();
            return;
        }

        FacePlayer();

        castTimer -= Time.deltaTime;
        waveCooldownTimer -= Time.deltaTime;
        UpdateWaveText();

        if (busy)
        {
            agent.isStopped = true;
            if (animator != null)
            {
                animator.SetBool("moving", false);
            }
            return;
        }

        if (castTimer <= 0f)
        {
            StartCoroutine(CastRoutine());
        }
        else if (waveCooldownTimer <= 0f && weakZombiePrefab != null)
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
        if (animator != null)
        {
            animator.SetBool("moving", true);
        }
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
        if (castSFX != null)
        {
            AudioSource.PlayClipAtPoint(castSFX, transform.position);
        }

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

        // short pause after firing before the boss can act again
        yield return new WaitForSeconds(0.3f);
        busy = false;
    }

    System.Collections.IEnumerator SummonRoutine()
    {
        busy = true;
        summoning = true;
        agent.isStopped = true;
        UpdateWaveText();

        if (animator != null)
        {
            animator.SetBool("moving", false);
            animator.SetTrigger("summon");
        }
        if (summonSFX != null)
        {
            AudioSource.PlayClipAtPoint(summonSFX, transform.position);
        }

        yield return new WaitForSeconds(summonWindup);

        if (!isDying && weakZombiePrefab != null)
        {
            // every wave spawns more enemies than the previous one
            int count = waveSize + wavesSummoned;
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position + offset, out hit, 4f, NavMesh.AllAreas))
                {
                    summoned.Add(Instantiate(weakZombiePrefab, hit.position, Quaternion.identity));
                }
            }
            wavesSummoned++;
            waveCooldownTimer = waveCooldown;
        }

        summoning = false;
        UpdateWaveText();
        yield return new WaitForSeconds(0.4f);
        busy = false;
    }

    void UpdateWaveText()
    {
        if (waveText == null || isDying)
        {
            return;
        }

        int nextWaveCount = waveSize + wavesSummoned;
        if (summoning)
        {
            waveText.text = "Summoning: " + nextWaveCount + " mobs";
            return;
        }

        int seconds = Mathf.Max(0, Mathf.CeilToInt(waveCooldownTimer));
        waveText.text = "Next wave: " + seconds + "s / " + nextWaveCount + " mobs";
    }

    public void TakeDamage(int amount)
    {
        if (isDying)
        {
            return;
        }

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
        if (isDying)
        {
            return;
        }
        isDying = true;

        agent.isStopped = true;
        StopAllCoroutines();

        if (deathSFX != null)
        {
            AudioSource.PlayClipAtPoint(deathSFX, transform.position);
        }
        if (animator != null)
        {
            animator.SetBool("moving", false);
            animator.SetTrigger("die");
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }
        if (waveText != null)
        {
            waveText.text = "";
        }

        DungeonManager manager = FindAnyObjectByType<DungeonManager>();
        if (manager != null)
        {
            manager.BossDefeated(transform.position);
        }

        Destroy(gameObject, deathAnimTime);
    }
}
