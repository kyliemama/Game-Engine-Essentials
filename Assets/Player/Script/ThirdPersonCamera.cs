using UnityEngine;

/// <summary>
/// Third-person orbit camera. Put this on the Main Camera and keep the camera OUT of the
/// Player hierarchy (it must be a root object in the scene).
///  - The mouse orbits the camera around the character.
///  - PlayerMovement (View Mode = Third Person) turns the character to face where the camera looks.
///  - The camera pulls in when a wall or object is in the way.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The Player. Found automatically by the 'Player' tag if left empty.")]
    public Transform target;
    [Tooltip("Where the camera looks at, relative to the target's feet (about shoulder/head height).")]
    public Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);
    [Tooltip("Slides the camera sideways (x) for an over-the-shoulder view. 0 = directly behind.")]
    public Vector3 shoulderOffset = new Vector3(0.6f, 0f, 0f);

    [Header("Distance")]
    public float distance = 4f;
    public bool zoomWithScroll = true;
    public float minDistance = 2f;
    public float maxDistance = 8f;
    public float zoomSpeed = 3f;

    [Header("Orbit")]
    public float sensitivity = 2f;
    public bool invertY = false;
    public float minPitch = -30f;
    public float maxPitch = 70f;
    public float startPitch = 15f;
    public bool lockCursor = true;

    [Header("Smoothing")]
    [Tooltip("How quickly the camera catches up with the character. 0 = rigid.")]
    public float followSmoothTime = 0.04f;

    [Header("Collision")]
    public LayerMask collisionMask = ~0;
    public float collisionRadius = 0.25f;
    [Tooltip("Gap kept between the camera and a wall.")]
    public float collisionBuffer = 0.1f;
    [Tooltip("How fast the camera eases back out after an obstruction clears.")]
    public float zoomOutSpeed = 8f;

    /// <summary>The camera's horizontal angle, in degrees.</summary>
    public float Yaw => yaw;

    float yaw;
    float pitch;
    float currentDistance;
    Vector3 pivot;
    Vector3 pivotVelocity;
    bool initialised;

    readonly RaycastHit[] hitBuffer = new RaycastHit[16];

    void Start()
    {
        if (!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }

        if (!target)
        {
            Debug.LogWarning("ThirdPersonCamera: no target. Assign the Player, or give it the 'Player' tag.", this);
            return;
        }

        yaw = target.eulerAngles.y;
        pitch = startPitch;
        pivot = target.position + targetOffset;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // Only orbit while the cursor is locked (so an open inventory or menu doesn't spin the camera)
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity;
            pitch += (invertY ? 1f : -1f) * Input.GetAxis("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            if (zoomWithScroll)
            {
                distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Follow the character smoothly
        Vector3 desiredPivot = target.position + targetOffset;
        pivot = followSmoothTime > 0f
            ? Vector3.SmoothDamp(pivot, desiredPivot, ref pivotVelocity, followSmoothTime)
            : desiredPivot;

        // Where the camera wants to be
        Vector3 offset = rotation * new Vector3(shoulderOffset.x, shoulderOffset.y, -distance);
        float desiredDistance = offset.magnitude;
        Vector3 direction = desiredDistance > 0.001f ? offset / desiredDistance : -(rotation * Vector3.forward);

        float allowed = ObstructedDistance(direction, desiredDistance);

        if (!initialised)
        {
            currentDistance = allowed;
            initialised = true;
        }

        // Snap in immediately (never clip through a wall), ease back out
        currentDistance = allowed < currentDistance
            ? allowed
            : Mathf.MoveTowards(currentDistance, allowed, zoomOutSpeed * Time.deltaTime);

        transform.position = pivot + direction * currentDistance;
        transform.rotation = rotation;
    }

    float ObstructedDistance(Vector3 direction, float maxDistanceToCheck)
    {
        int count = Physics.SphereCastNonAlloc(
            pivot, collisionRadius, direction, hitBuffer, maxDistanceToCheck,
            collisionMask, QueryTriggerInteraction.Ignore);

        float allowed = maxDistanceToCheck;

        for (int i = 0; i < count; i++)
        {
            Collider col = hitBuffer[i].collider;
            if (col.transform.IsChildOf(target)) continue;   // never collide with the character itself

            float d = Mathf.Max(0.3f, hitBuffer[i].distance - collisionBuffer);
            if (d < allowed) allowed = d;
        }

        return allowed;
    }
}
