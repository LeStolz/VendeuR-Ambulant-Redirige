using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;

public class IceCreamToppingComponent : IIceCreamComponent
{
    public ToppingSO topping;
    public Material sprinklesMaterial;

    public void UpdateIceCreamComponentVisuals(List<GameObject> visuals)
    {
        var visual = visuals.Last(go => go.activeInHierarchy);
        if (topping.name.Contains("Sprinkles"))
        {
            visual.GetComponent<Renderer>().AddMaterial(sprinklesMaterial);
        }
        else if (topping.name.Contains("Milk"))
        {
            visual.transform.Find("Condensed Milk").gameObject.SetActive(true);
        }
    }

    public void Consume(GameObject gameObject)
    {
    }

    public void UpdateIceCreamUIVisuals(List<GameObject> uiVisuals, int index)
    {
        var toppingDisplay = uiVisuals.Find(go => go.name.Contains(topping.name));
        toppingDisplay.SetActive(true);
    }

    public bool Equals(IIceCreamComponent other)
    {
        return other is IceCreamToppingComponent otherTopping &&
               topping == otherTopping.topping;
    }

    public bool CanAdd(List<IIceCreamComponent> currentComponents)
    {
        return currentComponents.OfType<IceCreamFlavorComponent>().Count() > 0 && !currentComponents.Contains(this);
    }
}
