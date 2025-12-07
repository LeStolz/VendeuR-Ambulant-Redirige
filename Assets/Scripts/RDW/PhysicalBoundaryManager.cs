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
    [SerializeField] InputActionReference leftInteractionSelect;
    [SerializeField] InputActionReference rightInteractionSelect;
    [SerializeField] InputActionReference restartInteraction;
    [SerializeField] InputActionReference finishInteraction;

    PhysicalBoundaryVisibilityManager physicalBoundaryVisibilityManager;

    List<NearFarInteractor> interactors = new();
    NearFarInteractor cachedActiveInteractor = null;
    float timeSinceLastInteraction = 0f;

    LineRenderer lineRenderer;
    bool isPlacing = false;

    public List<Vector3> BoundaryPoints { get; private set; } = new();
    public Vector3 BoundaryCenter { get; private set; }
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

        leftInteractionSelect.action.performed += PlacePoint;
        rightInteractionSelect.action.performed += PlacePoint;
        restartInteraction.action.performed += RestartPlacement;
        finishInteraction.action.performed += FinishPlacement;
    }

    void OnDestroy()
    {
        leftInteractionSelect.action.performed -= PlacePoint;
        rightInteractionSelect.action.performed -= PlacePoint;
        restartInteraction.action.performed -= RestartPlacement;
        finishInteraction.action.performed -= FinishPlacement;
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

        BoundaryPoints[BoundaryPoints.Count - 1] = hit.point;

        PlaceBoundary();

        timeSinceLastInteraction += Time.deltaTime;
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

        BoundaryCenter = BoundaryPoints.Aggregate(Vector3.zero, (acc, point) => acc + point) / BoundaryPoints.Count;

        BoundaryRadius = 0f;
        for (int i = 0; i < BoundaryPoints.Count; i++)
        {
            Vector3 pointA = BoundaryPoints[i];
            Vector3 pointB = BoundaryPoints[(i + 1) % BoundaryPoints.Count];

            // Calculate distance from center to edge AB
            Vector3 lineDir = (pointB - pointA).normalized;
            Vector3 AtoCenter = BoundaryCenter - pointA;
            Vector3 AtoCenterProjected = Vector3.Project(AtoCenter, lineDir);
            Vector3 closestPoint = pointA + AtoCenterProjected;
            float dist = Vector3.Distance(BoundaryCenter, closestPoint);

            BoundaryRadius = Mathf.Max(BoundaryRadius, dist);
        }

        lineRenderer.loop = true;
    }

    void PlacePoint(InputAction.CallbackContext ctx)
    {
        if (
            !isPlacing ||
            ctx.ReadValue<float>() < GlobalThresholds.INTERACTION_ACTIVE_THRESHOLD ||
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