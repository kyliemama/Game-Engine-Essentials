using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// First-person camera polish. Put this on the Main Camera.
/// Hierarchy: Player (CharacterController + PlayerMovement) -> CameraPivot -> Main Camera.
/// Mouse look is already handled by PlayerMovement (it rotates the player and CameraPivot),
/// so this script only adds head bob, sprint FOV, strafe tilt and a landing dip.
/// </summary>
[RequireComponent(typeof(Camera))]
public class FPSCamera : MonoBehaviour
{
    [Header("References (auto-found on the parent if left empty)")]
    public PlayerMovement player;
    public CharacterController controller;

    [Header("Hide the head from the first-person view")]
    [Tooltip("Drag the Head, Hair, Eyebrow and Scarf renderers here.")]
    public Renderer[] headRenderers;
    public float nearClip = 0.1f;

    [Header("Field of View")]
    public float baseFov = 75f;
    public float sprintFov = 85f;
    public float fovSpeed = 8f;

    [Header("Head Bob")]
    public bool enableBob = true;
    public float bobFrequency = 9f;
    public float bobAmountX = 0.03f;
    public float bobAmountY = 0.05f;
    public float bobSmoothing = 10f;

    [Header("Strafe Tilt")]
    public float maxTilt = 2f;      // degrees of roll when strafing
    public float tiltSpeed = 8f;

    [Header("Landing Dip")]
    public float landDipAmount = 0.012f; // metres per m/s of fall speed
    public float maxLandDip = 0.15f;
    public float landRecovery = 8f;

    Camera cam;
    Vector3 basePosition;
    Vector3 bobOffset;
    float bobTimer;
    float landOffset;
    float roll;
    float lastYVelocity;
    bool wasGrounded = true;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!player) player = GetComponentInParent<PlayerMovement>();
        if (!controller) controller = GetComponentInParent<CharacterController>();

        basePosition = transform.localPosition;
        cam.nearClipPlane = nearClip;
        cam.fieldOfView = baseFov;

        HideHead();
    }

    void HideHead()
    {
        // Shadows Only: the head still casts a shadow, but isn't drawn to the camera.
        foreach (Renderer r in headRenderers)
            if (r) r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }

    void LateUpdate()
    {
        if (!controller) return;

        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;
        float speed = horizontalVelocity.magnitude;

        float walkSpeed = player ? player.walkSpeed : 5f;
        float sprintSpeed = player ? player.sprintSpeed : 8f;
        bool sprinting = speed > (walkSpeed + sprintSpeed) * 0.5f;

        UpdateFov(sprinting);
        UpdateBob(speed, walkSpeed, sprintSpeed);
        UpdateTilt(horizontalVelocity, sprintSpeed);
        UpdateLanding();

        Vector3 target = basePosition + bobOffset + Vector3.up * landOffset;
        transform.localPosition = target;
        transform.localRotation = Quaternion.Euler(0f, 0f, roll);
    }

    void UpdateFov(bool sprinting)
    {
        float targetFov = sprinting ? sprintFov : baseFov;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, fovSpeed * Time.deltaTime);
    }

    void UpdateBob(float speed, float walkSpeed, float sprintSpeed)
    {
        Vector3 targetBob = Vector3.zero;

        if (enableBob && controller.isGrounded && speed > 0.1f)
        {
            bobTimer += Time.deltaTime * bobFrequency * (speed / walkSpeed);
            float strength = Mathf.Clamp01(speed / sprintSpeed);

            targetBob = new Vector3(
                Mathf.Cos(bobTimer * 0.5f) * bobAmountX,
                Mathf.Sin(bobTimer) * bobAmountY,
                0f) * strength;
        }

        bobOffset = Vector3.Lerp(bobOffset, targetBob, bobSmoothing * Time.deltaTime);
    }

    void UpdateTilt(Vector3 horizontalVelocity, float sprintSpeed)
    {
        // Sideways speed relative to where the player is facing
        float sideways = Vector3.Dot(horizontalVelocity, transform.parent ? transform.parent.right : transform.right);
        float targetRoll = -Mathf.Clamp(sideways / sprintSpeed, -1f, 1f) * maxTilt;
        roll = Mathf.Lerp(roll, targetRoll, tiltSpeed * Time.deltaTime);
    }

    void UpdateLanding()
    {
        bool grounded = controller.isGrounded;

        if (!grounded)
        {
            lastYVelocity = controller.velocity.y;
        }
        else if (!wasGrounded)
        {
            float impact = Mathf.Clamp(-lastYVelocity * landDipAmount, 0f, maxLandDip);
            landOffset = -impact;
        }

        wasGrounded = grounded;
        landOffset = Mathf.Lerp(landOffset, 0f, landRecovery * Time.deltaTime);
    }
}
