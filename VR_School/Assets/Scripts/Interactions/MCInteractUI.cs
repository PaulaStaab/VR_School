/*using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MCInteractUI : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("UI Root")]
    [SerializeField] private GameObject interactionPanel;

    [Header("Buttons")]
    [SerializeField] private Button button1;
    [SerializeField] private TMP_Text button1Text;

    [SerializeField] private Button button2;
    [SerializeField] private TMP_Text button2Text;

    [SerializeField] private Button button3;
    [SerializeField] private TMP_Text button3Text;

    [SerializeField] private Button button4;
    [SerializeField] private TMP_Text button4Text;

    private MCInteractable currentInteractable;

    public bool IsVisible
    {
        get
        {
            return interactionPanel != null && interactionPanel.activeSelf;
        }
    }

    private void Awake()
    {
        ApplyCanvasCamera();
        Hide();
    }

    private void LateUpdate()
    {
        if (!IsVisible)
            return;

        if (currentInteractable == null)
        {
            Hide();
            return;
        }

        FollowCurrentInteractable();
    }

    public void Show(MCInteractable interactable)
    {
        if (interactable == null)
        {
            Hide();
            return;
        }

        if (!interactable.HasAnyAction())
        {
            Hide();
            return;
        }

        currentInteractable = interactable;

        SetupButton(button1, button1Text, currentInteractable.GetAction(0));
        SetupButton(button2, button2Text, currentInteractable.GetAction(1));
        SetupButton(button3, button3Text, currentInteractable.GetAction(2));
        SetupButton(button4, button4Text, currentInteractable.GetAction(3));

        FollowCurrentInteractable();

        if (interactionPanel != null)
            interactionPanel.SetActive(true);

        RebuildLayout();
    }

    public void Hide()
    {
        currentInteractable = null;

        ClearButton(button1);
        ClearButton(button2);
        ClearButton(button3);
        ClearButton(button4);

        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }

    private void FollowCurrentInteractable()
    {
        if (currentInteractable == null)
            return;

        Vector3 targetPosition = currentInteractable.GetMenuWorldPosition();
        Quaternion anchorRotation = currentInteractable.GetMenuWorldRotation();

        float faceAmount = currentInteractable.FaceCameraAmount;

        Quaternion finalRotation = anchorRotation;

        if (faceAmount > 0f && playerCamera != null)
        {
            Quaternion cameraRotation = GetCameraFacingRotation(targetPosition);

            finalRotation = Quaternion.Slerp(
                anchorRotation,
                cameraRotation,
                faceAmount
            );
        }

        transform.SetPositionAndRotation(targetPosition, finalRotation);
    }

    private Quaternion GetCameraFacingRotation(Vector3 menuPosition)
    {
        if (playerCamera == null)
            return transform.rotation;

        Vector3 direction = menuPosition - playerCamera.transform.position;

        if (direction.sqrMagnitude <= 0.0001f)
            return transform.rotation;

        return Quaternion.LookRotation(direction, Vector3.up);
    }

    private void SetupButton(Button button, TMP_Text buttonText, MCInteractable.MCAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();

        if (action == null || !action.HasAction())
        {
            button.gameObject.SetActive(false);
            return;
        }

        button.gameObject.SetActive(true);

        if (buttonText != null)
            buttonText.text = action.ButtonText;

        button.onClick.AddListener(action.Invoke);
    }

    private void ClearButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.gameObject.SetActive(false);
    }

    private void RebuildLayout()
    {
        if (interactionPanel == null)
            return;

        RectTransform rectTransform = interactionPanel.GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void ApplyCanvasCamera()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
            return;

        if (playerCamera == null)
            return;

        canvas.worldCamera = playerCamera;
    }
}*/
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MCInteractUI : MonoBehaviour
{
    private class PanelInstance
    {
        public MCInteractable interactable;
        public GameObject panelObject;
        public Button[] buttons = new Button[4];
        public TMP_Text[] buttonTexts = new TMP_Text[4];
    }

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;

    [Header("UI Root")]
    [SerializeField] private GameObject interactionPanel;

    [Header("Buttons")]
    [SerializeField] private Button button1;
    [SerializeField] private TMP_Text button1Text;

    [SerializeField] private Button button2;
    [SerializeField] private TMP_Text button2Text;

    [SerializeField] private Button button3;
    [SerializeField] private TMP_Text button3Text;

    [SerializeField] private Button button4;
    [SerializeField] private TMP_Text button4Text;

    private readonly Dictionary<MCInteractable, PanelInstance> activePanels = new Dictionary<MCInteractable, PanelInstance>();
    private readonly List<MCInteractable> removeBuffer = new List<MCInteractable>();

    public bool IsVisible
    {
        get
        {
            return activePanels.Count > 0;
        }
    }

    public bool HasVisiblePanels
    {
        get
        {
            return activePanels.Count > 0;
        }
    }

    private void Awake()
    {
        ApplyCanvasCamera();

        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }

    private void LateUpdate()
    {
        if (activePanels.Count <= 0)
            return;

        removeBuffer.Clear();

        foreach (KeyValuePair<MCInteractable, PanelInstance> pair in activePanels)
        {
            MCInteractable interactable = pair.Key;
            PanelInstance panelInstance = pair.Value;

            if (!IsInteractableValid(interactable))
            {
                removeBuffer.Add(interactable);
                continue;
            }

            if (panelInstance == null || panelInstance.panelObject == null)
            {
                removeBuffer.Add(interactable);
                continue;
            }

            FollowInteractable(panelInstance);
        }

        for (int i = 0; i < removeBuffer.Count; i++)
        {
            Hide(removeBuffer[i]);
        }

        removeBuffer.Clear();
    }

    public void Show(MCInteractable interactable)
    {
        if (interactable == null)
        {
            HideAll();
            return;
        }

        if (!interactable.HasAnyAction())
        {
            Hide(interactable);
            return;
        }

        PanelInstance panelInstance;

        if (!activePanels.TryGetValue(interactable, out panelInstance))
        {
            panelInstance = CreatePanelInstance(interactable);

            if (panelInstance == null)
                return;

            activePanels.Add(interactable, panelInstance);
        }

        SetupButton(panelInstance.buttons[0], panelInstance.buttonTexts[0], interactable.GetAction(0));
        SetupButton(panelInstance.buttons[1], panelInstance.buttonTexts[1], interactable.GetAction(1));
        SetupButton(panelInstance.buttons[2], panelInstance.buttonTexts[2], interactable.GetAction(2));
        SetupButton(panelInstance.buttons[3], panelInstance.buttonTexts[3], interactable.GetAction(3));

        FollowInteractable(panelInstance);

        if (panelInstance.panelObject != null)
            panelInstance.panelObject.SetActive(true);

        RebuildLayout(panelInstance.panelObject);
    }

    public void Hide(MCInteractable interactable)
    {
        if (interactable == null)
            return;

        PanelInstance panelInstance;

        if (!activePanels.TryGetValue(interactable, out panelInstance))
            return;

        ClearPanel(panelInstance);

        if (panelInstance.panelObject != null)
            Destroy(panelInstance.panelObject);

        activePanels.Remove(interactable);
    }

    public void Hide()
    {
        HideAll();
    }

    public void HideAll()
    {
        foreach (KeyValuePair<MCInteractable, PanelInstance> pair in activePanels)
        {
            ClearPanel(pair.Value);

            if (pair.Value != null && pair.Value.panelObject != null)
                Destroy(pair.Value.panelObject);
        }

        activePanels.Clear();

        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }

    public bool IsShowing(MCInteractable interactable)
    {
        if (interactable == null)
            return false;

        return activePanels.ContainsKey(interactable);
    }

    private PanelInstance CreatePanelInstance(MCInteractable interactable)
    {
        if (interactionPanel == null)
            return null;

        Transform parent = interactionPanel.transform.parent;

        GameObject panelObject = Instantiate(interactionPanel, parent);
        panelObject.name = interactionPanel.name + "_" + interactable.name;
        panelObject.SetActive(false);

        PanelInstance panelInstance = new PanelInstance();
        panelInstance.interactable = interactable;
        panelInstance.panelObject = panelObject;

        CacheButtonReferences(panelInstance);

        return panelInstance;
    }

    private void CacheButtonReferences(PanelInstance panelInstance)
    {
        if (panelInstance == null || panelInstance.panelObject == null)
            return;

        Button[] templateButtons = new Button[]
        {
            button1,
            button2,
            button3,
            button4
        };

        TMP_Text[] templateTexts = new TMP_Text[]
        {
            button1Text,
            button2Text,
            button3Text,
            button4Text
        };

        Transform cloneRoot = panelInstance.panelObject.transform;

        for (int i = 0; i < 4; i++)
        {
            panelInstance.buttons[i] = FindClonedComponent(cloneRoot, templateButtons[i]);
            panelInstance.buttonTexts[i] = FindClonedComponent(cloneRoot, templateTexts[i]);
        }

        FillMissingButtonReferences(panelInstance);
    }

    private T FindClonedComponent<T>(Transform cloneRoot, T templateComponent) where T : Component
    {
        if (cloneRoot == null)
            return null;

        if (templateComponent == null)
            return null;

        if (interactionPanel == null)
            return null;

        string relativePath = GetRelativePath(interactionPanel.transform, templateComponent.transform);

        if (string.IsNullOrEmpty(relativePath))
            return null;

        Transform clonedTransform = cloneRoot.Find(relativePath);

        if (clonedTransform == null)
            return null;

        return clonedTransform.GetComponent<T>();
    }

    private string GetRelativePath(Transform root, Transform target)
    {
        if (root == null || target == null)
            return string.Empty;

        if (root == target)
            return string.Empty;

        List<string> pathParts = new List<string>();
        Transform current = target;

        while (current != null && current != root)
        {
            pathParts.Add(current.name);
            current = current.parent;
        }

        if (current != root)
            return string.Empty;

        pathParts.Reverse();

        return string.Join("/", pathParts);
    }

    private void FillMissingButtonReferences(PanelInstance panelInstance)
    {
        if (panelInstance == null || panelInstance.panelObject == null)
            return;

        Button[] foundButtons = panelInstance.panelObject.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < 4; i++)
        {
            if (panelInstance.buttons[i] == null && i < foundButtons.Length)
                panelInstance.buttons[i] = foundButtons[i];

            if (panelInstance.buttonTexts[i] == null && panelInstance.buttons[i] != null)
                panelInstance.buttonTexts[i] = panelInstance.buttons[i].GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void FollowInteractable(PanelInstance panelInstance)
    {
        if (panelInstance == null)
            return;

        if (panelInstance.interactable == null)
            return;

        if (panelInstance.panelObject == null)
            return;

        Vector3 targetPosition = panelInstance.interactable.GetMenuWorldPosition();
        Quaternion anchorRotation = panelInstance.interactable.GetMenuWorldRotation();

        float faceAmount = panelInstance.interactable.FaceCameraAmount;

        Quaternion finalRotation = anchorRotation;

        if (faceAmount > 0f && playerCamera != null)
        {
            Quaternion cameraRotation = GetCameraFacingRotation(targetPosition);

            finalRotation = Quaternion.Slerp(
                anchorRotation,
                cameraRotation,
                faceAmount
            );
        }

        panelInstance.panelObject.transform.SetPositionAndRotation(targetPosition, finalRotation);
    }

    private Quaternion GetCameraFacingRotation(Vector3 menuPosition)
    {
        if (playerCamera == null)
            return transform.rotation;

        Vector3 direction = menuPosition - playerCamera.transform.position;

        if (direction.sqrMagnitude <= 0.0001f)
            return transform.rotation;

        return Quaternion.LookRotation(direction, Vector3.up);
    }

    private void SetupButton(Button button, TMP_Text buttonText, MCInteractable.MCAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();

        if (action == null || !action.HasAction())
        {
            button.gameObject.SetActive(false);
            return;
        }

        button.gameObject.SetActive(true);

        if (buttonText != null)
            buttonText.text = action.ButtonText;

        button.onClick.AddListener(action.Invoke);
    }

    private void ClearPanel(PanelInstance panelInstance)
    {
        if (panelInstance == null)
            return;

        for (int i = 0; i < panelInstance.buttons.Length; i++)
        {
            ClearButton(panelInstance.buttons[i]);
        }
    }

    private void ClearButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.gameObject.SetActive(false);
    }

    private void RebuildLayout(GameObject panelObject)
    {
        if (panelObject == null)
            return;

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
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

    private void ApplyCanvasCamera()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
            return;

        if (playerCamera == null)
            return;

        canvas.worldCamera = playerCamera;
    }
}