///*using UnityEngine;

//// Handles first-person style mouse look rotation and clamps vertical camera movement.
//public class MouseLook : MonoBehaviour
//{
//    [Header("Einstellungen")]
//    // Horizontal mouse sensitivity.
//    public float mouseSensitivityX = 250f;
//    // Vertical mouse sensitivity.
//    public float mouseSensitivityY = 250f;

//    // Minimum vertical look angle.
//    public float minimumY = -60f;
//    // Maximum vertical look angle.
//    public float maximumY = 60f;
//    // Stores the current vertical rotation value.
//    private float rotationY = 0f;

//    void Update()
//    {
//        // Read mouse movement input and scale it by sensitivity and frame time.
//        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
//        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

//        // Update and clamp the vertical rotation.
//        rotationY -= mouseY;
//        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

//        // Apply the combined vertical and horizontal local rotation.
//        transform.localRotation = Quaternion.Euler(rotationY, transform.localEulerAngles.y + mouseX, 0);
//    }

//using UnityEngine;
//using UnityEngine.InputSystem;

//public class MouseLook : MonoBehaviour
//{
//    public float mouseSensitivityX = 250f;
//    public float mouseSensitivityY = 250f;
//    public float minimumY = -60f;
//    public float maximumY = 60f;

//    private Vector2 lookInput;
//    private float rotationY;

//    public void OnLook(InputValue value)
//    {
//        lookInput = value.Get<Vector2>();
//    }

//    private void Start()
//    {
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }

//    private void Update()
//    {
//        float mouseX = lookInput.x * mouseSensitivityX * Time.deltaTime;
//        float mouseY = lookInput.y * mouseSensitivityY * Time.deltaTime;

//        transform.parent.Rotate(Vector3.up * mouseX);

//        rotationY -= mouseY;
//        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

//        transform.localRotation = Quaternion.Euler(rotationY, 0f, 0f);
//    }
//}
///*void Start()
//    {
//        // Lock and hide the cursor when the scene starts.
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }
//}*/

//using UnityEngine;
//using UnityEngine.InputSystem;

//public class MouseLook : MonoBehaviour
//{
//    [Header("Referenzen")]
//    public Transform cameraTransform;

//    [Header("Einstellungen")]
//    public float mouseSensitivityX = 250f;
//    public float mouseSensitivityY = 250f;
//    public float minimumY = -60f;
//    public float maximumY = 60f;

//    private Vector2 lookInput;
//    private float cameraPitch;

//    public void OnLook(InputValue value)
//    {
//        lookInput = value.Get<Vector2>();
//    }

//    private void Start()
//    {
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }

//    private void Update()
//    {
//        float mouseX = lookInput.x * mouseSensitivityX * Time.deltaTime;
//        float mouseY = lookInput.y * mouseSensitivityY * Time.deltaTime;

//        // Player links/rechts drehen
//        transform.Rotate(Vector3.up * mouseX);

//        // Kamera hoch/runter drehen
//        cameraPitch -= mouseY;
//        cameraPitch = Mathf.Clamp(cameraPitch, minimumY, maximumY);

//        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
//    }
//}

using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Referenzen")]
    [SerializeField] private Transform cameraTransform;

    [Header("Einstellungen")]
    [SerializeField] private float mouseSensitivityX = 0.15f;
    [SerializeField] private float mouseSensitivityY = 0.15f;
    [SerializeField] private float minimumY = -80f;
    [SerializeField] private float maximumY = 80f;
    [SerializeField] private bool invertY = false;

    private Vector2 lookInput;
    private float cameraPitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        RotatePlayerAndCamera();
        HandleCursorUnlock();
    }

    private void RotatePlayerAndCamera()
    {
        float mouseX = lookInput.x * mouseSensitivityX;
        float mouseY = lookInput.y * mouseSensitivityY;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch += invertY ? mouseY : -mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minimumY, maximumY);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void HandleCursorUnlock()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
