using UnityEngine;

// Top-down player controller for Crypt Crawler.
// WASD movement relative to the camera's facing; the character rotates to face
// its movement direction. While attacking, MeleeAttack rotates the player to
// face the mouse instead (see FaceMouseLock).
[RequireComponent(typeof(CharacterController))]
public class CrawlerController : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = 9.81f;
    public float rotationSmoothTime = 0.08f;
    public Transform cameraTransform;

    private CharacterController controller;
    private Animator animator;
    private float rotationVelocity;
    private float verticalVelocity;

    // While true, movement does NOT rotate the character (MeleeAttack owns
    // facing during a swing so the player attacks toward the mouse).
    public bool FaceMouseLock { get; set; }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (!DungeonManager.IsPlaying) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        input = Vector3.ClampMagnitude(input, 1f);

        // Make input relative to where the camera is looking (flattened).
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();
        Vector3 camRight = cameraTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDirection = camForward * input.z + camRight * input.x;

        // Rotate the character to face movement (unless an attack owns facing).
        if (moveDirection.sqrMagnitude > 0.001f && !FaceMouseLock)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle,
                                                      ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }

        // Simple gravity so the controller stays grounded on uneven floors.
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 velocity = moveDirection * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // Drive the Animator: 0 = idle, 1 = run. MeleeAttack sets the attack trigger.
        if (animator != null)
        {
            animator.SetInteger("animState", moveDirection.sqrMagnitude > 0.001f ? 1 : 0);
        }
    }

    // Rotate instantly to face a world-space point (used by MeleeAttack).
    public void FacePoint(Vector3 worldPoint)
    {
        Vector3 lookDirection = worldPoint - transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}
