using UnityEngine;

public class IceCreamMaking : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("cone"))
        {
            Debug.Log("Ice cream has been added to the cone!");
        }
    }
}
