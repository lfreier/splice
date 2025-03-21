﻿using UnityEditor;
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
		BATTERY = 3,
		PETRI_DISH = 4
	};

	public enum usableType
	{
		HEALTH_VIAL = 0,
		BATTERY = 1,
		REFILL = 2
	};

	static public int MAX_KEYCARD_TYPE	= (int)keycardType.VIOLET;
	static public int MAX_PICKUP_ITEM_TYPE = (int)pickupType.PETRI_DISH;
	static public int MAX_USABLE_ITEM_TYPE	= (int)usableType.REFILL;

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

	public static int pickupToUsable(pickupType type)
	{
		switch (type)
		{
			case pickupType.HEALTH_VIAL:
				return (int)usableType.HEALTH_VIAL;
			case pickupType.BATTERY:
				return (int)usableType.BATTERY;
			case pickupType.PETRI_DISH:
				return (int)usableType.REFILL;
			default:
				return -1;
		}
	}
}