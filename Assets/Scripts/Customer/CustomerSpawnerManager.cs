using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawnerManager : MonoBehaviour
{
    public static CustomerSpawnerManager Instance { get; private set; }

    [SerializeField] List<Customer> customerPrefabs;
    [SerializeField] float spawnDistanceMinThreshold = 10f;
    [SerializeField] float spawnDistanceMaxThreshold = 100f;
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
            yield return new WaitForSeconds(1f);

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
                spawnDistanceMinThreshold < distanceToPlayer
                && distanceToPlayer < spawnDistanceMaxThreshold
                && !existingCustomers.Exists(customer => customer.transform.parent.position == segment.position)
            )
            {
                var randomCustomer = customerPrefabs[Random.Range(0, customerPrefabs.Count)];
                var width = segment.localScale.x * 5f;
                var randomCustomerPosition =
                    segment.transform.position
                    + Vector3.up * randomCustomer.transform.localScale.y
                    + Vector3.right * Random.Range(-width, width)
                    + Vector3.forward * Random.Range(-width, width);

                var randomCustomerColor = Random.ColorHSV();

                var customer = Instantiate(
                    randomCustomer,
                    randomCustomerPosition,
                    segment.transform.rotation,
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
