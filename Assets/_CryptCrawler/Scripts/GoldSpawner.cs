using UnityEngine;
using UnityEngine.AI;

// Periodically drops gold somewhere reachable in the level: every
// "spawnInterval" seconds there's a "spawnChance" chance of one gold pickup
// appearing at a random point on the NavMesh, at least "minPlayerDistance"
// from the player (so collecting it pulls the player around the map while
// kiting zombies). Capped so the floor doesn't flood.
public class GoldSpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject goldPrefab;        // GoldPickup prefab
    public float spawnInterval = 5f;
    [Range(0f, 1f)] public float spawnChance = 0.5f;
    public int maxActiveGold = 8;

    [Header("Placement")]
    public float spawnRadius = 12f;      // how far from this object to scatter
    public float minPlayerDistance = 5f; // keep drops away from the player
    public float spawnHeight = 0.5f;     // lift above the floor

    private Transform player;
    private float timer = 0f;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (!DungeonManager.IsPlaying || goldPrefab == null) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        if (Random.value > spawnChance) return;
        if (CountActiveGold() >= maxActiveGold) return;

        Vector3 spawnPosition;
        if (TryFindSpawnPoint(out spawnPosition))
        {
            Instantiate(goldPrefab, spawnPosition, Quaternion.identity);
        }
    }

    // Try a handful of random points; accept the first that lands on the
    // NavMesh (guaranteed walkable floor, never inside a wall or prop) and is
    // far enough from the player.
    bool TryFindSpawnPoint(out Vector3 result)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 circle = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = transform.position + new Vector3(circle.x, 0f, circle.y);

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(candidate, out hit, 2f, NavMesh.AllAreas))
            {
                continue; // not on walkable floor, try again
            }

            if (player != null &&
                Vector3.Distance(hit.position, player.position) < minPlayerDistance)
            {
                continue; // too close to the player, try again
            }

            result = hit.position + Vector3.up * spawnHeight;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    int CountActiveGold()
    {
        // Gold pickups are the only ResourcePickups of type Gold in the scene.
        ResourcePickup[] pickups =
            FindObjectsByType<ResourcePickup>();

        int count = 0;
        foreach (ResourcePickup pickup in pickups)
        {
            if (pickup.type == ResourcePickup.PickupType.Gold) count++;
        }
        return count;
    }
}
