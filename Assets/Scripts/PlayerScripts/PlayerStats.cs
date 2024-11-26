using System.Collections;
using UnityEngine;

public class PlayerStats
{
	public int[] keycardCount = new int[PickupDefs.MAX_KEYCARD_TYPE + 1];
	public int cells = 0;
	public float mutationBar = 0;
	public float maxMutationBar = 100;
	private ActorDefs.ActorData playerData;

	private GameManager gameManager = null;

	public void addItem(PickupInterface pickup)
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}
		switch (pickup.getPickupType())
		{
			case PickupDefs.pickupType.KEYCARD:
				/* Add the keycard to the player inventory by just incrementing */
				Keycard card = (Keycard)pickup;
				keycardCount[(int)card._keycardType]++;
				gameManager.signalUpdateKeycardCount(keycardCount[(int)card._keycardType], card._keycardType);
				break;
			case PickupDefs.pickupType.CELL:
			default:
				int toAdd = pickup.getCount();
				cells += toAdd;
				addMutationBar(toAdd);
				gameManager.signalUpdateCellCount(cells);
				break;
		}
	}

	public void addMutationBar(float toAdd)
	{
		mutationBar = mutationBar + toAdd > maxMutationBar? maxMutationBar: mutationBar + toAdd;
	}

	public float getPlayerMaxHealth()
	{
		return playerData.maxHealth;
	}

	public float getPlayerSavedHealth()
	{
		return playerData.health;
	}

	public float getMutationBar()
	{
		return mutationBar;
	}

	public void savePlayerData(Actor player)
	{
		playerData = player.actorData;
	}

	public void useKeycard(int keycardIndex)
	{
		if (keycardIndex >= 0 && keycardIndex <= PickupDefs.MAX_KEYCARD_TYPE)
		{
			keycardCount[keycardIndex]--;
			gameManager.signalUpdateKeycardCount(keycardCount[keycardIndex], (PickupDefs.keycardType) keycardIndex);
		}
	}
}