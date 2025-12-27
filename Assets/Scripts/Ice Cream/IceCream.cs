using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class IceCream : MonoBehaviour
{
    [SerializeField] List<GameObject> iceCreamDisplays;
    [SerializeField] Material sprinklesMaterial;

    public CustomerOrderSO IceCreamOrder { get; private set; }
    List<IceCreamAddTrigger> triggers;

    void Start()
    {
        IceCreamOrder = ScriptableObject.CreateInstance<CustomerOrderSO>();

        triggers = new List<IceCreamAddTrigger>(GetComponentsInChildren<IceCreamAddTrigger>());
        foreach (var trigger in triggers)
        {
            trigger.OnIceCreamComponentAdded += HandleIceCreamComponentAdded;
        }
    }

    void OnDestroy()
    {
        foreach (var trigger in triggers)
        {
            trigger.OnIceCreamComponentAdded -= HandleIceCreamComponentAdded;
        }
    }

    void HandleIceCreamComponentAdded(IceCreamComponent component)
    {
        if (
            component.flavor != null
            && IceCreamOrder.iceCreamFlavors.Count < iceCreamDisplays.Count
            && IceCreamOrder.toppings.Count == 0)
        {
            IceCreamOrder.iceCreamFlavors.Add(component.flavor);
            iceCreamDisplays[IceCreamOrder.iceCreamFlavors.Count - 1]
                .GetComponent<Renderer>().material.color = component.flavor.color;
            iceCreamDisplays[IceCreamOrder.iceCreamFlavors.Count - 1].SetActive(true);
            Destroy(component.gameObject);
        }

        if (
            component.topping != null
            && IceCreamOrder.iceCreamFlavors.Count > 0
            && !IceCreamOrder.toppings.Contains(component.topping)
        )
        {
            IceCreamOrder.toppings.Add(component.topping);

            if (component.topping.name.Contains("Sprinkles"))
            {
                iceCreamDisplays[IceCreamOrder.toppings.Count - 1]
                    .GetComponent<Renderer>().AddMaterial(sprinklesMaterial);
            }
            else if (component.topping.name.Contains("Condensed Milk"))
            {
                iceCreamDisplays[IceCreamOrder.toppings.Count - 1]
                    .transform.Find("Condensed Milk").gameObject.SetActive(true);
            }

            Destroy(component.gameObject);
        }
    }

    public void Initialize(ConeFlavorSO coneFlavor)
    {
        IceCreamOrder.coneFlavor = coneFlavor;
    }
}