using UnityEditor;
using UnityEngine;

public static class PickupDefs
{
	public static string OBJECT_PICKUP_TAG = "ObjectPickup";

	public enum pickupType
	{
		KEYCARD = 0
	};

	public enum keycardType
	{
		RED = 0,
		BLUE = 1
	};

	static public int MAX_KEYCARD_TYPE = (int)keycardType.BLUE;

	public static bool canBePickedUp(GameObject target)
	{
		return target.tag.StartsWith("Object") && target.tag.Equals(PickupDefs.OBJECT_PICKUP_TAG);
	}
}