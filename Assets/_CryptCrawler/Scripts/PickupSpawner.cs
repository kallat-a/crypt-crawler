using UnityEngine;
using UnityEngine.AI;

public class PickupSpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject goldPrefab;
    public float spawnInterval = 5f;
    [Range(0f, 1f)]
    public float spawnChance = 0.5f;
    public int maxActiveGold = 8;

    [Header("Placement")]
    public float spawnRadius = 12f;
    public float minPlayerDistance = 5f;
    public float spawnHeight = 0.5f;

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
        if (!DungeonManager.IsPlaying || goldPrefab == null)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer < spawnInterval)
        {
            return;
        }
        timer = 0f;

        if (Random.value > spawnChance)
        {
            return;
        }
        if (CountActivePickups() >= maxActiveGold)
        {
            return;
        }

        Vector3 spawnPosition;
        if (TryFindSpawnPoint(out spawnPosition))
        {
            Instantiate(goldPrefab, spawnPosition, Quaternion.identity);
        }
    }

    bool TryFindSpawnPoint(out Vector3 result)
    {
        // try multiple times since random positions may land off the NavMesh or too close to the player
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 circle = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = transform.position + new Vector3(circle.x, 0f, circle.y);

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(candidate, out hit, 2f, NavMesh.AllAreas))
            {
                continue;
            }

            if (player != null && Vector3.Distance(hit.position, player.position) < minPlayerDistance)
            {
                continue;
            }

            result = hit.position + Vector3.up * spawnHeight;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    int CountActivePickups()
    {
        ResourcePickup[] pickups = FindObjectsByType<ResourcePickup>(FindObjectsInactive.Exclude);

        int count = 0;
        foreach (ResourcePickup pickup in pickups)
        {
            if (pickup.type == ResourcePickup.PickupType.Health)
            {
                count++;
            }
        }
        return count;
    }
}
