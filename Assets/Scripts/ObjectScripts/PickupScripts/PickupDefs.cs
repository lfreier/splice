using UnityEditor;
using UnityEngine;

public static class PickupDefs
{
	public static string OBJECT_PICKUP_TAG = "ObjectPickup";

	public enum keycardType
	{
		RED = 0,
		BLUE = 1,
		YELLOW = 2,
		VIOLET = 3
	};

	public enum pickupType
	{
		CELL = 0,
		KEYCARD = 1,
		HEALTH_VIAL = 2,
		BATTERY = 3
	};

	static public int MAX_KEYCARD_TYPE = (int)keycardType.YELLOW;

	static public float CELL_ATTRACT_RANGE = 5F;
	static public float CELL_ACCELERATION = 10F;
	static public string CELL_ANIM_TRIGGER = "Pop";

	public static bool canBePickedUp(GameObject target)
	{
		return target.tag.StartsWith("Object") && target.tag.Equals(PickupDefs.OBJECT_PICKUP_TAG);
	}

	public static Color getKeycardColor(keycardType keyType)
	{
		switch (keyType)
		{
			case keycardType.VIOLET:
				return GameManager.COLOR_VIOLET;
			case keycardType.YELLOW:
				return GameManager.COLOR_YELLOW;
			case keycardType.BLUE:
				return GameManager.COLOR_BLUE;
			case keycardType.RED:
			default:
				return GameManager.COLOR_RED;
		}
	}
}