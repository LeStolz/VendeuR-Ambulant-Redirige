using System;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Meta;
using UnityEngine.XR.OpenXR.NativeTypes;

public class BoundaryVisibilityController : MonoBehaviour
{
    public static BoundaryVisibilityController Instance { get; private set; }

    BoundaryVisibilityFeature _feature;

    public event Action<XrBoundaryVisibility> OnVisibilityChanged;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFeature();
    }

    void InitializeFeature()
    {
        _feature = OpenXRSettings.Instance.GetFeature<BoundaryVisibilityFeature>();

        if (_feature == null)
        {
            Debug.LogWarning("[BoundaryVisibilityController] BoundaryVisibilityFeature not turn On in OpenXR Settings.");
            return;
        }

        _feature.boundaryVisibilityChanged += HandleVisibilityChanged;

        XrResult result = _feature.TryRequestBoundaryVisibility(XrBoundaryVisibility.VisibilityNotSuppressed);
        Debug.Log("Request result: " + result);
    }

    void HandleVisibilityChanged(object sender, XrBoundaryVisibility visibility)
    {
        Debug.Log($"[BoundaryVisibility] Visibility changed → {visibility}");

        OnVisibilityChanged?.Invoke(visibility);
    }
}
