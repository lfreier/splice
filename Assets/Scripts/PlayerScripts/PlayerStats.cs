using System.Collections;
using UnityEngine;
using static PickupDefs;

/* Only exists in the game manager */
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
	public int petriCellAmount = 0;
	private MutationDefs.MutationData mutationData;
	private ActorDefs.ActorData playerData;
	private GameObject[] mutationList;

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
			case pickupType.PETRI_DISH:
				usableItemCount[(int)usableType.REFILL]++;
				if (usableItemSprite[(int)usableType.REFILL] == null)
				{
					usableItemSprite[(int)usableType.REFILL] = pickup.getIcon();
				}
				if (resetIcon == true)
				{
					activeItemIndex = (int)usableType.REFILL;
				}
				if (petriCellAmount <= 0)
				{
					petriCellAmount = pickup.getCount();
				}
				break;
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

	public void equipMutation(MutationInterface mut)
	{
		if (mut != null)
		{
			var existingMuts = player.mutationHolder.GetComponents(mut.GetType());
			if (existingMuts.Length > 0)
			{
				/* the mutation already exists - return */
				return;
			}

			GameObject mutPrefab;
			switch (mut.getId())
			{
				case "MBeast":
					mutPrefab = player.instantiateActive(player.gameManager.mutPBeast);
					break;
				case "MBladeWing":
					mutPrefab = player.instantiateActive(player.gameManager.mutPBladeWing);
					break;
				case "MLimb":
					mutPrefab = player.instantiateActive(player.gameManager.mutPLimb);
					break;
				default:
					return;
			}

			MutationInterface newMut = (MutationInterface)mutPrefab.GetComponentInChildren(mut.GetType());
			if (null != (newMut = newMut.mEquip(player)))
			{
				if (newMut.getMutationType() == mutationTrigger.ACTIVE_SLOT)
				{
					if (player.activeSlots[0] == null)
					{
						player.activeSlots[0] = newMut;
						return;
					}
				}
			}
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

	public void loadPlayerData(Actor player)
	{
		if (playerData.maxHealth != 0)
		{
			player.actorData = playerData;
		}

		if (mutationList != null)
		{
			foreach (GameObject mut in mutationList)
			{
				if (mut != null)
				{
					MutationInterface mutInt = mut.GetComponentInChildren<MutationInterface>();
					if (mutInt != null)
					{
						equipMutation(mutInt);
						GameObject.Destroy(mut);
					}
				}
			}
		}
	}

	public void savePlayerData(Actor player)
	{
		playerData = player.actorData;

		int mutCount = player.mutationHolder.transform.childCount;
		mutationList = new GameObject[mutCount];
		for (int i = 0; i < mutCount; i++)
		{
			mutationList[i] = GameObject.Instantiate(player.mutationHolder.transform.GetChild(i).gameObject);
			mutationList[i].SetActive(false);
			mutationList[i].transform.SetParent(gameManager.gameObject.transform);
		}
	}

	public void useActiveItem()
	{
		if (usableItemCount[activeItemIndex] <= 0)
		{
			return;
		}

		switch ((usableType)activeItemIndex)
		{
			case usableType.REFILL:
				usePetriDish();
				break;
			case usableType.BATTERY:
				useBattery();
				break;
			case usableType.HEALTH_VIAL:
				useHealthVial();
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

	private void usePetriDish()
	{
		if (getMutationBar() < getMaxMutationBar())
		{
			usableItemCount[activeItemIndex]--;
			cells += petriCellAmount;
			addMutationBar(petriCellAmount);
			gameManager.signalUpdateCellCount(cells);
		}
	}
}