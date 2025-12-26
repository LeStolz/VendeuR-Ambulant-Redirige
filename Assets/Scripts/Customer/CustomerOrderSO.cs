using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomerOrderSO", menuName = "Scriptable Objects/CustomerOrder")]
public class CustomerOrderSO : ScriptableObject
{
	public ConeFlavorSO coneFlavor;
	public List<IceCreamFlavorSO> iceCreamFlavors;
	public List<ToppingSO> toppings;
}
