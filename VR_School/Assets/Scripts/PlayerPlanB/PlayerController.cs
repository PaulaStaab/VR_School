using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    // Main CharacterController used for movement and collision.
    [SerializeField] private CharacterController controller;

    // Transform used for ground detection checks.
    [SerializeField] private Transform groundCheck;

    [Header("Camera")]
    // Reference to the player camera transform.
    [SerializeField] private Transform playerCamera;

    [Header("Movement")]
    // Default movement speed while walking.
    [SerializeField] private float walkSpeed = 6f;

    // Reduced movement speed while sneaking or crouching.
    [SerializeField] private float sneakSpeed = 2f;

    [Header("Crouch")]
    // CharacterController height while crouching.
    [SerializeField] private float crouchHeight = 1f;

    // CharacterController height while standing.
    [SerializeField] private float standingHeight = 2f;

    // Target camera Y offset while crouching.
    [SerializeField] private float crouchCameraY = 0.9f;

    // Target camera Y offset while standing.
    [SerializeField] private float standingCameraY = 1.6f;

    // Interpolation speed used when changing crouch height.
    [SerializeField] private float crouchSpeed = 10f;

    [Header("Jump & Gravity")]
    // Gravity force applied to the player.
    [SerializeField] private float gravity = -20f;

    // Jump height used to calculate jump velocity.
    [SerializeField] private float jumpHeight = 1.5f;

    // Extra gravity multiplier applied while falling.
    [SerializeField] private float fallMultiplier = 5f;

    [Header("Ground Check")]
    // Radius of the sphere used for ground detection.
    [SerializeField] private float groundDistance = 0.4f;

    // Layer mask used to determine what counts as ground.
    [SerializeField] private LayerMask groundMask;

    // Input actions instance used for player input handling.
    private InputSystem_Actions controls;

    // Cached movement input from the input system.
    private Vector2 moveInput;

    // Current vertical velocity used for gravity and jumping.
    private Vector3 velocity;

    // True while the player is currently grounded.
    public bool IsGrounded { get; private set; }

    // True while the player is currently sneaking.
    public bool IsSneaking { get; private set; }

    // True while the player is currently crouching.
    public bool IsCrouching { get; private set; }

    // Public read-only access to the raw move input.
    public Vector2 MoveInput => moveInput;

    // Returns the player's current world-space movement velocity based on input and speed.
    public Vector3 WorldMoveVelocity
    {
        get
        {
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            return move * CurrentSpeed;
        }
    }

    // Returns the current movement speed depending on sneak / crouch state.
    private float CurrentSpeed => (IsSneaking || IsCrouching) ? sneakSpeed : walkSpeed;

    private void Awake()
    {
        // Create a new input action instance.
        controls = new InputSystem_Actions();

        // Cache move input when movement is performed.
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();

        // Reset move input when movement input is canceled.
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Trigger jump input.
        controls.Player.Jump.performed += ctx => Jump();

        // Enable sneak while the input is held.
        controls.Player.Sneak.performed += ctx => IsSneaking = true;

        // Disable sneak when the input is released.
        controls.Player.Sneak.canceled += ctx => IsSneaking = false;

        // Toggle crouch state when crouch input is performed.
        controls.Player.Crouch.performed += ctx => ToggleCrouch();
    }

    private void OnEnable() => controls.Enable();

    private void OnDisable() => controls.Disable();

    private void Update()
    {
        // Process all core player systems every frame.
        HandleGroundCheck();
        HandleMovement();
        HandleCrouch();
        ApplyGravity();
    }
    
    private void HandleGroundCheck()
    {
        // Use a sphere check to determine whether the player is grounded.
        IsGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        // Keep a small downward velocity while grounded
        // so the CharacterController stays properly attached to the ground.
        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        // Convert input into movement relative to the player's orientation.
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Apply horizontal movement through the CharacterController.
        controller.Move(move * CurrentSpeed * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        // Smoothly interpolate the controller height toward the crouch / stand target.
        float targetHeight = IsCrouching ? crouchHeight : standingHeight;
        float currentHeight = controller.height;

        controller.height = Mathf.Lerp(currentHeight, targetHeight, crouchSpeed * Time.deltaTime);

        // Keep the controller center aligned with the current height.
        controller.center = new Vector3(0f, controller.height / 2f, 0f);

        // Compute the target camera height for crouch / stand state.
        // Currently prepared for later visual camera interpolation.
        float targetCamY = IsCrouching ? crouchCameraY : standingCameraY;
    }

    private void ToggleCrouch()
    {
        // Before standing up again, check whether enough space is available.
        if (IsCrouching)
        {
            if (!CanStandUp())
                return;
        }

        // Toggle crouch state.
        IsCrouching = !IsCrouching;
    }

    private bool CanStandUp()
    {
        // Placeholder for future ceiling / obstacle checks.
        return true;
    }

    private void ApplyGravity()
    {
        // Apply stronger gravity while falling for snappier jump behavior.
        if (velocity.y < 0f)
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Apply vertical velocity through the CharacterController.
        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        // The player can only jump while grounded and not crouching.
        if (!IsGrounded || IsCrouching)
            return;

        // Calculate jump velocity based on the configured jump height and gravity.
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        IsGrounded = false;
    }
}