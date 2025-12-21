using System;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Meta;
using UnityEngine.XR.OpenXR.NativeTypes;

public class PhysicalBoundaryVisibilityManager : MonoBehaviour
{
    BoundaryVisibilityFeature visibilityFeature;

    public event Action<XrBoundaryVisibility> OnVisibilityChanged;

    void Awake()
    {
        InitializeVisibilityFeature();
    }

    void InitializeVisibilityFeature()
    {
        visibilityFeature = OpenXRSettings.Instance.GetFeature<BoundaryVisibilityFeature>();

        if (visibilityFeature == null)
        {
            Debug.LogWarning("BoundaryVisibilityFeature not found in OpenXR settings.");
            return;
        }

        visibilityFeature.boundaryVisibilityChanged += HandleVisibilityChanged;
    }

    void OnDestroy()
    {
        if (visibilityFeature != null)
        {
            visibilityFeature.boundaryVisibilityChanged -= HandleVisibilityChanged;
        }
    }

    public void SetBoundaryVisibility(XrBoundaryVisibility visibility)
    {
        if (visibilityFeature != null)
        {
            XrResult result = visibilityFeature.TryRequestBoundaryVisibility(visibility);
        }
    }

    void HandleVisibilityChanged(object sender, XrBoundaryVisibility visibility)
    {
        OnVisibilityChanged?.Invoke(visibility);
    }
}
