using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class IceCreamContainer : MonoBehaviour
{
    [SerializeField] IceCreamFlavorSO iceCreamFlavor;

    void Start()
    {
        GetComponent<Renderer>().material.SetColor("_BaseColor", iceCreamFlavor.color);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Spoon"))
        {
            var iceCream = other.GetComponentInChildren<IceCreamComponentGO>(true);
            iceCream.gameObject.SetActive(true);
            iceCream.component = new IceCreamFlavorComponent { flavor = iceCreamFlavor };
            iceCream.GetComponent<Renderer>().material.SetColor("_BaseColor", iceCreamFlavor.color);
            iceCream.Interact();
        }
    }
}
