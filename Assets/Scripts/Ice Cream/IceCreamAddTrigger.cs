using System;
using UnityEngine;

public class IceCreamAddTrigger : MonoBehaviour
{
    public event Action<IceCreamComponent> OnIceCreamComponentAdded;

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IceCreamComponent>(out var iceCreamComponent))
        {
            return;
        }
        OnIceCreamComponentAdded?.Invoke(iceCreamComponent);
    }
}