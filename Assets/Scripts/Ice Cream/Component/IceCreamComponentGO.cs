using UnityEngine;

public class IceCreamComponentGO : MonoBehaviour
{
    [SerializeField] ToppingSO topping;
    [SerializeField] IceCreamFlavorSO flavor;

    [SerializeField] Material sprinklesMaterial;
    public IIceCreamComponent component;

    void Start()
    {
        if (topping != null)
        {
            component = new IceCreamToppingComponent { topping = topping, sprinklesMaterial = sprinklesMaterial };
        }
        else
        {
            component = new IceCreamFlavorComponent { flavor = flavor };
        }
    }

    public void Consume()
    {
        component.Consume(gameObject);
    }
}
