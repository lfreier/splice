using System.Collections;
using UnityEngine;

public class PlayerStats
{
	public int[] keycardCount = new int[PickupDefs.MAX_KEYCARD_TYPE + 1];
	public int cells = 0;
	public int mutationBar = 0;

	public PlayerHUD hud;
	public void addItem(PickupInterface pickup)
	{
		switch (pickup.getPickupType())
		{
			case PickupDefs.pickupType.KEYCARD:
				/* Add the keycard to the player inventory by just incrementing */
				Keycard card = (Keycard)pickup;
				keycardCount[(int)card._keycardType]++;
				break;
			case PickupDefs.pickupType.CELL:
			default:
				cells += pickup.getCount();
				hud.updateCells(cells);
				break;
		}
	}
}