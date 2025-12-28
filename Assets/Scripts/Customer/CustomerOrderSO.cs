using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomerOrderSO", menuName = "Scriptable Objects/CustomerOrder")]
public class CustomerOrderSO : ScriptableObject, IEquatable<CustomerOrderSO>
{
	public ConeFlavorSO coneFlavor;
	public List<IIceCreamComponent> iceCreamComponents;

	public bool Equals(CustomerOrderSO other)
	{
		if (coneFlavor != other.coneFlavor) return false;
		if (iceCreamComponents.Count != other.iceCreamComponents.Count) return false;

		for (int i = 0; i < iceCreamComponents.Count; i++)
		{
			if (!iceCreamComponents.Exists(component => component.Equals(other.iceCreamComponents[i])))
			{
				return false;
			}
		}

		return true;
	}
}
