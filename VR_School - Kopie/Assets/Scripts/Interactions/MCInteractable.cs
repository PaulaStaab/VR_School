/*using UnityEngine;
using UnityEngine.Events;

public class MCInteractable : MonoBehaviour
{
    [System.Serializable]
    public class MCAction
    {
        [SerializeField] private string buttonText = "Aktion";
        [SerializeField] private UnityEvent onClick;

        public string ButtonText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(buttonText))
                    return "Aktion";

                return buttonText;
            }
        }

        public bool HasAction()
        {
            return onClick != null && onClick.GetPersistentEventCount() > 0;
        }

        public void Invoke()
        {
            if (onClick == null)
                return;

            onClick.Invoke();
        }
    }

    [Header("MC Point")]
    [SerializeField] private Transform uiAnchor;

    [Header("Menu Placement")]
    [SerializeField] private Vector3 menuLocalOffset = Vector3.zero;
    [SerializeField] private float pullTowardCamera = 0.05f;

    [Header("Position Follow")]
    [SerializeField] private bool followTargetPosition = true;

    [Range(0f, 1f)]
    [SerializeField] private float positionFollowAmount = 1.0f;

    [Header("Camera Facing")]
    [SerializeField] private bool facePlayerCamera = true;

    [Range(0f, 1f)]
    [SerializeField] private float faceCameraAmount = 0.5f;

    [SerializeField] private bool flipFacingDirection;

    [Header("Actions")]
    [SerializeField] private MCAction action1 = new MCAction();
    [SerializeField] private MCAction action2 = new MCAction();
    [SerializeField] private MCAction action3 = new MCAction();
    [SerializeField] private MCAction action4 = new MCAction();

    public bool FollowTargetPosition => followTargetPosition;
    public float PositionFollowAmount => positionFollowAmount;
    public bool FacePlayerCamera => facePlayerCamera;
    public float FaceCameraAmount => faceCameraAmount;
    public bool FlipFacingDirection => flipFacingDirection;

    public Vector3 InteractionWorldPosition
    {
        get
        {
            if (uiAnchor != null)
                return uiAnchor.position;

            return transform.position;
        }
    }

    public Vector3 GetMenuWorldPosition(Camera playerCamera)
    {
        Transform anchorTransform = uiAnchor != null ? uiAnchor : transform;

        Vector3 menuPosition = anchorTransform.TransformPoint(menuLocalOffset);

        if (playerCamera != null && pullTowardCamera > 0f)
        {
            Vector3 directionToCamera = playerCamera.transform.position - menuPosition;

            if (directionToCamera.sqrMagnitude > 0.0001f)
                menuPosition += directionToCamera.normalized * pullTowardCamera;
        }

        return menuPosition;
    }

    public MCAction GetAction(int index)
    {
        switch (index)
        {
            case 0:
                return action1;

            case 1:
                return action2;

            case 2:
                return action3;

            case 3:
                return action4;
        }

        return null;
    }

    public bool HasAnyAction()
    {
        for (int i = 0; i < 4; i++)
        {
            MCAction action = GetAction(i);

            if (action != null && action.HasAction())
                return true;
        }

        return false;
    }
}*/
using UnityEngine;
using UnityEngine.Events;

public class MCInteractable : MonoBehaviour
{
    [System.Serializable]
    public class MCAction
    {
        [SerializeField] private string buttonText = "Aktion";
        [SerializeField] private UnityEvent onClick;

        public string ButtonText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(buttonText))
                    return "Aktion";

                return buttonText;
            }
        }

        public bool HasAction()
        {
            return onClick != null && onClick.GetPersistentEventCount() > 0;
        }

        public void Invoke()
        {
            if (onClick == null)
                return;

            onClick.Invoke();
        }
    }

    [Header("MC Point")]
    [SerializeField] private Transform uiAnchor;

    [Header("Menu Placement")]
    [SerializeField] private Vector3 menuLocalOffset = Vector3.zero;

    [Header("Camera Facing")]
    [Range(0f, 1f)]
    [SerializeField] private float faceCameraAmount = 0f;

    [Header("Actions")]
    [SerializeField] private MCAction action1 = new MCAction();
    [SerializeField] private MCAction action2 = new MCAction();
    [SerializeField] private MCAction action3 = new MCAction();
    [SerializeField] private MCAction action4 = new MCAction();

    public float FaceCameraAmount => faceCameraAmount;

    private Transform MenuAnchor
    {
        get
        {
            if (uiAnchor != null)
                return uiAnchor;

            return transform;
        }
    }

    public Vector3 InteractionWorldPosition
    {
        get
        {
            return MenuAnchor.position;
        }
    }

    public Vector3 GetMenuWorldPosition()
    {
        return MenuAnchor.TransformPoint(menuLocalOffset);
    }

    public Quaternion GetMenuWorldRotation()
    {
        return MenuAnchor.rotation;
    }

    public MCAction GetAction(int index)
    {
        switch (index)
        {
            case 0:
                return action1;

            case 1:
                return action2;

            case 2:
                return action3;

            case 3:
                return action4;
        }

        return null;
    }

    public bool HasAnyAction()
    {
        for (int i = 0; i < 4; i++)
        {
            MCAction action = GetAction(i);

            if (action != null && action.HasAction())
                return true;
        }

        return false;
    }
}