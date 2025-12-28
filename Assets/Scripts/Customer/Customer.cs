using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    CustomerOrderSO order;
    CustomerOrderTrigger customerOrderTrigger;

    [SerializeField] GameObject orderDisplayUI;
    [SerializeField] List<GameObject> orderComponentDisplays;

    void Start()
    {
        order = GenerateRandomOrder();
        customerOrderTrigger = GetComponentInChildren<CustomerOrderTrigger>();

        customerOrderTrigger.OnPlayerEnterRange += HandlePlayerEnterRange;
        customerOrderTrigger.OnPlayerExitRange += HandlePlayerExitRange;
    }

    void OnDestroy()
    {
        customerOrderTrigger.OnPlayerEnterRange -= HandlePlayerEnterRange;
        customerOrderTrigger.OnPlayerExitRange -= HandlePlayerExitRange;
    }

    void HandlePlayerEnterRange()
    {
        orderComponentDisplays.ForEach(go => go.SetActive(false));

        var coneDisplay = orderComponentDisplays.Find(go => go.name.Contains("Cone"));
        coneDisplay.GetComponentInChildren<Image>().color = order.coneFlavor.color;
        coneDisplay.SetActive(true);

        for (int i = 0; i < order.iceCreamComponents.Count; i++)
        {
            order.iceCreamComponents[i].UpdateIceCreamUIVisuals(orderComponentDisplays, i);
        }

        orderDisplayUI.SetActive(true);
    }

    void HandlePlayerExitRange()
    {
        orderDisplayUI.SetActive(false);
    }

    CustomerOrderSO GenerateRandomOrder()
    {
        CustomerOrderSO newOrder = ScriptableObject.CreateInstance<CustomerOrderSO>();

        newOrder.coneFlavor = GenerateRandomComponent(
            IceCreamManager.Instance.ConeFlavors
        );
        var iceCreamFlavors = GenerateRandomComponents(
            IceCreamManager.Instance.IceCreamFlavors, false, 1
        ).ConvertAll(flavorSO => new IceCreamFlavorComponent { flavor = flavorSO });
        var toppings = GenerateRandomComponents(
            IceCreamManager.Instance.Toppings, true, 0
        ).ConvertAll(toppingSO => new IceCreamToppingComponent { topping = toppingSO });

        newOrder.iceCreamComponents = new List<IIceCreamComponent>();
        newOrder.iceCreamComponents.AddRange(iceCreamFlavors);
        newOrder.iceCreamComponents.AddRange(toppings);

        return newOrder;
    }

    void OnTriggerEnter(Collider other)
    {
        if (
            !other.TryGetComponent<IceCream>(out var iceCream) &&
            !other.transform.parent.TryGetComponent<IceCream>(out iceCream) &&
            !other.transform.parent.parent.TryGetComponent<IceCream>(out iceCream)
        ) return;

        if (iceCream.IceCreamOrder.Equals(order))
        {
            Debug.Log("Customer served!");
            Destroy(iceCream.gameObject);
        }
        else
        {
            Debug.Log("Wrong order!");
        }
    }

    List<T> GenerateRandomComponents<T>(List<T> sourceList, bool unique, int minCount)
    {
        int maxCount = sourceList.Count;
        int count = Random.Range(minCount, maxCount + 1);
        List<T> items = new();

        if (unique && count >= sourceList.Count)
        {
            return new List<T>(sourceList);
        }

        for (int i = 0; i < count; i++)
        {
            T randomItem = GenerateRandomComponent(sourceList);
            items.Add(randomItem);
        }

        return items;
    }

    T GenerateRandomComponent<T>(List<T> sourceList)
    {
        int enumCount = sourceList.Count;
        return sourceList[Random.Range(0, enumCount)];
    }
}