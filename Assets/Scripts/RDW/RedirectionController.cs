using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PhysicalBoundaryManager))]
public class RedirectionController : MonoBehaviour
{
    public static RedirectionController Instance { get; private set; }

    [Header("References")]
    [SerializeField] new Transform camera;
    [SerializeField] Transform virtualWorld;
    [SerializeField] Transform cart;

    [Header("Translation Gain")]
    [SerializeField] float headingToCenterDotThreshold = 0.45f;
    [SerializeField] float maxExtraTranslationGain = 0.26f;
    [SerializeField] float minExtraTranslationGain = 0.14f;

    [Header("Rotation Gain")]
    [SerializeField] float extraStillRotationGain = 0.005f;
    [SerializeField] float maxExtraRotationGain = 0.49f;
    [SerializeField] float minExtraRotationGain = 0.2f;

    [Header("Curvature Gain")]
    [SerializeField] float minCurvatureGainRadius = 22f;

    [Header("Reset")]
    [SerializeField] GameObject resetWarningUI;
    [SerializeField] TMP_Text resetWarningAngleLeftUI;
    private bool isResetting = false;
    private float resetLocalYaw = 0f;
    private float startVirtualLocalYaw = 0f;
    private float startVirtualWorldYaw = 0f;

    [Header("Steer-to-Action")]
    [SerializeField] float minS2ARotationThreshold = 10f;
    [SerializeField] float maxS2ARotationThreshold = 10f;
    [SerializeField] float timeToCompleteSteer = 2f;

    PhysicalBoundaryManager physicalBoundaryManager;
    Vector3 RealWorldOrigin => physicalBoundaryManager.BoundaryCenter.position;
    float RealWorldRadius => physicalBoundaryManager.BoundaryRadius;
    float SafeRealWorldRadius => RealWorldRadius * 0.2f;
    float DistractorRealWorldRadius => RealWorldRadius * 0.8f;
    float DangerRealWorldRadius => RealWorldRadius * 1f;

    Vector3 camPos;
    Vector3 center;
    Vector3 toCenter;
    float distToCenter;
    float centerYaw;
    float yawToRotateToFaceCenter;
    Vector3 translationDelta;
    Vector3 translation;
    float headingToCenterDot;

    Vector3 prevPos;
    float prevYaw;
    float prevYawToRotateToFaceCenter;

    Vector3 target;
    Vector3 toTarget;
    float yawToRotateToFaceTarget;
    float prevYawToRotateToFaceTarget;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        physicalBoundaryManager = GetComponent<PhysicalBoundaryManager>();

