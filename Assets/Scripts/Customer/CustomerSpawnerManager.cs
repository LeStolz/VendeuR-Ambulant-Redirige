using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawnerManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_4 = new WaitForSeconds(0.4f);

    public static CustomerSpawnerManager Instance { get; private set; }

    [SerializeField] List<Customer> customerPrefabs;
    [SerializeField] float minSpawnDistanceFromPlayer = 10f;
    [SerializeField] float maxSpawnDistanceFromPlayer = 50f;
    [SerializeField] float minSpawnDistanceFromOtherCustomers = 10f;
    [SerializeField] int minCustomers = 8;
    List<Customer> existingCustomers = new();
    GameObject player;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        IEnumerator StartCouroutine()
        {
            yield return _waitForSeconds0_4;

            bool possibleToSpawn = true;
            while (existingCustomers.Count < minCustomers && possibleToSpawn)
            {
                possibleToSpawn = SpawnCustomer();
            }
        }

        StartCoroutine(StartCouroutine());
    }

    bool SpawnCustomer()
    {
        var roadSegments = RoadManager.Instance.RoadSegments;
        bool possibleToSpawn = false;

        foreach (var segment in roadSegments)
        {
            var distanceToPlayer = Vector3.Distance(segment.transform.position, player.transform.position);

            if (
                minSpawnDistanceFromPlayer < distanceToPlayer && distanceToPlayer < maxSpawnDistanceFromPlayer
                && !existingCustomers.Exists(
                    customer => Vector3.Distance(
                        customer.transform.parent.position, segment.position
                    ) < minSpawnDistanceFromOtherCustomers
                )
            )
            {
                var randomCustomer = customerPrefabs[Random.Range(0, customerPrefabs.Count)];
                var width = segment.localScale.x * 5f;
                var randomCustomerPosition =
                    segment.transform.position
                    + Vector3.up * randomCustomer.transform.localScale.y
                    + Vector3.right * Random.Range(-width, width)
                    + Vector3.forward * Random.Range(-width, width);
                var randomCustomerRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                var randomCustomerColor = Random.ColorHSV();

                var customer = Instantiate(
                    randomCustomer,
                    randomCustomerPosition,
                    randomCustomerRotation,
                    segment.transform
                );

                customer.GetComponentInChildren<Renderer>().material.color = randomCustomerColor;
                existingCustomers.Add(customer);
                possibleToSpawn = true;
                break;
            }
        }

        return possibleToSpawn;
    }

    public void DespawnCustomer(Customer customer)
    {
        existingCustomers.Remove(customer);
        Destroy(customer.gameObject);
        SpawnCustomer();
    }
}
