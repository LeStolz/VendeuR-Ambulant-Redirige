using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IceCream : MonoBehaviour
{
    [SerializeField] List<GameObject> iceCreamDisplays;
    [SerializeField] Renderer coneRenderer;
    [SerializeField] Material sprinklesMaterial;

    public CustomerOrderSO IceCreamOrder { get; private set; }
    List<IceCreamAddTrigger> triggers;

    void OnDestroy()
    {
        foreach (var trigger in triggers)
        {
            trigger.OnIceCreamComponentAdded -= HandleIceCreamComponentAdded;
        }
    }

    void HandleIceCreamComponentAdded(IceCreamComponentGO componentGO)
    {
        bool canAdd = false;
        var component = componentGO.component;
        var iceCreamFlavors = IceCreamOrder.iceCreamComponents.OfType<IceCreamFlavorComponent>();

        if (
            component is IceCreamFlavorComponent
            && iceCreamFlavors.Count() < IceCreamManager.Instance.IceCreamFlavors.Count
            && IceCreamOrder.iceCreamComponents.OfType<IceCreamToppingComponent>().Count() == 0
        )
        {
            canAdd = true;
        }

        if (
            component is IceCreamToppingComponent toppingComponent
            && iceCreamFlavors.Count() > 0
            && !IceCreamOrder.iceCreamComponents.Contains(toppingComponent)
        )
        {
            canAdd = true;
        }

        if (!canAdd) return;

        component.UpdateIceCreamComponentVisuals(iceCreamDisplays);
        componentGO.Consume();

        IceCreamOrder.iceCreamComponents.Add(component);
    }

    void Update()
    {
        var s = "";
        foreach (var component in IceCreamOrder.iceCreamComponents)
        {
            if (component is IceCreamToppingComponent tc)
            {
                s += tc.topping.name;
            }
            if (component is IceCreamFlavorComponent fc)
            {
                s += fc.flavor.name;
            }
        }

        Debug.Log(s);
    }

    public void Initialize(ConeFlavorSO coneFlavor)
    {
        IceCreamOrder = ScriptableObject.CreateInstance<CustomerOrderSO>();
        IceCreamOrder.iceCreamComponents = new List<IIceCreamComponent>();

        triggers = new List<IceCreamAddTrigger>(GetComponentsInChildren<IceCreamAddTrigger>());
        foreach (var trigger in triggers)
        {
            trigger.OnIceCreamComponentAdded += HandleIceCreamComponentAdded;
        }

        IceCreamOrder.coneFlavor = coneFlavor;
        coneRenderer.material.SetColor("_BaseColor", coneFlavor.color);
    }
}