using System.Collections;
using UnityEngine;
using static PickupDefs;

public class PlayerStats
{
	public int[] keycardCount			= new int[MAX_KEYCARD_TYPE + 1];
	public int[] usableItemCount		= new int[MAX_USABLE_ITEM_TYPE + 1];
	public Sprite[] usableItemSprite	= new Sprite[MAX_USABLE_ITEM_TYPE + 1];

	/* TODO: this is a weird way of doing this */
	//public UsableItem[] usableItems = new UsableItem[MAX_USABLE_TYPE + 1];
	//public UsableItem activeItem;
	public int activeItemIndex;

	public int cells = 0;
	private MutationDefs.MutationData mutationData;
	private ActorDefs.ActorData playerData;

	public PlayerHUD playerHUD;

	public Actor player;

	private GameManager gameManager = null;

	public PlayerStats()
	{
		mutationData = new MutationDefs.MutationData();
		mutationData.maxMutationBar = 100;
		mutationData.mutationBar = 0;
	}

	public void addItem(PickupInterface pickup)
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}

		bool resetIcon = false;
		if (playerHUD.activeItemIcon.enabled == false)
		{
			resetIcon = true;
		}

		switch (pickup.getPickupType())
		{
			case pickupType.BATTERY:
				usableItemCount[(int)usableType.BATTERY]++;
				if (usableItemSprite[(int)usableType.BATTERY] == null)
				{
					usableItemSprite[(int)usableType.BATTERY] = pickup.getIcon();
				}
				if (resetIcon == true)
				{
					activeItemIndex = (int)usableType.BATTERY;
				}
				break;
			case pickupType.HEALTH_VIAL:
				usableItemCount[(int)usableType.HEALTH_VIAL]++;
				if (usableItemSprite[(int)usableType.HEALTH_VIAL] == null)
				{
					usableItemSprite[(int)usableType.HEALTH_VIAL] = pickup.getIcon();
				}
				if (resetIcon == true)
				{
					activeItemIndex = (int)usableType.HEALTH_VIAL;
				}
				break;
			case pickupType.KEYCARD:
				/* Add the keycard to the player inventory by just incrementing */
				Keycard card = (Keycard)pickup;
				keycardCount[(int)card._keycardType]++;
				gameManager.signalUpdateKeycardCount(keycardCount[(int)card._keycardType], card._keycardType);
				break;
			case pickupType.CELL:
			default:
				int toAdd = pickup.getCount();
				cells += toAdd;
				addMutationBar(toAdd);
				gameManager.signalUpdateCellCount(cells);
				break;
		}

		if (resetIcon)
		{
			cycleActiveItem();
		}

		//TODO: add the item to a usable item slot if it doesn't exist yet
	}

	public void addMutationBar(int toAdd)
	{
		mutationData.mutationBar = mutationData.mutationBar + toAdd > mutationData.maxMutationBar ? mutationData.maxMutationBar : mutationData.mutationBar + toAdd;
	}

	public void cycleActiveItem()
	{
		int nextItemIndex = activeItemIndex + 1 >= usableItemCount.Length ? 0 : activeItemIndex + 1;
		while (usableItemCount[nextItemIndex] <= 0)
		{
			nextItemIndex = nextItemIndex + 1 >= usableItemCount.Length ? 0 : nextItemIndex + 1;
			if (nextItemIndex == activeItemIndex)
			{
				break;
			}
		}

		if (usableItemSprite[nextItemIndex] != null && usableItemCount[nextItemIndex] > 0)
		{
			playerHUD.changeActiveItemIcon(usableItemSprite[nextItemIndex]);
			activeItemIndex = nextItemIndex;
		}
	}

	public float getPlayerMaxHealth()
	{
		return playerData.maxHealth;
	}

	public float getPlayerSavedHealth()
	{
		return playerData.health;
	}

	public int getMaxMutationBar()
	{
		return mutationData.maxMutationBar;
	}

	public int getMutationBar()
	{
		return mutationData.mutationBar;
	}

	public void resetCounts()
	{
		for(int i = 0; i < MAX_KEYCARD_TYPE + 1; i ++)
		{
			keycardCount[i] = 0;
		}
		for (int i = 0; i < MAX_USABLE_ITEM_TYPE + 1; i++)
		{
			usableItemCount[i] = 0;
		}
	}

	public void savePlayerData(Actor player)
	{
		playerData = player.actorData;
	}

	public void useActiveItem()
	{
		if (usableItemCount[activeItemIndex] <= 0)
		{
			return;
		}

		switch ((usableType)activeItemIndex)
		{
			case usableType.HEALTH_VIAL:
				useHealthVial();
				break;
			case usableType.BATTERY:
				useBattery();
				break;
			default:
				break;
		}

		if (usableItemCount[activeItemIndex] == 0)
		{
			cycleActiveItem();
			if (usableItemCount[activeItemIndex] == 0)
			{
				playerHUD.activeItemIcon.enabled = false;
			}
		}
	}

	private void useBattery()
	{
		SwingBatteryWeapon weapon = player.getEquippedWeapon().GetComponentInChildren<SwingBatteryWeapon>();
		if (weapon != null && weapon.filledBatteries < weapon.maxBatteries)
		{
			/* TODO: so janky, not great */
			weapon.reduceDurability(-2);
			usableItemCount[activeItemIndex]--;
		}
	}

	private void useHealthVial()
	{
		if (player.actorData.health < player.actorData.maxHealth)
		{
			usableItemCount[activeItemIndex]--;
			player.takeHeal(1F);
		}
	}

	public void useKeycard(int keycardIndex)
	{
		if (keycardIndex >= 0 && keycardIndex <= MAX_KEYCARD_TYPE)
		{
			keycardCount[keycardIndex]--;
			gameManager.signalUpdateKeycardCount(keycardCount[keycardIndex], (PickupDefs.keycardType) keycardIndex);
		}
	}
}