using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CustomerVisuals))]
public class Customer : MonoBehaviour
{
    CustomerOrderSO order;
    CustomerVisuals customerVisuals;

    [SerializeField] List<GameObject> orderComponentDisplays;
    [SerializeField] AudioSource orderCompleteAudioSource;
    [SerializeField] AudioSource orderWrongAudioSource;
    [SerializeField] float orderDistanceThreshold = 4f;

    Transform target;
    bool orderGiven = false;

    void Start()
    {
        order = GenerateRandomOrder();
        customerVisuals = GetComponent<CustomerVisuals>();

        target = Camera.main.transform;
    }

    void Update()
    {
        if (!orderGiven && Vector3.Distance(target.position, transform.position) < orderDistanceThreshold)
        {
            HandlePlayerEnterRange();
            customerVisuals.HandlePlayerEnterRange();
            orderGiven = true;
        }
        else if (orderGiven && Vector3.Distance(target.position, transform.position) >= orderDistanceThreshold)
        {
            customerVisuals.HandlePlayerExitRange();
            orderGiven = false;
        }
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
            !other.transform.parent.TryGetComponent(out iceCream) &&
            !other.transform.parent.parent.TryGetComponent(out iceCream)
        ) return;

        if (iceCream.IceCreamOrder.Equals(order))
        {
            orderCompleteAudioSource.Play();
            customerVisuals.SetEmotion(EyeEmotion.Happy);
            HapticManager.Instance.TriggerSucessHaptic(HapticManager.Hand.Both);
            Destroy(iceCream.gameObject);

            IEnumerator DespawnAfterSound()
            {
                yield return new WaitForSeconds(orderCompleteAudioSource.clip.length);
                CustomerSpawnerManager.Instance.DespawnCustomer(this);
            }

            StartCoroutine(DespawnAfterSound());
        }
        else
        {
            orderWrongAudioSource.Play();
            customerVisuals.SetEmotion(EyeEmotion.Angry);
            HapticManager.Instance.TriggerFailureHaptic(HapticManager.Hand.Both);
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