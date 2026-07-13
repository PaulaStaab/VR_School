// GEZEanim.cs
// Controls the GEZE door closer arm linkage for animated doors.
// Project: VR school building
// Author: Matthias Hofherr
// Created: 2026

using UnityEngine;

public class GEZEanim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform armA;
    [SerializeField] private Transform armB;

    [Header("Arm Lengths")]
    [SerializeField] private float armALength = 0.5f;
    [SerializeField] private float armBLength = 0.5f;

    [Header("Joint Side")]
    [SerializeField] private bool invertJointSide = false;

    [Header("Debug")]
    [SerializeField] private bool drawDebugLines = true;

    private Vector3 armAStartEuler;
    private Vector3 armBStartEuler;

    private Vector3 armAStartDirection;
    private Vector3 armBStartDirection;

    private bool initialized = false;

    private void Start()
    {
        Initialize();
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            Initialize();
        }

        UpdateArms();
    }

    private void Initialize()
    {
        if (armA == null || armB == null)
        {
            return;
        }

        Vector3 connectionPoint;

        // Calculate the initial mechanical joint position from both arm pivots and arm lengths.
        bool solved = TrySolveConnectionPoint(
            armA.position,
            armB.position,
            Mathf.Max(armALength, 0.0001f),
            Mathf.Max(armBLength, 0.0001f),
            invertJointSide,
            out connectionPoint
        );

        if (!solved)
        {
            return;
        }

        // Store the initial rotations and directions.
        // These values define the zero/reference pose of the linkage.
        armAStartEuler = armA.localEulerAngles;
        armBStartEuler = armB.localEulerAngles;

        armAStartDirection = GetFlatLocalDirectionToPoint(armA, connectionPoint);
        armBStartDirection = GetFlatLocalDirectionToPoint(armB, connectionPoint);

        initialized = true;
    }

    private void UpdateArms()
    {
        if (armA == null || armB == null)
        {
            return;
        }

        Vector3 connectionPoint;

        // Recalculate the current joint position every frame.
        bool solved = TrySolveConnectionPoint(
            armA.position,
            armB.position,
            Mathf.Max(armALength, 0.0001f),
            Mathf.Max(armBLength, 0.0001f),
            invertJointSide,
            out connectionPoint
        );

        if (!solved)
        {
            return;
        }

        RotateArmToConnectionPoint(
            armA,
            armAStartEuler,
            armAStartDirection,
            connectionPoint
        );

        RotateArmToConnectionPoint(
            armB,
            armBStartEuler,
            armBStartDirection,
            connectionPoint
        );

        if (drawDebugLines)
        {
            // Red: Arm A pivot to calculated joint.
            // Green: Arm B pivot to calculated joint.
            Debug.DrawLine(armA.position, connectionPoint, Color.red);
            Debug.DrawLine(armB.position, connectionPoint, Color.green);
        }
    }

    private void RotateArmToConnectionPoint(
        Transform arm,
        Vector3 startEuler,
        Vector3 startDirection,
        Vector3 connectionPoint
    )
    {
        Vector3 currentDirection = GetFlatLocalDirectionToPoint(arm, connectionPoint);

        if (startDirection.sqrMagnitude <= 0.0001f || currentDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        // Calculate the planar angle difference between the initial direction and the current target direction.
        float angle = Vector3.SignedAngle(
            startDirection.normalized,
            currentDirection.normalized,
            Vector3.up
        );

        // Only the local Y rotation is changed. X and Z remain based on the original pose.
        Vector3 newEuler = startEuler;
        newEuler.y = startEuler.y + angle;

        arm.localEulerAngles = newEuler;
    }

    private Vector3 GetFlatLocalDirectionToPoint(Transform arm, Vector3 worldPoint)
    {
        Vector3 worldDirection = worldPoint - arm.position;

        Transform parent = arm.parent;

        // Convert the world direction into the arm parent's local space.
        Vector3 localDirection = parent != null
            ? parent.InverseTransformDirection(worldDirection)
            : worldDirection;

        // The linkage is solved in the horizontal plane only.
        localDirection.y = 0f;

        if (localDirection.sqrMagnitude <= 0.0001f)
        {
            return Vector3.zero;
        }

        return localDirection.normalized;
    }

    private bool TrySolveConnectionPoint(
        Vector3 armAPivot,
        Vector3 armBPivot,
        float lengthA,
        float lengthB,
        bool invertSide,
        out Vector3 connectionPoint
    )
    {
        connectionPoint = Vector3.zero;

        // Work in the XZ plane. Y is restored later from the arm pivot heights.
        Vector3 pointA = new Vector3(armAPivot.x, 0f, armAPivot.z);
        Vector3 pointB = new Vector3(armBPivot.x, 0f, armBPivot.z);

        Vector3 between = pointB - pointA;
        float distance = between.magnitude;

        if (distance <= 0.0001f)
        {
            return false;
        }

        Vector3 dir = between.normalized;

        // Clamp the distance so the circle intersection remains mathematically valid.
        float minDistance = Mathf.Abs(lengthA - lengthB) + 0.0001f;
        float maxDistance = lengthA + lengthB - 0.0001f;

        float clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Circle-circle intersection:
        // pointA/lengthA and pointB/lengthB define the mechanical joint position.
        float x = (clampedDistance * clampedDistance + lengthA * lengthA - lengthB * lengthB)
                  / (2f * clampedDistance);

        float hSquared = lengthA * lengthA - x * x;
        float h = Mathf.Sqrt(Mathf.Max(0f, hSquared));

        Vector3 midPoint = pointA + dir * x;

        Vector3 perp = new Vector3(-dir.z, 0f, dir.x);

        Vector3 candidate1 = midPoint + perp * h;
        Vector3 candidate2 = midPoint - perp * h;

        // Select one of the two possible joint positions.
        Vector3 chosen = invertSide ? candidate2 : candidate1;

        float y = (armAPivot.y + armBPivot.y) * 0.5f;

        connectionPoint = new Vector3(chosen.x, y, chosen.z);

        return true;
    }
}