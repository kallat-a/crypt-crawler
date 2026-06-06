using UnityEngine;

// Exit gate for Crypt Crawler levels. Put this on the gate with a trigger
// collider; when the player touches it, the DungeonManager decides whether
// the level is complete (player must hold the key).
public class ExitGate : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        DungeonManager manager = FindAnyObjectByType<DungeonManager>();
        if (manager != null)
        {
            manager.TryExit();
        }
    }
}
