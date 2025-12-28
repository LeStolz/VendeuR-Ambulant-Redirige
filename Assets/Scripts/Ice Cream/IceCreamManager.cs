using System.Collections.Generic;
using UnityEngine;

public class IceCreamManager : MonoBehaviour
{
	public static IceCreamManager Instance { get; private set; }

	[SerializeField] List<IceCreamFlavorSO> iceCreamFlavors;
	[SerializeField] List<ConeFlavorSO> coneFlavors;
	[SerializeField] List<ToppingSO> toppings;

	public List<IceCreamFlavorSO> IceCreamFlavors { get => iceCreamFlavors; private set => iceCreamFlavors = value; }
	public List<ConeFlavorSO> ConeFlavors { get => coneFlavors; private set => coneFlavors = value; }
	public List<ToppingSO> Toppings { get => toppings; private set => toppings = value; }

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
	}
}