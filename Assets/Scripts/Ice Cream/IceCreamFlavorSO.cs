using UnityEngine;

public enum IceCreamFlavor
{
	Matcha,
	Chocolate,
	Strawberry,
}

[CreateAssetMenu(fileName = "IceCreamFlavorSO", menuName = "Scriptable Objects/IceCreamFlavor")]
public class IceCreamFlavorSO : ScriptableObject
{
	public IceCreamFlavor flavor;
	public Color color;
	public GameObject prefab;
}
