using UnityEngine;

public enum Topping
{
	Sprinkles,
	CondensedMilk,
}

[CreateAssetMenu(fileName = "ToppingSO", menuName = "Scriptable Objects/Topping")]
public class ToppingSO : ScriptableObject
{
	public Topping type;
	public float price;
	public GameObject prefab;
}
