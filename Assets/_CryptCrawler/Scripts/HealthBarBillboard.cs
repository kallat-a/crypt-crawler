using UnityEngine;

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
        if (cameraTransform == null)
        {
            return;
        }

        transform.rotation = cameraTransform.rotation;
    }
}
