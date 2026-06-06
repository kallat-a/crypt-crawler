using UnityEngine;
using System.Collections;

// Typed pickup for Crypt Crawler: gold (score), the level key, or health.
// Spins in place; on player touch it applies its effect, plays a sound, and
// shrinks out with a code-driven tween. Ammo type arrives with throwables in FP3.
public class ResourcePickup : MonoBehaviour
{
    public enum PickupType { Gold, Key, Health, Speed, Strength }

    [Header("Pickup Settings")]
    public PickupType type = PickupType.Gold;
    public int amount = 5;               // gold value or health restored
    public float rotationSpeed = 60f;
    public AudioClip collectSFX;

    [Header("Collect Animation")]
    public float collectShrinkTime = 0.25f;

    private bool collected = false;

    void Update()
    {
        if (!collected)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        ApplyEffect(other);

        if (collectSFX != null)
        {
            AudioSource.PlayClipAtPoint(collectSFX, transform.position);
        }

        // Disable the collider so it can't trigger twice during the tween.
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        StartCoroutine(ShrinkAndDestroy());
    }

    void ApplyEffect(Collider playerCollider)
    {
        DungeonManager manager = FindAnyObjectByType<DungeonManager>();

        switch (type)
        {
            case PickupType.Gold:
                if (manager != null) manager.AddGold(amount);
                break;

            case PickupType.Key:
                if (manager != null) manager.CollectKey();
                break;

            case PickupType.Health:
                PlayerVitals vitals = playerCollider.GetComponent<PlayerVitals>();
                if (vitals != null) vitals.Heal(amount);
                break;
            case PickupType.Speed:
                CrawlerController crawler = playerCollider.GetComponent<CrawlerController>();
                if (crawler != null) crawler.ApplySpeedBoost(2f, 5f);
                break;

            case PickupType.Strength:
                MeleeAttack melee = playerCollider.GetComponent<MeleeAttack>();
                if (melee != null) melee.ApplyStrengthBoost(25, 5f);
                break;
        }
    }

    // Code-driven collect animation: scale to zero over collectShrinkTime.
    IEnumerator ShrinkAndDestroy()
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < collectShrinkTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / collectShrinkTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
