using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PhysicalBoundaryManager))]
public class RedirectionController : MonoBehaviour
{
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
    private float resetYaw = 0f;
    private float startVirtualYaw = 0f;
    private float startVirtualWorldYaw = 0f;

    PhysicalBoundaryManager physicalBoundaryManager;
    Vector3 RealWorldOrigin => physicalBoundaryManager.BoundaryCenter;
    float RealWorldRadius => physicalBoundaryManager.BoundaryRadius;
    float SafeRealWorldRadius => RealWorldRadius * 0.2f;
    float DangerRealWorldRadius => RealWorldRadius * 0.9f;

    Vector3 prevPos;
    float prevYaw;
    float prevYawToRotateToFaceCenter;

    void Start()
    {
        physicalBoundaryManager = GetComponent<PhysicalBoundaryManager>();

        prevPos = camera.position;
        prevYaw = camera.eulerAngles.y;
    }

    void Update()
    {
        Vector3 camPos = new(camera.position.x, 0, camera.position.z);
        Vector3 center = new(RealWorldOrigin.x, 0, RealWorldOrigin.z);
        Vector3 toCenter = (center - camPos).normalized;
        Vector3 translation = (camPos - new Vector3(prevPos.x, 0, prevPos.z)).normalized;
        float headingToCenterDot = Vector3.Dot(translation, toCenter);

        float centerYaw = SignedAngleOnXZ(Vector3.forward, toCenter);
        float distToCenter = Vector3.Distance(camPos, center);
        float yawToRotateToFaceCenter = Mathf.DeltaAngle(camera.eulerAngles.y, centerYaw);

        ResetRotation(distToCenter, headingToCenterDot);
        if (isResetting)
        {
            UpdatePrevTransform(yawToRotateToFaceCenter);
            return;
        }

        float translationGain = ComputeTranslationGain(translation, headingToCenterDot, distToCenter);
        float rotationGain = ComputeRotationGain(distToCenter, prevYawToRotateToFaceCenter, yawToRotateToFaceCenter);
        float curvatureGain = ComputeStillAndCurvatureGain(yawToRotateToFaceCenter);
        ApplyTranslationGain(translationGain);
        ApplyRotationGain(rotationGain);
        ApplyStillAndCurvatureGain(curvatureGain);

        UpdatePrevTransform(yawToRotateToFaceCenter);
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
            return 1;

        float extraGain;
        if (distToCenter <= SafeRealWorldRadius && headingToCenterDot > headingToCenterDotThreshold)
        {
            extraGain = -minExtraTranslationGain;
        }
        else if (distToCenter >= SafeRealWorldRadius && headingToCenterDot > headingToCenterDotThreshold)
        {
            extraGain = 0;
        }
        else
        {
            extraGain = maxExtraTranslationGain;
        }

        extraGain = Mathf.Max(Mathf.Abs(headingToCenterDot), headingToCenterDotThreshold) * extraGain;
        extraGain *= Mathf.InverseLerp(0, RealWorldRadius, distToCenter);

        return 1 + extraGain;
    }

    void ApplyTranslationGain(float gain)
    {
        Vector3 delta = camera.position - prevPos;
        delta.y = 0;
        Vector3 modified = delta * gain;

        virtualWorld.position -= modified - delta;
    }

    float ComputeRotationGain(float distToCenter, float prevYawToRotateToGetToCenter, float yawToRotateToFaceCenter)
    {
        float rotation = Mathf.Abs(yawToRotateToFaceCenter) - Mathf.Abs(prevYawToRotateToGetToCenter);

        if (Mathf.Abs(rotation) < GlobalThresholds.EPS)
            return 1;

        float extraGain = (rotation > 0) ? -minExtraRotationGain : maxExtraRotationGain;

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

    float ComputeStillAndCurvatureGain(float yawToRotate)
    {
        float curvatureGain = 1 / minCurvatureGainRadius;
        float speed = (camera.position - prevPos).magnitude;
        curvatureGain = Mathf.Sign(yawToRotate) * curvatureGain * speed * Mathf.Rad2Deg;
        float stillGain = Mathf.Sign(yawToRotate) * extraStillRotationGain * Mathf.Rad2Deg * Time.deltaTime;

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

    void ResetRotation(float distToCenter, float headingToCenterDot)
    {
        if (isResetting)
        {
            ApplyResetYaw();
            return;
        }

        Debug.Log(distToCenter + " " + DangerRealWorldRadius);

        if (
            distToCenter >= DangerRealWorldRadius &&
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

        startVirtualYaw = camera.eulerAngles.y;
        startVirtualWorldYaw = virtualWorld.eulerAngles.y;
        resetYaw = camera.eulerAngles.y + 180;
    }

    private void ApplyResetYaw()
    {
        var yawToRotate = Mathf.Abs(Mathf.DeltaAngle(camera.eulerAngles.y, resetYaw));

        if (yawToRotate < GlobalThresholds.ANG_EPS)
        {
            isResetting = false;

            resetWarningUI.SetActive(false);

            return;
        }

        // float veOriy = (camera.eulerAngles.y - prevLocalRot) * 2f;
        // virtualWorld.RotateAround(camera.position, Vector3.up, veOriy);

        float virtualDeltaYaw = camera.eulerAngles.y - startVirtualYaw;

        resetWarningAngleLeftUI.text = $"Please turn {(int)yawToRotate}°.";

        virtualWorld.RotateAround(
            camera.position, Vector3.up, -virtualWorld.eulerAngles.y + (virtualDeltaYaw + startVirtualWorldYaw)
        );
    }
}
