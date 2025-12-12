using UnityEngine;

[RequireComponent(typeof(PhysicalBoundaryManager))]
public class RedirectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] new Transform camera;
    [SerializeField] Transform virtualWorld;

    [Header("Translation Gain")]
    [SerializeField] float headingToCenterDotThreshold = 0.5f;
    [SerializeField] float maxExtraTranslationGain = 0.26f;
    [SerializeField] float minExtraTranslationGain = 0.14f;

    [Header("Rotation Gain")]
    [SerializeField] float extraStillRotationGain = 0.005f;
    [SerializeField] float maxExtraRotationGain = 0.49f;
    [SerializeField] float minExtraRotationGain = 0.2f;

    [Header("Curvature Gain")]
    [SerializeField] float minCurvatureGainRadius = 22f;

    PhysicalBoundaryManager physicalBoundaryManager;
    Vector3 RealWorldOrigin => physicalBoundaryManager.BoundaryCenter;
    float RealWorldRadius => physicalBoundaryManager.BoundaryRadius;
    float SafeRealWorldRadius => RealWorldRadius * 0.2f;

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
        float centerYaw = SignedAngleOnXZ(Vector3.forward, toCenter);
        float distToCenter = Vector3.Distance(camPos, center);
        float yawToRotateToFaceCenter = Mathf.DeltaAngle(camera.eulerAngles.y, centerYaw);

        float translationGain = ComputeTranslationGain(toCenter, distToCenter);
        float rotationGain = ComputeRotationGain(distToCenter, prevYawToRotateToFaceCenter, yawToRotateToFaceCenter);
        float curvatureGain = ComputeStillAndCurvatureGain(yawToRotateToFaceCenter);

        ApplyTranslationGain(translationGain);
        ApplyRotationGain(rotationGain);
        ApplyStillAndCurvatureGain(curvatureGain);

        prevPos = camera.position;
        prevYaw = camera.eulerAngles.y;
        prevYawToRotateToFaceCenter = yawToRotateToFaceCenter;
    }

    float ComputeTranslationGain(Vector3 toCenter, float distToCenter)
    {
        Vector3 translation = camera.position - prevPos;
        translation.y = 0;

        if (translation.magnitude < GlobalThresholds.EPS)
            return 1;

        float headingToCenter = Vector3.Dot(translation.normalized, toCenter);

        float extraGain;
        if (distToCenter <= SafeRealWorldRadius && headingToCenter > headingToCenterDotThreshold)
        {
            extraGain = -minExtraTranslationGain;
        }
        else if (distToCenter >= SafeRealWorldRadius && headingToCenter > headingToCenterDotThreshold)
        {
            extraGain = 0;
        }
        else
        {
            extraGain = maxExtraTranslationGain;
        }

        extraGain = Mathf.Max(Mathf.Abs(headingToCenter), headingToCenterDotThreshold) * extraGain;
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
}
