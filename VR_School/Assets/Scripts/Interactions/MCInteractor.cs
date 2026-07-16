/*using UnityEngine;

public class MCInteractor : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Interaction UI")]
    [SerializeField] private MCInteractUI interactUI;

    [Header("Proximity Detection")]
    [SerializeField] private float showDistance = 1.6f;
    [SerializeField] private float hideDistance = 2.0f;
    [SerializeField] private float switchDistanceAdvantage = 0.35f;

    [Header("Search")]
    [SerializeField] private float refreshInterval = 1.0f;

    private MCInteractable[] interactables;
    private MCInteractable currentInteractable;

    private bool menuIsOpen;
    private float nextRefreshTime;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponent<Camera>();

        RefreshInteractables();
    }

    private void Start()
    {
        SetGameplayCursor();
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        if (interactUI == null)
            return;

        if (Time.time >= nextRefreshTime)
            RefreshInteractables();

        if (menuIsOpen)
            UpdateOpenMenu();
        else
            TryOpenNearestMenu();
    }

    private void TryOpenNearestMenu()
    {
        MCInteractable nearest = FindNearestInteractableInRange(showDistance, null, out _);

        if (nearest == null)
            return;

        OpenMenu(nearest);
    }

    private void UpdateOpenMenu()
    {
        if (currentInteractable == null)
        {
            CloseMenu();
            return;
        }

        if (!currentInteractable.gameObject.activeInHierarchy)
        {
            CloseMenu();
            return;
        }

        if (!currentInteractable.enabled)
        {
            CloseMenu();
            return;
        }

        if (!currentInteractable.HasAnyAction())
        {
            CloseMenu();
            return;
        }

        float currentDistance = GetDistanceTo(currentInteractable);

        if (currentDistance > hideDistance)
        {
            CloseMenu();
            return;
        }

        MCInteractable nearest = FindNearestInteractableInRange(
            showDistance,
            currentInteractable,
            out float nearestDistance
        );

        if (nearest == null)
            return;

        bool nearestIsClearlyCloser = nearestDistance < currentDistance - switchDistanceAdvantage;

        if (!nearestIsClearlyCloser)
            return;

        OpenMenu(nearest);
    }

    private void OpenMenu(MCInteractable interactable)
    {
        if (interactable == null)
            return;

        currentInteractable = interactable;
        menuIsOpen = true;

        if (interactUI != null)
            interactUI.Show(currentInteractable);

        SetMenuCursor();
    }

    private MCInteractable FindNearestInteractableInRange(
        float maxDistance,
        MCInteractable ignoreInteractable,
        out float nearestDistance
    )
    {
        nearestDistance = float.MaxValue;

        if (interactables == null)
            return null;

        MCInteractable nearest = null;

        for (int i = 0; i < interactables.Length; i++)
        {
            MCInteractable interactable = interactables[i];

            if (interactable == null)
                continue;

            if (interactable == ignoreInteractable)
                continue;

            if (!interactable.gameObject.activeInHierarchy)
                continue;

            if (!interactable.enabled)
                continue;

            if (!interactable.HasAnyAction())
                continue;

            float distance = GetDistanceTo(interactable);

            if (distance > maxDistance)
                continue;

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearest = interactable;
        }

        return nearest;
    }

    private float GetDistanceTo(MCInteractable interactable)
    {
        if (interactable == null)
            return float.MaxValue;

        return Vector3.Distance(
            playerCamera.transform.position,
            interactable.InteractionWorldPosition
        );
    }

    private void CloseMenu()
    {
        currentInteractable = null;
        menuIsOpen = false;

        if (interactUI != null)
            interactUI.Hide();

        SetGameplayCursor();
    }

    private void SetMenuCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetGameplayCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    [ContextMenu("Refresh Interactables")]
    public void RefreshInteractables()
    {
        interactables = FindObjectsByType<MCInteractable>(FindObjectsInactive.Exclude);
        nextRefreshTime = Time.time + refreshInterval;
    }
}*/
using UnityEngine;

public class MCInteractor : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("Interaction UI")]
    [SerializeField] private MCInteractUI interactUI;

    [Header("Proximity Detection")]
    [SerializeField] private float showDistance = 1.6f;
    [SerializeField] private float hideDistance = 2.0f;

    [Header("Old Single Panel Setting")]
    [SerializeField] private float switchDistanceAdvantage = 0.35f;

    [Header("Search")]
    [SerializeField] private float refreshInterval = 1.0f;

    private MCInteractable[] interactables;

    private bool cursorIsMenu;
    private float nextRefreshTime;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponent<Camera>();

        RefreshInteractables();
    }

    private void Start()
    {
        SetGameplayCursor();
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        if (interactUI == null)
            return;

        if (Time.time >= nextRefreshTime)
            RefreshInteractables();

        UpdateInteractablePanels();
        UpdateCursorState();
    }

    private void UpdateInteractablePanels()
    {
        if (interactables == null)
            return;

        for (int i = 0; i < interactables.Length; i++)
        {
            MCInteractable interactable = interactables[i];

            if (!IsInteractableValid(interactable))
            {
                if (interactUI.IsShowing(interactable))
                    interactUI.Hide(interactable);

                continue;
            }

            float distance = GetDistanceTo(interactable);
            bool panelIsShowing = interactUI.IsShowing(interactable);

            if (panelIsShowing)
            {
                if (distance > hideDistance)
                    interactUI.Hide(interactable);

                continue;
            }

            if (distance <= showDistance)
                interactUI.Show(interactable);
        }
    }

    private bool IsInteractableValid(MCInteractable interactable)
    {
        if (interactable == null)
            return false;

        if (!interactable.gameObject.activeInHierarchy)
            return false;

        if (!interactable.enabled)
            return false;

        if (!interactable.HasAnyAction())
            return false;

        return true;
    }

    private float GetDistanceTo(MCInteractable interactable)
    {
        if (interactable == null)
            return float.MaxValue;

        return Vector3.Distance(
            playerCamera.transform.position,
            interactable.InteractionWorldPosition
        );
    }

    private void UpdateCursorState()
    {
        bool shouldUseMenuCursor = interactUI != null && interactUI.HasVisiblePanels;

        if (shouldUseMenuCursor == cursorIsMenu)
            return;

        if (shouldUseMenuCursor)
            SetMenuCursor();
        else
            SetGameplayCursor();
    }

    private void SetMenuCursor()
    {
        cursorIsMenu = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetGameplayCursor()
    {
        cursorIsMenu = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnValidate()
    {
        if (showDistance < 0f)
            showDistance = 0f;

        if (hideDistance < 0f)
            hideDistance = 0f;

        if (hideDistance < showDistance)
            hideDistance = showDistance;

        if (switchDistanceAdvantage < 0f)
            switchDistanceAdvantage = 0f;

        if (refreshInterval < 0.05f)
            refreshInterval = 0.05f;
    }

    [ContextMenu("Refresh Interactables")]
    public void RefreshInteractables()
    {
        interactables = FindObjectsByType<MCInteractable>(FindObjectsInactive.Exclude);
        nextRefreshTime = Time.time + refreshInterval;
    }
}