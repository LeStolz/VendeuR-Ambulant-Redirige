using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public static RoadManager Instance { get; private set; }
    public List<Transform> RoadSegments { get; private set; } = new List<Transform>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    List<T> RandomShuffle<T>(List<T> sourceList)
    {
        var list = new List<T>(sourceList);

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[j], list[i]) = (list[i], list[j]);
        }

        return list;
    }

    void Start()
    {
        RoadSegments = GameObject.FindGameObjectsWithTag("Road")
            .Select(go => go.transform)
            .ToList();

        RoadSegments = RandomShuffle(RoadSegments);
    }
}