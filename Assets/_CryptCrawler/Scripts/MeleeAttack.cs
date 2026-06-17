using UnityEngine;
using System.Collections;

// Melee attack for Crypt Crawler. On left-click the player faces the mouse,
// swings, and damages every enemy inside a forgiving wedge in front of them.
//
// FP3 hitbox fix (TA feedback): the old version measured distance/angle from
// the player's PIVOT to the enemy's PIVOT, so tall models or fast movers were
// missed. Now it uses each enemy collider's CLOSEST POINT to the player, checks
// that point against the swing range, and uses a generous angle. Also damages
// the boss, not just zombies.
[RequireComponent(typeof(CrawlerController))]
public class MeleeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 25;
    public float range = 2.4f;          // reach (slightly longer than before)
    public float radius = 1.0f;         // forgiveness around the swing center
    public float arcAngle = 140f;       // wider wedge than before
    public float cooldown = 0.6f;
    public float faceLockDuration = 0.25f;

    [Header("Audio")]
    public AudioClip swingSFX;

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
        if (!DungeonManager.IsPlaying) return;

        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (faceLockTimer > 0f)
        {
            faceLockTimer -= Time.deltaTime;
            if (faceLockTimer <= 0f) crawler.FaceMouseLock = false;
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

        if (animator != null) animator.SetTrigger("attack");
        if (swingSFX != null) AudioSource.PlayClipAtPoint(swingSFX, transform.position);

        // The swing is centered slightly in front of the player.
        Vector3 swingCenter = transform.position + transform.forward * (range * 0.5f)
                              + Vector3.up * 0.5f;

        // Overlap a sphere around the swing center: generous, catches tall and
        // fast enemies the old pivot-to-pivot check missed.
        Collider[] hits = Physics.OverlapSphere(swingCenter, range * 0.5f + radius);

        // Track enemies we've already damaged this swing (models can have
        // multiple colliders).
        var damaged = new System.Collections.Generic.HashSet<GameObject>();

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            GameObject root = hit.transform.root.gameObject;
            if (damaged.Contains(root)) continue;

            // Use the collider's CLOSEST point to the player for direction, so a
            // tall model whose pivot is at the feet still registers.
            Vector3 closest = hit.ClosestPoint(transform.position);
            Vector3 toEnemy = closest - transform.position;
            toEnemy.y = 0f;

            // Wide wedge in front; if the enemy is basically on top of us, skip
            // the angle check entirely (point-blank always hits).
            bool pointBlank = toEnemy.magnitude < 0.75f;
            if (!pointBlank && Vector3.Angle(transform.forward, toEnemy) > arcAngle * 0.5f)
                continue;

            // Damage zombies OR the boss.
            ZombieBehavior zombie = root.GetComponent<ZombieBehavior>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage);
                damaged.Add(root);
                continue;
            }

            BossBehavior boss = root.GetComponent<BossBehavior>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                damaged.Add(root);
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
        yield return new WaitForSeconds(duration);
        damage -= bonus;
        SetTint(Color.white);
    }

    void SetTint(Color color)
    {
        foreach (Renderer r in playerRenderers)
            r.material.color = color;
    }
}
