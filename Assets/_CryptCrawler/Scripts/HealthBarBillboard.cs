using UnityEngine;

// Keeps a world-space UI element (e.g. an enemy health bar) facing the camera.
public class HealthBarBillboard : MonoBehaviour
{
    private Transform cameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Match the camera's rotation so the bar is always readable.
        transform.rotation = cameraTransform.rotation;
    }
}
