using System.Collections;
using UnityEngine;

public class PlayerInventory
{
	public int[] keycardCount = new int[PickupDefs.MAX_KEYCARD_TYPE + 1];

	public void addItem(PickupInterface pickup)
	{
		switch (pickup.getPickupType())
		{
			case PickupDefs.pickupType.KEYCARD:
			default:
				/* Add the keycard to the player inventory by just incrementing */
				Keycard card = (Keycard)pickup;
				keycardCount[(int)card._keycardType]++;
				break;
		}
	}
}