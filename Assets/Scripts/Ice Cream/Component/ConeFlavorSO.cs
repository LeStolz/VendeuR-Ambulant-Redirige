using UnityEngine;

public enum ConeFlavor
{
	Vanilla,
	Chocolate,
	Strawberry,
}

[CreateAssetMenu(fileName = "ConeFlavorSO", menuName = "Scriptable Objects/ConeFlavor")]
public class ConeFlavorSO : ScriptableObject
{
	public ConeFlavor flavor;
	public Color color;
	public GameObject prefab;
}
