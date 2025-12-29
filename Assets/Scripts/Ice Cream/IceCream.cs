using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCream : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_5 = new WaitForSeconds(0.5f);
    [SerializeField] List<GameObject> iceCreamDisplays;
    [SerializeField] Renderer coneRenderer;
    [SerializeField] Material sprinklesMaterial;

    public CustomerOrderSO IceCreamOrder { get; private set; }
    List<IceCreamAddTrigger> triggers;
    bool canAdd = true;

    void OnDestroy()
    {
        foreach (var trigger in triggers)
        {
            trigger.OnIceCreamComponentAdded -= HandleIceCreamComponentAdded;
        }
    }

    IEnumerator ResetCanAdd()
    {
        yield return _waitForSeconds0_5;
        canAdd = true;
    }

    void HandleIceCreamComponentAdded(IceCreamComponentGO componentGO)
    {
        if (!canAdd) return;
        canAdd = false;

        var component = componentGO.component;

        if (!component.CanAdd(IceCreamOrder.iceCreamComponents))
        {
            StartCoroutine(ResetCanAdd());
            return;
        }

        component.UpdateIceCreamComponentVisuals(iceCreamDisplays);
        componentGO.Consume();

        IceCreamOrder.iceCreamComponents.Add(component);

        StartCoroutine(ResetCanAdd());
    }

    public void Initialize(ConeFlavorSO coneFlavor)
    {
        IceCreamOrder = ScriptableObject.CreateInstance<CustomerOrderSO>();
        IceCreamOrder.iceCreamComponents = new List<IIceCreamComponent>();

        triggers = new List<IceCreamAddTrigger>(GetComponentsInChildren<IceCreamAddTrigger>(true));
        foreach (var trigger in triggers)
        {
            trigger.OnIceCreamComponentAdded += HandleIceCreamComponentAdded;
        }

        IceCreamOrder.coneFlavor = coneFlavor;
        coneRenderer.material.SetColor("_BaseColor", coneFlavor.color);
    }
}