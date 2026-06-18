using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CrawlerController))]
public class MeleeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 25;
    public float range = 2.4f;
    public float radius = 1.0f;
    public float arcAngle = 140f;
    public float cooldown = 0.6f;
    public float faceLockDuration = 0.25f;

    [Header("Audio")]
    public AudioClip swingSFX;

    public float StrengthBoostRemaining { get; private set; } = 0f;

    private CrawlerController crawler;
    private Animator animator;
    private float cooldownTimer = 0f;
    private float faceLockTimer = 0f;
    private Renderer[] playerRenderers;

    void Start()
    {
        crawler = GetComponent<CrawlerController>();
        animator = GetComponent<Animator>();
        playerRenderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (!DungeonManager.IsPlaying)
        {
            return;
        }

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (faceLockTimer > 0f)
        {
            faceLockTimer -= Time.deltaTime;
            if (faceLockTimer <= 0f)
            {
                crawler.FaceMouseLock = false;
            }
        }

        if (Input.GetButtonDown("Fire1") && cooldownTimer <= 0f)
        {
            Swing();
        }
    }

    void Swing()
    {
        cooldownTimer = cooldown;

        Vector3 mousePoint;
        if (TryGetMouseGroundPoint(out mousePoint))
        {
            crawler.FacePoint(mousePoint);
            crawler.FaceMouseLock = true;
            faceLockTimer = faceLockDuration;
        }

        if (animator != null)
        {
            animator.SetTrigger("attack");
        }
        if (swingSFX != null)
        {
            AudioSource.PlayClipAtPoint(swingSFX, transform.position);
        }

        // to offset the swing area sphere upward so it hits at chest height instead of the ground
        Vector3 swingCenter = transform.position + transform.forward * (range * 0.5f) + Vector3.up * 0.5f;

        Collider[] hits = Physics.OverlapSphere(swingCenter, range * 0.5f + radius);

        // stores the enemies who have already been damaged to avoid damaging them again for the same hit
        List<GameObject> damaged = new List<GameObject>();

        foreach (Collider hit in hits)
        {
            ZombieBehavior zombie = hit.GetComponentInParent<ZombieBehavior>();
            BossBehavior boss = hit.GetComponentInParent<BossBehavior>();
            if (zombie == null && boss == null)
            {
                continue;
            }

            GameObject target;
            if (zombie != null)
            {
                target = zombie.gameObject;
            }
            else
            {
                target = boss.gameObject;
            }

            if (damaged.Contains(target))
            {
                continue;
            }

            Vector3 closest = hit.ClosestPoint(transform.position);
            Vector3 toEnemy = closest - transform.position;
            toEnemy.y = 0f;

            // determines whether the enemy is within a distance where it can be attacked
            bool pointBlank = toEnemy.magnitude < 0.75f;
            if (!pointBlank && Vector3.Angle(transform.forward, toEnemy) > arcAngle * 0.5f)
            {
                continue;
            }

            if (zombie != null)
            {
                zombie.TakeDamage(damage);
                damaged.Add(target);
                continue;
            }

            if (boss != null)
            {
                boss.TakeDamage(damage);
                damaged.Add(target);
            }
        }
    }

    bool TryGetMouseGroundPoint(out Vector3 point)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        float distance;
        if (ground.Raycast(ray, out distance))
        {
            point = ray.GetPoint(distance);
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    public void ApplyStrengthBoost(int bonus, float duration)
    {
        StartCoroutine(StrengthBoostRoutine(bonus, duration));
    }

    IEnumerator StrengthBoostRoutine(int bonus, float duration)
    {
        damage += bonus;
        SetTint(new Color(1f, 0.1f, 0.1f));
        StrengthBoostRemaining = duration;
        while (StrengthBoostRemaining > 0f)
        {
            StrengthBoostRemaining -= Time.deltaTime;
            yield return null;
        }
        StrengthBoostRemaining = 0f;
        damage -= bonus;
        SetTint(Color.white);
    }

    void SetTint(Color color)
    {
        foreach (Renderer r in playerRenderers)
        {
            r.material.color = color;
        }
    }
}
