using System;
using UnityEngine;

public class CustomerOrderTrigger : MonoBehaviour
{
    public event Action OnPlayerEnterRange;
    public event Action OnPlayerExitRange;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnterRange?.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExitRange?.Invoke();
        }
    }
}