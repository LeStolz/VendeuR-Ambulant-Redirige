using UnityEngine;

[RequireComponent(typeof(PhysicalBoundaryManager))]
public class RedirectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] new Transform camera;
    [SerializeField] Transform virtualWorld;

    [Header("Translation Gain")]
    [SerializeField] float baseTranslationGain = 1.0f;
    [SerializeField] float maxExtraTranslationGain = 0.26f;
    [SerializeField] float minExtraTranslationGain = 0.14f;

    [Header("Rotation Gain")]
    [SerializeField] float baseRotationGain = 0.1f;
    [SerializeField] float maxExtraRotationGain = 0.49f;
    [SerializeField] float minExtraRotationGain = 0.2f;

    [Header("Curvature Gain")]
    [SerializeField] float minCurvatureGainRadius = 22f;

    [Header("Steer To Center")]
    [SerializeField] float steerStrength = 1.0f;

    PhysicalBoundaryManager physicalBoundaryManager;
    Vector3 RealWorldOrigin => physicalBoundaryManager.BoundaryCenter;
    float RealWorldRadius => physicalBoundaryManager.BoundaryRadius;
    float SafeRealWorldRadius => RealWorldRadius * 0.2f;

    Vector3 prevLocalPos;
    float prevLocalRot;

    void Start()
    {
        physicalBoundaryManager = GetComponent<PhysicalBoundaryManager>();

        prevLocalPos = camera.localPosition;
        prevLocalRot = GetYaw(camera.eulerAngles);
    }

    void Update()
    {
        float adaptiveT = ComputeAdaptiveTranslationGain();
        float adaptiveR = ComputeAdaptiveRotationGain();

        ApplyTranslationGain(adaptiveT);
        ApplyRotationGain(adaptiveR);
        // ApplyCurvatureGain();

        // SteerToCenter();
    }

    float ComputeAdaptiveTranslationGain()
    {
        Vector2 camPos = new(camera.position.x, camera.position.z);
        Vector2 center = new(RealWorldOrigin.x, RealWorldOrigin.z);

        float distToCenter = Vector2.Distance(camPos, center);
        Vector3 toCenter = (RealWorldOrigin - camera.position).normalized;
        float headingToCenter = Vector3.Dot(camera.forward, toCenter);

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

        return baseTranslationGain + extraGain;
    }

    // HERE
    float ComputeAdaptiveRotationGain()
    {
        Vector2 camPos = new Vector2(camera.position.x, camera.position.z);
        Vector2 center = new Vector2(RealWorldOrigin.x, RealWorldOrigin.z);
        float dist = Vector2.Distance(camPos, center);

        float distFactor = Mathf.InverseLerp(SafeRealWorldRadius, RealWorldRadius, dist);

        float extraGain = maxExtraRotationGain * distFactor;

        return baseRotationGain + extraGain;
    }

    void ApplyTranslationGain(float gain)
    {
        Vector3 delta = camera.localPosition - prevLocalPos;
        Vector3 modified = delta * gain;

        virtualWorld.position -= modified - delta;
        prevLocalPos = camera.localPosition;
    }

    // HERE
    void ApplyRotationGain(float gain)
    {
        float yaw = GetYaw(camera.eulerAngles);
        float deltaYaw = Mathf.DeltaAngle(prevLocalRot, yaw);
        float modified = deltaYaw * gain;

        virtualWorld.Rotate(Vector3.up, modified - deltaYaw);
        prevLocalRot = yaw;
    }

    void ApplyCurvatureGain()
    {
        // if (Mathf.Abs(baseCurvatureGain) < 0.0001f) return;

        // float speed = (camera.localPosition - prevLocalPos).magnitude;
        // float rotation = baseCurvatureGain * speed * Mathf.Rad2Deg;

        // virtualWorld.Rotate(Vector3.up, rotation);
    }

    // ============================================================
    //                  STEER TO CENTER (S2C)
    // ============================================================
    /// <summary>
    /// Gently rotates the virtual world to bend the user back toward center.
    /// </summary>
    void SteerToCenter()
    {
        Vector3 cam = camera.position;
        Vector3 center = RealWorldOrigin;

        Vector3 toCenter = (center - cam).normalized;

        // Signed angle between user's forward and center direction
        float angleToCenter = SignedAngleOnXZ(camera.forward, toCenter);

        // If user is drifting outward, nudge world opposite direction
        float steer = steerStrength * angleToCenter * Time.deltaTime;

        virtualWorld.Rotate(Vector3.up, steer);
    }

    float SignedAngleOnXZ(Vector3 a, Vector3 b)
    {
        a.y = 0; b.y = 0;
        return Vector3.SignedAngle(a, b, Vector3.up);
    }

    float GetYaw(Vector3 euler) => euler.y;
}
