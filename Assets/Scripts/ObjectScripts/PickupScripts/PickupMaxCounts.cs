using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPickupMax", menuName = "ScriptableObjects/PickupMaxCounts")]

public class PickupMaxCounts : ScriptableObject
{
	[field: SerializeField]
	public int healthVialMax { get; private set; } = 3;

	[field: SerializeField]
	public int batteryMax { get; private set; } = 3;

	[field: SerializeField]
	public int refillMax { get; private set; } = 3;
}