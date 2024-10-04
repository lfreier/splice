using UnityEditor;
using UnityEngine;

public static class PickupDefs
{
	public static string OBJECT_PICKUP_TAG = "ObjectPickup";

	public enum keycardType
	{
		RED = 0,
		BLUE = 1
	};

	public enum pickupType
	{
		CELL = 0,
		KEYCARD = 1,
		HEALTH_VIAL = 2
	};

	static public int MAX_KEYCARD_TYPE = (int)keycardType.BLUE;

	static public float CELL_ATTRACT_RANGE = 5F;
	static public float CELL_ACCELERATION = 10F;
	static public string CELL_ANIM_TRIGGER = "Pop";

	public static bool canBePickedUp(GameObject target)
	{
		return target.tag.StartsWith("Object") && target.tag.Equals(PickupDefs.OBJECT_PICKUP_TAG);
	}
}