using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.OpenXR.Features.Meta;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PhysicalBoundaryVisibilityManager))]
public class PhysicalBoundaryManager : MonoBehaviour
{
    [SerializeField] InputActionReference placeInteraction;
    [SerializeField] InputActionReference restartFinishInteraction;

    PhysicalBoundaryVisibilityManager physicalBoundaryVisibilityManager;

    List<NearFarInteractor> interactors = new();
    NearFarInteractor cachedActiveInteractor = null;
    float timeSinceLastInteraction = 0f;

    LineRenderer lineRenderer;
    bool isPlacing = false;

    public List<Vector3> BoundaryPoints { get; private set; } = new();
    public Transform BoundaryCenter { get; private set; }
    public float BoundaryRadius { get; private set; }

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        physicalBoundaryVisibilityManager = GetComponent<PhysicalBoundaryVisibilityManager>();

        if (isPlacing == false)
        {
            Vector3[] positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);
            BoundaryPoints = positions.ToList();
            FinishPlacement(new InputAction.CallbackContext());
        }
        else
        {
            RestartPlacement(new InputAction.CallbackContext());
        }

        interactors = FindObjectsByType<NearFarInteractor>(
            FindObjectsInactive.Include, FindObjectsSortMode.None
        ).ToList();

        placeInteraction.action.performed += PlacePoint;
        restartFinishInteraction.action.performed += RestartFinishPlacement;
    }

    void OnDestroy()
    {
        placeInteraction.action.performed -= PlacePoint;
        restartFinishInteraction.action.performed -= RestartFinishPlacement;
    }

    void Update()
    {
        var interactor = GetActiveInteractor();

        if (interactor == null || !isPlacing) return;

        Ray ray = new(
            interactor.farInteractionCaster.castOrigin.position,
            interactor.farInteractionCaster.castOrigin.forward
        );
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        Vector3 placePos = transform.InverseTransformPoint(new(hit.point.x, 0.1f, hit.point.z));
        BoundaryPoints[^1] = placePos;

        PlaceBoundary();

        timeSinceLastInteraction += Time.deltaTime;
    }

    void RestartFinishPlacement(InputAction.CallbackContext ctx)
    {
        if (isPlacing)
        {
            FinishPlacement(ctx);
        }
        else
        {
            RestartPlacement(ctx);
        }
    }

    void RestartPlacement(InputAction.CallbackContext ctx)
    {
        isPlacing = true;
        physicalBoundaryVisibilityManager.SetBoundaryVisibility(XrBoundaryVisibility.VisibilityNotSuppressed);

        BoundaryPoints.Clear();
        BoundaryPoints.Add(Vector3.zero);

        lineRenderer.loop = false;
        PlaceBoundary();
    }

    void FinishPlacement(InputAction.CallbackContext ctx)
    {
        isPlacing = false;
        physicalBoundaryVisibilityManager.SetBoundaryVisibility(XrBoundaryVisibility.VisibilitySuppressed);

        if (BoundaryCenter == null)
        {
            BoundaryCenter = new GameObject("Physical Boundary Center").transform;
            BoundaryCenter.parent = transform;
        }
        BoundaryCenter.position =
            BoundaryPoints.Aggregate(Vector3.zero, (acc, point) => acc + point) / BoundaryPoints.Count;

        BoundaryRadius = float.MaxValue;
        for (int i = 0; i < BoundaryPoints.Count; i++)
        {
            Vector3 pointA = BoundaryPoints[i];
            Vector3 pointB = BoundaryPoints[(i + 1) % BoundaryPoints.Count];

            // Calculate distance from center to edge AB
            Vector3 lineDir = (pointB - pointA).normalized;
            Vector3 AtoCenter = BoundaryCenter.position - pointA;
            Vector3 AtoCenterProjected = Vector3.Project(AtoCenter, lineDir);
            Vector3 closestPoint = pointA + AtoCenterProjected;
            float dist = Vector3.Distance(BoundaryCenter.position, closestPoint);

            BoundaryRadius = Mathf.Min(BoundaryRadius, dist);
        }

        lineRenderer.loop = true;
    }

    void PlacePoint(InputAction.CallbackContext ctx)
    {
        if (
            !isPlacing ||
            timeSinceLastInteraction < (float)GlobalThresholds.INTERACTION_ACTIVE_DEBOUNCE.TotalSeconds
        ) return;

        timeSinceLastInteraction = 0f;
        BoundaryPoints.Add(Vector3.zero);
    }

    void PlaceBoundary()
    {
        lineRenderer.positionCount = BoundaryPoints.Count;
        lineRenderer.SetPositions(BoundaryPoints.ToArray());
    }

    NearFarInteractor GetActiveInteractor()
    {
        if (cachedActiveInteractor != null && cachedActiveInteractor.gameObject.activeInHierarchy)
        {
            return cachedActiveInteractor;
        }

        var activeInteractors = interactors.FindAll(
            interactor => interactor.gameObject.activeInHierarchy
        );

        if (activeInteractors.Count < 1) return null;
        if (activeInteractors.Count == 1) return activeInteractors[0];

        cachedActiveInteractor = activeInteractors.FirstOrDefault(
            interactor => interactor.handedness == InteractorHandedness.Right
        ) ?? activeInteractors[0];

        return cachedActiveInteractor;
    }
}