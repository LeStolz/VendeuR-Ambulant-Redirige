using UnityEngine;

public class RedirectionController : MonoBehaviour
{
    [Header("References")]
    public Transform vrCamera;
    public Transform realWorldOrigin;
    public Transform virtualOrigin;

    [Header("Base Gains")]
    public float baseTranslationGain = 1.0f;
    public float baseRotationGain = 1.0f;
    public float curvatureGain = 0f;

    [Header("Adaptive Gain Parameters")]
    public float maxExtraTranslationGain = 0.25f; // +25% max when at boundary
    public float maxExtraRotationGain = 0.2f;     // +20% steering rotation
    public float physicalRadius = 2.5f;           // physical room radius (meters)
    public float safeRadius = 1.0f;               // inside this = no redirection

    [Header("Steer-To-Center")]
    public float steerStrength = 1.0f; // how strongly S2C rotates scene

    private Vector3 prevLocalPos;
    private float prevYaw;

    void Start()
    {
        prevLocalPos = vrCamera.localPosition;
        prevYaw = GetYaw(vrCamera.eulerAngles);
    }

    void Update()
    {
        float adaptiveT = ComputeAdaptiveTranslationGain();
        float adaptiveR = ComputeAdaptiveRotationGain();

        ApplyTranslationGain(adaptiveT);
        ApplyRotationGain(adaptiveR);
        ApplyCurvatureGain();

        SteerToCenter();     // NEW
    }

    // ============================================================
    //            ADAPTIVE GAINS
    // ============================================================

    /// <summary>
    /// Translation gain increases when user is far from center or moving away.
    /// </summary>
    float ComputeAdaptiveTranslationGain()
    {
        Vector2 camPos = new Vector2(vrCamera.position.x, vrCamera.position.z);
        Vector2 center = new Vector2(realWorldOrigin.position.x, realWorldOrigin.position.z);
        float dist = Vector2.Distance(camPos, center);

        // Distance factor (0 to 1)
        float distFactor = Mathf.InverseLerp(safeRadius, physicalRadius, dist);

        // Heading factor: 1 when walking outward, 0 when inward
        Vector3 toCenter = (realWorldOrigin.position - vrCamera.position).normalized;
        float headingDot = Vector3.Dot(vrCamera.forward, -toCenter);
        float headingFactor = Mathf.Clamp01(headingDot); // only boost if facing outward

        float extraGain = maxExtraTranslationGain * (0.5f * distFactor + 0.5f * headingFactor);

        return baseTranslationGain + extraGain;
    }

    /// <summary>
    /// Rotation gain amplifies outward-facing turns, or adds bias when far from center.
    /// </summary>
    float ComputeAdaptiveRotationGain()
    {
        Vector2 camPos = new Vector2(vrCamera.position.x, vrCamera.position.z);
        Vector2 center = new Vector2(realWorldOrigin.position.x, realWorldOrigin.position.z);
        float dist = Vector2.Distance(camPos, center);

        float distFactor = Mathf.InverseLerp(safeRadius, physicalRadius, dist);

        float extraGain = maxExtraRotationGain * distFactor;

        return baseRotationGain + extraGain;
    }

    // ============================================================
    //                  GAINS IMPLEMENTATION
    // ============================================================

    void ApplyTranslationGain(float gain)
    {
        Vector3 delta = vrCamera.localPosition - prevLocalPos;
        Vector3 modified = delta * gain;
        virtualOrigin.position -= (modified - delta);
        prevLocalPos = vrCamera.localPosition;
    }

    void ApplyRotationGain(float gain)
    {
        float yaw = GetYaw(vrCamera.eulerAngles);
        float deltaYaw = Mathf.DeltaAngle(prevYaw, yaw);
        float modified = deltaYaw * gain;

        virtualOrigin.Rotate(Vector3.up, modified - deltaYaw);
        prevYaw = yaw;
    }

    void ApplyCurvatureGain()
    {
        if (Mathf.Abs(curvatureGain) < 0.0001f) return;

        float speed = (vrCamera.localPosition - prevLocalPos).magnitude;
        float rotation = curvatureGain * speed * Mathf.Rad2Deg;

        virtualOrigin.Rotate(Vector3.up, rotation);
    }

    // ============================================================
    //                  STEER TO CENTER (S2C)
    // ============================================================
    /// <summary>
    /// Gently rotates the virtual world to bend the user back toward center.
    /// </summary>
    void SteerToCenter()
    {
        Vector3 cam = vrCamera.position;
        Vector3 center = realWorldOrigin.position;

        Vector3 toCenter = (center - cam).normalized;

        // Signed angle between user's forward and center direction
        float angleToCenter = SignedAngleOnXZ(vrCamera.forward, toCenter);

        // If user is drifting outward, nudge world opposite direction
        float steer = steerStrength * angleToCenter * Time.deltaTime;

        virtualOrigin.Rotate(Vector3.up, steer);
    }

    float SignedAngleOnXZ(Vector3 a, Vector3 b)
    {
        a.y = 0; b.y = 0;
        return Vector3.SignedAngle(a, b, Vector3.up);
    }

    float GetYaw(Vector3 euler) => euler.y;
}
