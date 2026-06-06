using UnityEngine;

// Fixed-offset top-down follow camera. Set the camera's position/angle in the
// editor relative to the player; this keeps that offset while following.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 10f;

    private Vector3 offset;

    void Start()
    {
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition,
                                          smoothSpeed * Time.deltaTime);
    }
}