        prevPos = camera.position;
        prevYaw = camera.eulerAngles.y;
    }

    void Update()
    {
        if (physicalBoundaryManager.IsPlacing) return;

        camPos = new(camera.position.x, 0, camera.position.z);
        center = new(RealWorldOrigin.x, 0, RealWorldOrigin.z);

        toCenter = (center - camPos).normalized;
        distToCenter = Vector3.Distance(camPos, center);
        centerYaw = SignedAngleOnXZ(Vector3.forward, toCenter);
        yawToRotateToFaceCenter = Mathf.DeltaAngle(camera.eulerAngles.y, centerYaw);

        translationDelta = camPos - new Vector3(prevPos.x, 0, prevPos.z);
        translation = translationDelta.normalized;
        headingToCenterDot = Vector3.Dot(translation, toCenter);

        if (target != default)
        {
            toTarget = (target - camPos).normalized;
            yawToRotateToFaceTarget = Vector3.SignedAngle(toCenter, toTarget, Vector3.up);
        }

        ResetRotation();
        if (isResetting)
        {
            UpdatePrevTransform();
            return;
        }
        else
        {
            Distract();
        }

        ApplyTranslationGain(ComputeTranslationGain());
        ApplyRotationGain(ComputeRotationGain());
        ApplyStillAndCurvatureGain(ComputeStillAndCurvatureGain());

        UpdatePrevTransform();
    }

    private void UpdatePrevTransform()
    {
        prevPos = camera.position;
        prevYaw = camera.eulerAngles.y;
        prevYawToRotateToFaceCenter = yawToRotateToFaceCenter;
        prevYawToRotateToFaceTarget = yawToRotateToFaceTarget;
    }

    float ComputeTranslationGain()
    {
        if (translation.magnitude < GlobalThresholds.EPS)
            return 0;

        float extraGain;
        if (distToCenter <= SafeRealWorldRadius && headingToCenterDot > headingToCenterDotThreshold)
        {
            extraGain = minExtraTranslationGain;
        }
        else if (distToCenter >= SafeRealWorldRadius && headingToCenterDot > headingToCenterDotThreshold)
        {
            extraGain = 0;
        }
        else
        {
            extraGain = -maxExtraTranslationGain;
        }

        extraGain = Mathf.Max(Mathf.Abs(headingToCenterDot), headingToCenterDotThreshold) * extraGain;
        extraGain *= Mathf.InverseLerp(0, RealWorldRadius, distToCenter);

        return extraGain;
    }

    void ApplyTranslationGain(float gain)
    {
        Vector3 modified = translationDelta * gain;

        virtualWorld.position -= modified - translationDelta;
    }

    bool prevMinRotationGainApplied = false;
    float ComputeRotationGain()
    {
        float rotation = Mathf.Abs(yawToRotateToFaceCenter) - Mathf.Abs(prevYawToRotateToFaceCenter);

        if (Mathf.Abs(rotation) < GlobalThresholds.EPS)
            return 1;

        float extraGain = rotation > 0 ? minExtraRotationGain : -maxExtraRotationGain;
        if (target != default)
        {
            if (Mathf.Abs(yawToRotateToFaceTarget) - Mathf.Abs(prevYawToRotateToFaceTarget) > GlobalThresholds.EPS)
                prevMinRotationGainApplied = !prevMinRotationGainApplied;

            if (prevMinRotationGainApplied)
                extraGain = rotation > 0 ? -maxExtraRotationGain : minExtraRotationGain;
            else
                extraGain = rotation > 0 ? minExtraRotationGain : -maxExtraRotationGain;
        }

        extraGain *= Mathf.InverseLerp(0, RealWorldRadius, distToCenter);

        return 1 + extraGain;
    }

    void ApplyRotationGain(float gain)
    {
        float yaw = camera.eulerAngles.y;
        float deltaYaw = Mathf.DeltaAngle(prevYaw, yaw);
        float modified = deltaYaw * gain;

        virtualWorld.RotateAround(camera.position, Vector3.up, modified - deltaYaw);
    }

    float ComputeStillAndCurvatureGain()
    {
        if (Mathf.Abs(yawToRotateToFaceCenter) < GlobalThresholds.ANG_EPS)
            return 0;

        var direction = 0 < yawToRotateToFaceCenter ||
            yawToRotateToFaceCenter >= 160 || yawToRotateToFaceCenter <= -160 ? 1 : -1;

        float curvatureGain = 1 / minCurvatureGainRadius;
        float speed = translationDelta.magnitude;
        curvatureGain = direction * -curvatureGain * speed * Mathf.Rad2Deg;
        float stillGain = direction * -extraStillRotationGain * Mathf.Rad2Deg * Time.deltaTime;

        return curvatureGain + stillGain;
    }

    void ApplyStillAndCurvatureGain(float gain)
    {
        virtualWorld.RotateAround(camera.position, Vector3.up, gain);
    }

    float SignedAngleOnXZ(Vector3 a, Vector3 b)
    {
        a.y = 0; b.y = 0;
        return Vector3.SignedAngle(a, b, Vector3.up);
    }

    void ResetRotation()
    {
        if (isResetting)
        {
            ApplyResetYaw();
            return;
        }

        if (
            distToCenter >= DangerRealWorldRadius &&
            (
                translationDelta.magnitude > 5 * GlobalThresholds.EPS ||
                translationDelta.magnitude > GlobalThresholds.EPS &&
                Vector3.Dot(camera.forward, translationDelta.normalized) > GlobalThresholds.EPS
            ) &&
            GlobalThresholds.EPS < Math.Abs(headingToCenterDot) &&
            headingToCenterDot <= headingToCenterDotThreshold)
        {
            StartReset();
        }
    }

    private void StartReset()
    {
        isResetting = true;

        resetWarningUI.SetActive(true);

        startVirtualLocalYaw = camera.localEulerAngles.y;
        startVirtualWorldYaw = virtualWorld.eulerAngles.y;
        resetLocalYaw = camera.localEulerAngles.y + 180f;
    }

    private void ApplyResetYaw()
    {
        var yawToRotate = Mathf.DeltaAngle(camera.localEulerAngles.y, resetLocalYaw);

        if (Mathf.Abs(yawToRotate) < GlobalThresholds.ANG_EPS)
        {
            isResetting = false;

            resetWarningUI.SetActive(false);

            return;
        }

        float virtualDeltaYaw = Mathf.DeltaAngle(camera.localEulerAngles.y, startVirtualLocalYaw);

        resetWarningAngleLeftUI.text = $"Please turn {(int)yawToRotate}°.";

        virtualWorld.RotateAround(
            camera.position, Vector3.up, -virtualWorld.eulerAngles.y + (virtualDeltaYaw + startVirtualWorldYaw)
        );
    }

    private void Distract()
    {
        if (distToCenter < DistractorRealWorldRadius) return;

        Customer newCustomer = CustomerSpawnerManager.Instance.SpawnCustomer(center);

        if (newCustomer == null) return;

        Transform distractor = newCustomer.transform.Find("Distractor");
        if (distractor != null)
        {
            distractor.gameObject.SetActive(true);
        }
        HapticManager.Instance.TriggerFailureHaptic(HapticManager.Hand.Both);
    }

    public void StartSteerToAction()
    {
        if (distToCenter < GlobalThresholds.EPS) return;

        (Transform nearestCustomer, Transform secondNearestCustomer) = GetTwoNearestCustomers();

        Transform parentOfNearestCustomer = nearestCustomer.parent;
        Transform parentOfCart = cart.parent;
        nearestCustomer.SetParent(virtualWorld, true);
        cart.SetParent(virtualWorld, true);

        target = new(secondNearestCustomer.position.x, 0, secondNearestCustomer.position.z);
        var toTarget = (target - camPos).normalized;

        yawToRotateToFaceTarget = Vector3.SignedAngle(toCenter, toTarget, Vector3.up);
        var yawToRotate = Mathf.Clamp(
            yawToRotateToFaceTarget, -minS2ARotationThreshold, maxS2ARotationThreshold
        );

        StartCoroutine(RotateWorldOverTime(
            yawToRotate, timeToCompleteSteer, nearestCustomer, parentOfNearestCustomer, parentOfCart
        ));
    }

    private IEnumerator RotateWorldOverTime(
        float totalAngle, float duration, Transform customer, Transform originalCustomerParent, Transform originalCartParent
    )
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float delta = totalAngle * Time.deltaTime / duration;
            virtualWorld.RotateAround(camera.position, Vector3.up, delta);
            elapsed += Time.deltaTime;
            yield return null;
        }

        customer.transform.SetParent(originalCustomerParent, true);
        cart.SetParent(originalCartParent, true);
        target = default;
    }

    private (Transform nearest, Transform secondNearest) GetTwoNearestCustomers()
    {
        Transform nearest = null;
        Transform secondNearest = null;

        float minDist1 = float.MaxValue;
        float minDist2 = float.MaxValue;

        foreach (var customer in CustomerSpawnerManager.Instance.ExistingCustomers)
        {
            float d = Vector3.Distance(customer.transform.position, camera.position);

            if (d < minDist1)
            {
                minDist2 = minDist1;
                secondNearest = nearest;

                minDist1 = d;
                nearest = customer.transform;
            }
            else if (d < minDist2)
            {
                minDist2 = d;
                secondNearest = customer.transform;
            }
        }

        return (nearest, secondNearest);
    }
}