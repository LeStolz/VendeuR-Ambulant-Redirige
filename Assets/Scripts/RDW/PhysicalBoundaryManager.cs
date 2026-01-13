using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Features.Meta;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PhysicalBoundaryVisibilityManager))]
[RequireComponent(typeof(RedirectionController))]
public class PhysicalBoundaryManager : MonoBehaviour
{
    [SerializeField] InputActionReference adjustBoundaryInteraction;
    [SerializeField] List<Transform> boundaryPoints = new();
    [SerializeField] GameObject boundaryCalibrationUI;

    RedirectionController redirectionController;
    PhysicalBoundaryVisibilityManager physicalBoundaryVisibilityManager;
    LineRenderer lineRenderer;

    public bool IsPlacing { get; private set; } = false;
    public Transform BoundaryCenter { get; private set; }

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        physicalBoundaryVisibilityManager = GetComponent<PhysicalBoundaryVisibilityManager>();
        redirectionController = GetComponent<RedirectionController>();

        lineRenderer.positionCount = boundaryPoints.Count;

        PlayerPrefs.DeleteAll(); // TEMPORARY: Clear saved boundary points for testing
        for (int i = 0; i < boundaryPoints.Count; i++)
        {
            float x = PlayerPrefs.GetFloat($"BoundaryPoint_{i}_X", boundaryPoints[i].localPosition.x);
            float z = PlayerPrefs.GetFloat($"BoundaryPoint_{i}_Z", boundaryPoints[i].localPosition.z);

            boundaryPoints[i].localPosition = new Vector3(x, 1, z);
        }

        AdjustBoundary(new InputAction.CallbackContext());
        adjustBoundaryInteraction.action.Enable();
    }

    void Update()
    {
        if (adjustBoundaryInteraction.action.WasPressedThisFrame())
        {
            AdjustBoundary(new InputAction.CallbackContext());
        }

        if (!IsPlacing) return;

        lineRenderer.SetPositions(boundaryPoints.Select(
            point =>
            {
                Vector3 localPosition = transform.InverseTransformPoint(point.position);
                return new Vector3(localPosition.x, 0, localPosition.z);
            }).ToArray());
    }

    void AdjustBoundary(InputAction.CallbackContext ctx)
    {
        IsPlacing = !IsPlacing;

        boundaryPoints.ForEach(point =>
        {
            point.gameObject.SetActive(IsPlacing);
            point.SetParent(transform);
        });

        // lineRenderer.enabled = IsPlacing;

        boundaryCalibrationUI.SetActive(IsPlacing);
        physicalBoundaryVisibilityManager.SetBoundaryVisibility(
            IsPlacing ? XrBoundaryVisibility.VisibilityNotSuppressed : XrBoundaryVisibility.VisibilitySuppressed
        );

        if (IsPlacing) return;

        var boundaryPositions = boundaryPoints.Select(point => point.position).ToArray();

        if (BoundaryCenter == null)
        {
            BoundaryCenter = new GameObject("Physical Boundary Center").transform;
            BoundaryCenter.parent = transform;
        }
        BoundaryCenter.localPosition = boundaryPositions.Aggregate(
            Vector3.zero, (acc, point) => acc + point
        ) / boundaryPoints.Count;


        for (int i = 0; i < boundaryPoints.Count; i++)
        {
            PlayerPrefs.SetFloat($"BoundaryPoint_{i}_X", boundaryPoints[i].localPosition.x);
            PlayerPrefs.SetFloat($"BoundaryPoint_{i}_Z", boundaryPoints[i].localPosition.z);
        }

        redirectionController.UpdateCurvatureGain(GetDistanceToBoundary(BoundaryCenter.position));
    }

    public float GetDistanceToBoundary(Vector3 position)
    {
        var boundaryPositions = boundaryPoints.Select(point => point.position).ToArray();
        var minDistance = float.MaxValue;

        for (int i = 0; i < boundaryPositions.Length; i++)
        {
            Vector3 pointA = boundaryPositions[i];
            Vector3 pointB = boundaryPositions[(i + 1) % boundaryPositions.Length];
            pointA.y = position.y;
            pointB.y = position.y;

            // Calculate distance from center to edge AB
            Vector3 lineDir = (pointB - pointA).normalized;
            Vector3 AtoCenter = position - pointA;
            Vector3 AtoCenterProjected = Vector3.Project(AtoCenter, lineDir);
            Vector3 closestPoint = pointA + AtoCenterProjected;
            float dist = Vector3.Distance(position, closestPoint);
            float sign = Vector3.Dot(Vector3.Cross(lineDir, AtoCenter), Vector3.up) < 0 ? 1f : -1f;
            dist *= sign;

            minDistance = Mathf.Min(minDistance, dist);
        }

        return minDistance;
    }
}