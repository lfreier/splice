using UnityEditor;
using UnityEngine;

public static class PickupDefs
{
	public static string OBJECT_PICKUP_TAG = "ObjectPickup";

	public static bool canBePickedUp(GameObject target)
	{
		return target.tag.StartsWith("Object") && target.tag.Equals(PickupDefs.OBJECT_PICKUP_TAG);
	}
}