using UnityEngine;

public class IceCreamContainer : MonoBehaviour
{
    private Material iceCreamMat;
    [SerializeField] private GameObject iceCream;

    void Start()
    {
        iceCreamMat = GetComponent<MeshRenderer>().materials[0];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Spoon"))
        {
            MakeIceCream();
        }
    }

    private void MakeIceCream()
    {
        Debug.Log("Creating ice cream...");
        iceCream.GetComponent<Renderer>().material = iceCreamMat;
        iceCream.SetActive(true);
    }
}
