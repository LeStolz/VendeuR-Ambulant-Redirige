using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawnerManager : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_4 = new WaitForSeconds(0.4f);

    public static CustomerSpawnerManager Instance { get; private set; }

    [SerializeField] bool experimentalMode = false;
    [SerializeField] List<Customer> customerPrefabs;
    [SerializeField] float minSpawnDistanceFromPlayer = 10f;
    [SerializeField] float maxSpawnDistanceFromPlayer = 50f;
    [SerializeField] float minSpawnDistanceFromOtherCustomers = 10f;
    [SerializeField] int minCustomers = 8;
    public List<Customer> ExistingCustomers { get; private set; } = new();
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

        if (!experimentalMode)
        {
            IEnumerator StartCouroutine()
            {
                yield return _waitForSeconds0_4;

                bool possibleToSpawn = true;
                while (ExistingCustomers.Count < minCustomers && possibleToSpawn)
                {
                    possibleToSpawn = SpawnCustomer() != null;
                }
            }

            StartCoroutine(StartCouroutine());
        }
    }

    public Customer SpawnCustomer(Vector3 position = default)
    {
        var roadSegments = RoadManager.Instance.RoadSegments;
        var segmentWidth = roadSegments[0].localScale.x * 5f;
        var segmentPosition = position;

        if (experimentalMode)
        {
            position = new Vector3(0, 0, 8);
            segmentPosition = position;
        }

        if (position == default)
            foreach (var segment in roadSegments)
            {
                var distanceToPlayer = Vector3.Distance(segment.transform.position, player.transform.position);

                if (
                    minSpawnDistanceFromPlayer < distanceToPlayer && distanceToPlayer < maxSpawnDistanceFromPlayer
                    && !ExistingCustomers.Exists(customer => Vector3.Distance(
                        customer.transform.position, segment.position
                    ) < minSpawnDistanceFromOtherCustomers)
                )
                {
                    segmentPosition = segment.position;
                    break;
                }
            }
        else
        {
            if (experimentalMode && ExistingCustomers.Exists(customer => Vector3.Distance(
                    customer.transform.position, position
                ) < 1)
            )
            {
                return null;
            }

            if (!experimentalMode && ExistingCustomers.Exists(customer => Vector3.Distance(
                    customer.transform.position, position
                ) < minSpawnDistanceFromOtherCustomers)
            )
            {
                return null;
            }
        }

        if (segmentPosition == default) return null;

        var randomCustomer = customerPrefabs[Random.Range(0, customerPrefabs.Count)];
        var randomCustomerPosition =
            position != default ? segmentPosition + Vector3.up * randomCustomer.transform.localScale.y :
            segmentPosition
            + Vector3.up * randomCustomer.transform.localScale.y
            + Vector3.right * Random.Range(-segmentWidth, segmentWidth)
            + Vector3.forward * Random.Range(-segmentWidth, segmentWidth);
        var randomCustomerRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        var randomCustomerColor = Random.ColorHSV();

        var customer = Instantiate(randomCustomer, randomCustomerPosition, randomCustomerRotation);
        customer.GetComponentInChildren<Renderer>().material.color = randomCustomerColor;
        ExistingCustomers.Add(customer);

        return customer;
    }

    public void DespawnCustomer(Customer customer)
    {
        ExistingCustomers.Remove(customer);
        StartCoroutine(RespawnCustomerCoroutine(customer));
    }

    IEnumerator RespawnCustomerCoroutine(Customer customer)
    {
        while (customer != null && customer.gameObject.transform.position.y > -5f)
        {
            customer.gameObject.transform.position += Vector3.down * Time.deltaTime;
            yield return null;
        }

        if (!experimentalMode) SpawnCustomer();
    }
}
