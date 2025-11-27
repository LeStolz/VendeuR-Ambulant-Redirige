using System;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Meta;

public class BoundaryVisibilityController : MonoBehaviour
{
    public static BoundaryVisibilityController Instance { get; private set; }

    BoundaryVisibilityFeature _feature;

    /// <summary>
    /// Event bạn có thể bắt ở script khác.
    /// Gửi lên khi boundary visibility thực sự thay đổi.
    /// </summary>
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
            Debug.LogWarning("[BoundaryVisibilityController] BoundaryVisibilityFeature chưa được bật trong OpenXR Settings.");
            return;
        }

        // Lắng nghe callback từ feature
        _feature.boundaryVisibilityChanged += HandleVisibilityChanged;
    }

    /// <summary>
    /// Đây là hàm được gọi khi runtime thật sự thay đổi visibility.
    /// (được bắn từ native → feature → controller)
    /// </summary>
    void HandleVisibilityChanged(object sender, XrBoundaryVisibility visibility)
    {
        Debug.Log($"[BoundaryVisibility] Visibility changed → {visibility}");

        OnVisibilityChanged?.Invoke(visibility);
    }
}
