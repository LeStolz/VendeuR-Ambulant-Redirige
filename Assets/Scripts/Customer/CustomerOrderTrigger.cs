using System;
using UnityEngine;

class CustomerOrderTrigger : MonoBehaviour
{
    public event Action OnPlayerEnterRange;
    public event Action OnPlayerExitRange;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            OnPlayerEnterRange?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            OnPlayerExitRange?.Invoke();
        }
    }
}