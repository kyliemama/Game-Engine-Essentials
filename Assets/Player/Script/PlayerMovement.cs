using UnityEngine;

/// <summary>
/// WASD movement using a CharacterController, for first OR third person.
/// Put this on the Player root (with a CharacterController).
/// First person: mouse look is handled here and cameraPivot holds the camera.
/// Third person: ThirdPersonCamera handles the mouse; the character turns to face where the camera looks.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public enum ViewMode { FirstPerson, ThirdPerson }

    [Header("View")]
    public ViewMode viewMode = ViewMode.ThirdPerson;
    [Tooltip("Third person: the camera the character turns to face. Found automatically (Main Camera) if empty.")]
    public Transform cameraTransform;
    [Tooltip("Third person: how fast the character turns to face the camera direction, in degrees per second.")]
    public float turnSpeed = 720f;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float acceleration = 12f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Look")]
    public Transform cameraPivot;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;
    public bool lockCursor = true;

    [Header("Animation (optional)")]
    public Animator animator;
    public float animDamp = 0.1f;

    CharacterController cc;
    Vector3 planarVelocity;
    float verticalVelocity;
    float pitch;

    static readonly int MoveX = Animator.StringToHash("MoveX");
    static readonly int MoveZ = Animator.StringToHash("MoveZ");
    static readonly int Grounded = Animator.StringToHash("Grounded");
    static readonly int Jump = Animator.StringToHash("Jump");

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (!InventoryUI.IsOpen) Look();
        Move();
    }

    void Look()
    {
        if (viewMode == ViewMode.ThirdPerson)
        {
            // ThirdPersonCamera reads the mouse. The character just turns to face where the camera looks.
            if (!cameraTransform) return;

            float yaw = Mathf.MoveTowardsAngle(
                transform.eulerAngles.y, cameraTransform.eulerAngles.y, turnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            return;
        }

        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mx);

        pitch = Mathf.Clamp(pitch - my, -maxLookAngle, maxLookAngle);
        if (cameraPivot) cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A / D
        float v = Input.GetAxisRaw("Vertical");   // W / S

        Vector3 input = new Vector3(h, 0f, v);
        input = Vector3.ClampMagnitude(input, 1f);

        bool sprinting = Input.GetKey(KeyCode.LeftShift) && v > 0f;
        float targetSpeed = sprinting ? sprintSpeed : walkSpeed;

        Vector3 targetVelocity = transform.TransformDirection(input) * targetSpeed;
        planarVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, acceleration * Time.deltaTime * targetSpeed);

        // Gravity + jump
        if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;

        if (cc.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator) animator.SetTrigger(Jump);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 motion = planarVelocity + Vector3.up * verticalVelocity;
        cc.Move(motion * Time.deltaTime);

        // Animator (blend tree driven by local-space movement, -1..1 range)
        if (animator)
        {
            Vector3 local = transform.InverseTransformDirection(planarVelocity) / sprintSpeed;
            animator.SetFloat(MoveX, local.x, animDamp, Time.deltaTime);
            animator.SetFloat(MoveZ, local.z, animDamp, Time.deltaTime);
            animator.SetBool(Grounded, cc.isGrounded);
        }
    }
}