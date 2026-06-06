using UnityEngine;
using System.Collections;

// Melee attack for Crypt Crawler. On left-click the player faces the mouse,
// swings, and damages every enemy inside a short arc in front of them.
[RequireComponent(typeof(CrawlerController))]
public class MeleeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 25;
    public float range = 1.8f;          // how far the swing reaches
    public float arcAngle = 100f;       // total arc width in degrees
    public float cooldown = 0.6f;
    public float faceLockDuration = 0.25f; // how long the swing owns facing

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

        // Face the point under the mouse so the player attacks where they aim.
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

        // Hit detection: every enemy collider within range AND inside the arc.
        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector3 toEnemy = hit.transform.position - transform.position;
            toEnemy.y = 0f;

            if (Vector3.Angle(transform.forward, toEnemy) <= arcAngle * 0.5f)
            {
                ZombieBehavior zombie = hit.GetComponent<ZombieBehavior>();
                if (zombie != null)
                {
                    zombie.TakeDamage(damage);
                }
            }
        }
    }

    // Raycast from the camera through the mouse onto the ground plane at the
    // player's height. Works regardless of what the ray actually hits.
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
