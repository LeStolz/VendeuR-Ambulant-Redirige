using System;
using UnityEngine;

[RequireComponent(typeof(PhysicalBoundaryManager))]
public class RedirectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] new Transform camera;
    [SerializeField] Transform virtualWorld;

    [Header("General Gains")]
    [SerializeField] float lookingAtCenterAngleThreshold = 10f;

    [Header("Translation Gain")]
    [SerializeField] float maxExtraTranslationGain = 0.26f;
    [SerializeField] float minExtraTranslationGain = 0.14f;

    [Header("Rotation Gain")]
    [SerializeField] float extraStillRotationGain = 0.05f;
    [SerializeField] float maxExtraRotationGain = 0.49f;
    [SerializeField] float minExtraRotationGain = 0.2f;

    [Header("Curvature Gain")]
    [SerializeField] float minCurvatureGainRadius = 22f;

    PhysicalBoundaryManager physicalBoundaryManager;
    Vector3 RealWorldOrigin => physicalBoundaryManager.BoundaryCenter;
    float RealWorldRadius => physicalBoundaryManager.BoundaryRadius;
    float SafeRealWorldRadius => RealWorldRadius * 0.2f;

    Vector3 prevLocalPos;
    float prevLocalRot;
    float prevYawToRotate;

    void Start()
    {
        physicalBoundaryManager = GetComponent<PhysicalBoundaryManager>();

        prevLocalPos = camera.localPosition;
        prevLocalRot = camera.eulerAngles.y;
    }

    void Update()
    {
        Vector2 camPos = new(camera.position.x, camera.position.z);
        Vector2 center = new(RealWorldOrigin.x, RealWorldOrigin.z);
        Vector3 toCenter = (RealWorldOrigin - camera.position).normalized;
        float centerYaw = SignedAngleOnXZ(Vector3.forward, toCenter);
        float distToCenter = Vector2.Distance(camPos, center);
        float yawToRotate = Mathf.DeltaAngle(camera.eulerAngles.y, centerYaw);

        float translationGain = ComputeTranslationGain(toCenter, distToCenter);
        float rotationGain = ComputeRotationGain(distToCenter, prevYawToRotate, yawToRotate);
        float curvatureGain = ComputeStillAndCurvatureGain(yawToRotate);
        ApplyTranslationGain(translationGain);
        ApplyRotationGain(rotationGain);

        ApplyStillAndCurvatureGain(curvatureGain);

        prevLocalPos = camera.localPosition;
        prevLocalRot = camera.eulerAngles.y;
        prevYawToRotate = yawToRotate;
    }

    float ComputeTranslationGain(Vector3 toCenter, float distToCenter)
    {
        Vector3 translation = camera.localPosition - prevLocalPos;
        float headingToCenter = Vector3.Dot(translation.normalized, toCenter);

        float extraGain;
        if (distToCenter <= SafeRealWorldRadius && headingToCenter > 0)
        {
            extraGain = -minExtraTranslationGain;
        }
        else if (distToCenter >= SafeRealWorldRadius && headingToCenter > 0)
        {
            extraGain = 0;
        }
        else
        {
            extraGain = maxExtraTranslationGain;
        }

        extraGain = Mathf.Abs(headingToCenter) * extraGain;
        extraGain *= Mathf.InverseLerp(0, RealWorldRadius, distToCenter);

        return 1 + extraGain;
    }

    void ApplyTranslationGain(float gain)
    {
        Vector3 delta = camera.localPosition - prevLocalPos;
        Vector3 modified = delta * gain;

        virtualWorld.position -= modified - delta;
    }

    float ComputeRotationGain(float distToCenter, float prevYawToRotate, float yawToRotate)
    {
        if (Mathf.Abs(yawToRotate) < lookingAtCenterAngleThreshold) return 1;

        float rotation = Mathf.Abs(yawToRotate) - Mathf.Abs(prevYawToRotate);

        float extraGain = (rotation > 0) ? -minExtraRotationGain : maxExtraRotationGain;

        extraGain *= Mathf.InverseLerp(0, RealWorldRadius, distToCenter);

        return 1 + extraGain;
    }

    void ApplyRotationGain(float gain)
    {
        float yaw = camera.eulerAngles.y;
        float deltaYaw = Mathf.DeltaAngle(prevLocalRot, yaw);
        float modified = deltaYaw * gain;

        virtualWorld.RotateAround(camera.position, Vector3.up, modified - deltaYaw);
    }

    float ComputeStillAndCurvatureGain(float yawToRotate)
    {
        if (Mathf.Abs(yawToRotate) < lookingAtCenterAngleThreshold) return 0;

        float curvatureGain = 1 / minCurvatureGainRadius;
        float speed = (camera.localPosition - prevLocalPos).magnitude;
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
}
