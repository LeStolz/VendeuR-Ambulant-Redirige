using System;
using System.Collections.Generic;
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

    [Header("Steer-to Action")]
    [SerializeField] float minRotationThreshold = -6.8f;
    [SerializeField] float maxRotationThreshold = 10f;
    [SerializeField] float timeToCompleteSteer = 2f;

    PhysicalBoundaryManager physicalBoundaryManager;
    Vector3 RealWorldOrigin => physicalBoundaryManager.BoundaryCenter.position;
    float RealWorldRadius => physicalBoundaryManager.BoundaryRadius;
    float SafeRealWorldRadius => RealWorldRadius * 0.2f;
    float DangerRealWorldRadius => RealWorldRadius * 1f;

    List<Customer> customers;

    Vector3 toCenter;
    Vector3 prevPos;
    float prevYaw;
    float prevYawToRotateToFaceCenter;

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
        customers = CustomerSpawnerManager.Instance.existingCustomers;

        prevPos = camera.position;
        prevYaw = camera.eulerAngles.y;
    }

    void Update()
    {
        Vector3 camPos = new(camera.position.x, 0, camera.position.z);
        Vector3 center = new(RealWorldOrigin.x, 0, RealWorldOrigin.z);

        toCenter = (center - camPos).normalized;
        float distToCenter = Vector3.Distance(camPos, center);
        float centerYaw = SignedAngleOnXZ(Vector3.forward, toCenter);
        float yawToRotateToFaceCenter = Mathf.DeltaAngle(camera.eulerAngles.y, centerYaw);

        // Vector3 translationDelta = camPos - new Vector3(prevPos.x, 0, prevPos.z);
        // Vector3 translation = translationDelta.normalized;

        // float headingToCenterDot = Vector3.Dot(translation, toCenter);

        // ResetRotation(distToCenter, headingToCenterDot, translationDelta);
        // if (isResetting)
        // {
        //     UpdatePrevTransform(yawToRotateToFaceCenter);
        //     return;
        // }

        // float translationGain = ComputeTranslationGain(translationDelta, headingToCenterDot, distToCenter);
        // float rotationGain = ComputeRotationGain(distToCenter, prevYawToRotateToFaceCenter, yawToRotateToFaceCenter);
        // float curvatureGain = ComputeStillAndCurvatureGain(yawToRotateToFaceCenter, translationDelta);

        // ApplyTranslationGain(translationGain, translationDelta);
        // ApplyRotationGain(rotationGain);
        // ApplyStillAndCurvatureGain(curvatureGain);

        // UpdatePrevTransform(yawToRotateToFaceCenter);
    }

    private void UpdatePrevTransform(float yawToRotateToFaceCenter)
    {
        prevPos = camera.position;
        prevYaw = camera.eulerAngles.y;
        prevYawToRotateToFaceCenter = yawToRotateToFaceCenter;
    }

    float ComputeTranslationGain(Vector3 translation, float headingToCenterDot, float distToCenter)
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

    void ApplyTranslationGain(float gain, Vector3 delta)
    {
        Vector3 modified = delta * gain;

        virtualWorld.position -= modified - delta;
    }

    float ComputeRotationGain(float distToCenter, float prevYawToRotateToGetToCenter, float yawToRotateToFaceCenter)
    {
        float rotation = Mathf.Abs(yawToRotateToFaceCenter) - Mathf.Abs(prevYawToRotateToGetToCenter);

        if (Mathf.Abs(rotation) < GlobalThresholds.EPS)
            return 1;

        float extraGain = (rotation > 0) ? minExtraRotationGain : -maxExtraRotationGain;

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

    float ComputeStillAndCurvatureGain(float yawToRotate, Vector3 translationDelta)
    {
        if (Mathf.Abs(yawToRotate) < GlobalThresholds.ANG_EPS)
            return 0;

        var direction =
        0 < yawToRotate ||
        yawToRotate >= 160 || yawToRotate <= -160 ? 1 : -1;

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

    void ResetRotation(float distToCenter, float headingToCenterDot, Vector3 translationDelta)
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

    public void StarSteerToAction()
    {
        Vector3 camPos = new(camera.position.x, 0, camera.position.z);
        Vector3 center = new(RealWorldOrigin.x, 0, RealWorldOrigin.z);

        if (Vector3.Distance(camPos, center) < GlobalThresholds.EPS)
        {
            return;
        }

        (Customer nearestCustomer, Customer secondNearestCustomer) = GetTwoNearestCustomers();

        Transform parentOfNearestCustomer = nearestCustomer.transform.parent;
        ChangeParentOfCustomer(nearestCustomer, virtualWorld);

        Vector3 secondNearestCustomerPos = new(secondNearestCustomer.transform.position.x, 0, secondNearestCustomer.transform.position.z);
        Vector3 toCustomer = (secondNearestCustomerPos - camPos).normalized;

        float angle = Vector3.SignedAngle(toCenter, toCustomer, Vector3.up);

        if (angle > 0)
        {
            angle = Mathf.Min(angle, maxRotationThreshold);
        }
        else if (angle < 0)
        {
            angle = Mathf.Max(angle, minRotationThreshold);
        }

        StartCoroutine(RotateWorldOverTime(angle, timeToCompleteSteer, nearestCustomer, parentOfNearestCustomer));
    }

    private void ChangeParentOfCustomer(Customer customer, Transform newParent)
    {
        customer.transform.SetParent(newParent, true);
    }

    private (Customer nearest, Customer secondNearest) GetTwoNearestCustomers()
    {
        Customer nearest = null;
        Customer secondNearest = null;

        float minDist1 = float.MaxValue;
        float minDist2 = float.MaxValue;

        foreach (var customer in customers)
        {
            float d = Vector3.Distance(customer.transform.position, camera.position);

            if (d < minDist1)
            {
                minDist2 = minDist1;
                secondNearest = nearest;

                minDist1 = d;
                nearest = customer;
            }
            else if (d < minDist2)
            {
                minDist2 = d;
                secondNearest = customer;
            }
        }

        return (nearest, secondNearest);
    }

    private IEnumerator RotateWorldOverTime(float totalAngle, float duration, Customer customer, Transform originalParent)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float delta = (totalAngle * Time.deltaTime) / duration;
            virtualWorld.RotateAround(camera.position, Vector3.up, delta);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ChangeParentOfCustomer(customer, originalParent);
    }

    private void Distractor()
    {
        Vector3 center = new(RealWorldOrigin.x, 0, RealWorldOrigin.z);
        Customer newCustomer = CustomerSpawnerManager.Instance.SpawnCustomerAtPosition(center);
        
        Transform distractor = newCustomer.transform.Find("Distractor");
        if (distractor != null)
        {
            distractor.gameObject.SetActive(true);
        }
    }
}
