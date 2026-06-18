using UnityEngine;

public class ExitGate : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        DungeonManager manager = FindAnyObjectByType<DungeonManager>();
        if (manager != null)
        {
            manager.TryExit();
        }
    }
}
