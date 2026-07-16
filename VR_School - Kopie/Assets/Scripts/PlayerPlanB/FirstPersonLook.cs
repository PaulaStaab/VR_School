using System.Collections;
using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    // Reference to the player body, used for horizontal rotation.
    [SerializeField] private Transform playerBody;

    // Reference to the player controller, used for crouch, ground and movement checks.
    [SerializeField] private PlayerController playerController;

    [Header("Mouse Sensitivity")]
    // Base horizontal mouse sensitivity.
    [SerializeField] private float senseX = 150f;

    // Base vertical mouse sensitivity.
    [SerializeField] private float senseY = 150f;

    [Header("Camera Smoothing")]
    // Smoothing time for mouse look input.
    [SerializeField] private float lookSmoothTime = 0.035f;

    [Header("Vertical Look Limits")]
    // Maximum upward look angle.
    [SerializeField] private float topLookAngle = -62f;

    // Maximum downward look angle.
    [SerializeField] private float bottomLookAngle = 45f;

    [Header("Lean Settings")]
    // Horizontal camera offset while leaning.
    [SerializeField] private float leanAmount = 0.3f;

    // Camera roll angle while leaning.
    [SerializeField] private float leanAngle = 12f;

    // Interpolation speed for leaning.
    [SerializeField] private float leanSpeed = 8f;

    [Header("Lean Collision")]
    // Radius of the sphere used to prevent leaning through walls.
    [SerializeField] private float leanSphereRadius = 0.2f;

    // Layer mask used for lean obstacle checks.
    [SerializeField] private LayerMask leanObstacleMask;

    [Header("Crouch Settings")]
    // Camera Y position while standing.
    [SerializeField] private float standingY = 1.6f;

    // Camera Y position while crouching.
    [SerializeField] private float crouchY = 0.9f;

    // Interpolation speed for crouch camera height changes.
    [SerializeField] private float crouchSpeed = 10f;

    [Header("Debug")]
    // Enables lean gizmo visualization in the Scene view.
    [SerializeField] private bool showLeanDebug = true;

    // Input actions instance used for look and lean input.
    private InputSystem_Actions controls;

    // Raw look input from the input system.
    private Vector2 lookInput;

    // Smoothed look input used for camera motion.
    private Vector2 smoothedLookInput;

    // Velocity value used internally by SmoothDamp.
    private Vector2 lookSmoothVelocity;

    // Current vertical camera rotation.
    private float xRotation;

    // Cached base sensitivities before settings modifications.
    private float baseSenseX;
    private float baseSenseY;

    // Runtime sensitivities after settings have been applied.
    private float currentSenseX;
    private float currentSenseY;

    // Whether vertical mouse input should be inverted.
    private bool invertY;

    // Current interpolated lean value.
    private float currentLean;

    // Desired lean direction: -1 left, 1 right, 0 center.
    private float targetLean;

    // Current interpolated camera Y position.
    private float currentY;

    // Initial local camera position used as the base for lean and crouch offsets.
    private Vector3 initialLocalPosition;

    // Public read-only access to the vertical look rotation.
    public float CurrentXRotation => xRotation;

    private void Awake()
    {
        // Store the base sensitivities so they can be modified by settings later.
        baseSenseX = senseX;
        baseSenseY = senseY;

        currentSenseX = baseSenseX;
        currentSenseY = baseSenseY;

        // Create the input actions instance.
        controls = new InputSystem_Actions();

        // Cache look input when mouse / stick look is performed.
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();

        // Reset look input when the action is canceled.
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable() => controls.Enable();

    private void OnDisable() => controls.Disable();

    private void Start()
    {
        // Lock and hide the cursor for first-person gameplay.
        LockCursor();

        // Cache the starting local camera position.
        initialLocalPosition = transform.localPosition;

        // Initialize the current camera height to the standing value.
        currentY = standingY;

        // Snap the camera to the standing Y position at startup.
        Vector3 pos = transform.localPosition;
        pos.y = standingY;
        transform.localPosition = pos;
    }

     public void ApplyMouseSettings(float sensitivityOffset, bool invertYSetting)
    {
        // Convert the sensitivity offset into a multiplier.
        float multiplier = 1f + sensitivityOffset;

        currentSenseX = baseSenseX * multiplier;
        currentSenseY = baseSenseY * multiplier;

        invertY = invertYSetting;
    }

    private void Update()
    {
        // Process leaning input, mouse look and final lean/crouch application.
        HandleLeanInput();
        HandleLookInput();
        ApplyLean();
    }

    private void HandleLookInput()
    {
        Vector2 processedLookInput;

        // Smooth the look input if smoothing is enabled.
        if (lookSmoothTime > 0.0001f)
        {
            smoothedLookInput = Vector2.SmoothDamp(
                smoothedLookInput,
                lookInput,
                ref lookSmoothVelocity,
                lookSmoothTime
            );

            processedLookInput = smoothedLookInput;
        }
        else
        {
            processedLookInput = lookInput;
        }

        // Convert input into horizontal and vertical mouse movement.
        float mouseX = processedLookInput.x * currentSenseX * Time.deltaTime;
        float mouseY = processedLookInput.y * currentSenseY * Time.deltaTime;

        // Invert vertical look if enabled.
        if (invertY)
            mouseY *= -1f;

        // Apply and clamp vertical rotation.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topLookAngle, bottomLookAngle);

        // Apply vertical look and lean roll to the camera.
        transform.localRotation = Quaternion.Euler(xRotation, 0f, currentLean * leanAngle);

        // Apply horizontal look to the player body.
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleLeanInput()
    {
        // Leaning is only allowed in specific movement states.
        if (!CanLean())
        {
            targetLean = 0f;
            return;
        }

        // Read lean input from the input system.
        if (controls.Player.LeanLeft.IsPressed())
            targetLean = -1f;
        else if (controls.Player.LeanRight.IsPressed())
            targetLean = 1f;
        else
            targetLean = 0f;
    }

    private void ApplyLean()
    {
        // Reduce the desired lean if a wall or obstacle blocks it.
        float allowedLean = GetAllowedLeanAmount(targetLean);

        // Smoothly interpolate toward the allowed lean amount.
        currentLean = Mathf.Lerp(currentLean, allowedLean, leanSpeed * Time.deltaTime);

        // Adjust camera height depending on crouch state.
        float targetY = playerController != null && playerController.IsCrouching
            ? crouchY
            : standingY;

        currentY = Mathf.Lerp(currentY, targetY, crouchSpeed * Time.deltaTime);

        // Apply final local position using lean offset and crouch height.
        transform.localPosition = new Vector3(
            initialLocalPosition.x + currentLean * leanAmount,
            currentY,
            initialLocalPosition.z
        );
    }

    private float GetAllowedLeanAmount(float desiredLean)
    {
        // No lean needed if centered or if the camera has no parent transform.
        if (desiredLean == 0f || transform.parent == null)
            return 0f;

        // Build cast data for the desired lean direction.
        GetLeanCastData(desiredLean, out Vector3 start, out Vector3 target, out Vector3 dir, out float dist);

        if (dist <= 0.0001f)
            return desiredLean;

        // Use a sphere cast to detect obstacles between the base and target lean positions.
        if (Physics.SphereCast(
                start,
                leanSphereRadius,
                dir,
                out RaycastHit hit,
                dist,
                leanObstacleMask,
                QueryTriggerInteraction.Ignore))
        {
            // Scale the lean amount down to the safe fraction before collision.
            float safe = hit.distance / dist;
            return desiredLean * safe;
        }

        return desiredLean;
    }

    private void GetLeanCastData(float lean, out Vector3 start, out Vector3 target, out Vector3 dir, out float dist)
    {
        // Use the cached base position during play mode, otherwise the current local position.
        Vector3 baseLocal = Application.isPlaying ? initialLocalPosition : transform.localPosition;

        // Build the leaned local target position.
        Vector3 targetLocal = baseLocal + new Vector3(lean * leanAmount, 0f, 0f);

        // Convert both positions into world space.
        start = transform.parent.TransformPoint(baseLocal);
        target = transform.parent.TransformPoint(targetLocal);

        // Calculate cast direction and distance.
        Vector3 delta = target - start;
        dist = delta.magnitude;
        dir = dist > 0.0001f ? delta / dist : Vector3.zero;
    }

    private bool CanLean()
    {
        // Leaning is only allowed if the player controller exists.
        if (playerController == null)
            return false;

        // Leaning is only allowed while grounded.
        if (!playerController.IsGrounded)
            return false;

        // Leaning is blocked while moving forward or backward.
        if (Mathf.Abs(playerController.MoveInput.y) > 0.01f)
            return false;

        return true;
    }

    private void LockCursor()
    {
        // Lock and hide the mouse cursor for first-person control.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Stop if debug visualization is disabled or no parent exists.
        if (!showLeanDebug || transform.parent == null)
            return;

        // Use the cached base position during play mode, otherwise current position.
        Vector3 baseLocal = Application.isPlaying ? initialLocalPosition : transform.localPosition;
        Vector3 center = transform.parent.TransformPoint(baseLocal);

        // Draw the center sphere at the base camera position.
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(center, leanSphereRadius);

        // Draw the full left lean cast and target sphere.
        GetLeanCastData(-1f, out Vector3 ls, out Vector3 lt, out _, out _);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(ls, lt);
        Gizmos.DrawWireSphere(lt, leanSphereRadius);

        // Draw the full right lean cast and target sphere.
        GetLeanCastData(1f, out Vector3 rs, out Vector3 rt, out _, out _);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(rs, rt);
        Gizmos.DrawWireSphere(rt, leanSphereRadius);
    }
}