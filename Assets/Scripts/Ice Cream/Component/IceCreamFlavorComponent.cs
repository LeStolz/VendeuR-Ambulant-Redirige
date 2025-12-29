using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class IceCreamFlavorComponent : IIceCreamComponent
{
    public IceCreamFlavorSO flavor;

    public void UpdateIceCreamComponentVisuals(List<GameObject> visuals)
    {
        if (flavor == null) return;

        var visual = visuals.First(go => !go.activeInHierarchy);

        visual.GetComponent<Renderer>().material.SetColor("_BaseColor", flavor.color);
        var iceCreamDisplayRenderers = visual.GetComponentsInChildren<Renderer>();
        foreach (var renderer in iceCreamDisplayRenderers)
        {
            renderer.material.SetColor("_BaseColor", flavor.color);
        }

        visual.SetActive(true);
    }

    public void Consume(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    public void UpdateIceCreamUIVisuals(List<GameObject> uiVisuals, int index)
    {
        var iceCreamDisplay = uiVisuals.Find(go => go.name.Contains($"Ice Cream ({index})"));
        iceCreamDisplay.GetComponentInChildren<Image>().color = flavor.color;
        iceCreamDisplay.SetActive(true);
    }

    public bool Equals(IIceCreamComponent other)
    {
        return other is IceCreamFlavorComponent otherFlavorComponent &&
               flavor == otherFlavorComponent.flavor;
    }

    public bool CanAdd(List<IIceCreamComponent> currentComponents)
    {
        return currentComponents.OfType<IceCreamFlavorComponent>().Count() < IceCreamManager.Instance.IceCreamFlavors.Count
             && currentComponents.OfType<IceCreamToppingComponent>().Count() == 0;
    }
}
