using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    CustomerOrderSO order;
    CustomerOrderTrigger orderTriggeer;

    [SerializeField] GameObject orderDisplayUI;
    [SerializeField] List<GameObject> orderComponentDisplays;

    void Start()
    {
        order = GenerateRandomOrder();
        orderTriggeer = GetComponentInChildren<CustomerOrderTrigger>();

        orderTriggeer.OnPlayerEnterRange += HandlePlayerEnterRange;
        orderTriggeer.OnPlayerExitRange += HandlePlayerExitRange;
    }

    void OnDestroy()
    {
        orderTriggeer.OnPlayerEnterRange -= HandlePlayerEnterRange;
        orderTriggeer.OnPlayerExitRange -= HandlePlayerExitRange;
    }

    void HandlePlayerEnterRange()
    {
        orderComponentDisplays.ForEach(go => go.SetActive(false));

        var coneDisplay = orderComponentDisplays.Find(go => go.name.Contains("Cone"));
        coneDisplay.GetComponentInChildren<Image>().color = order.coneFlavor.color;
        coneDisplay.SetActive(true);

        for (int i = 0; i < order.iceCreamFlavors.Count; i++)
        {
            var iceCreamDisplay = orderComponentDisplays.Find(go => go.name.Contains($"Ice Cream ({i})"));
            iceCreamDisplay.GetComponentInChildren<Image>().color = order.iceCreamFlavors[i].color;
            iceCreamDisplay.SetActive(true);
        }

        for (int i = 0; i < order.toppings.Count; i++)
        {
            var toppingDisplay = orderComponentDisplays.Find(go => go.name.Contains(order.toppings[i].name));
            toppingDisplay.SetActive(true);
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
        newOrder.iceCreamFlavors = GenerateRandomComponents(
            IceCreamManager.Instance.IceCreamFlavors, false, 1, 3
        );
        newOrder.toppings = GenerateRandomComponents(
            IceCreamManager.Instance.Toppings, true, 0, 2
        );

        return newOrder;
    }

    List<T> GenerateRandomComponents<T>(List<T> sourceList, bool unique, int minCount, int maxCount)
    {
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