using System;
using UnityEngine;

public class IceCreamAddTrigger : MonoBehaviour
{
    public event Action<IceCreamComponentGO> OnIceCreamComponentAdded;

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IceCreamComponentGO>(out var iceCreamComponent))
        {
            return;
        }
        OnIceCreamComponentAdded?.Invoke(iceCreamComponent);
    }
}