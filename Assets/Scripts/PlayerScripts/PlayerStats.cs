using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using static PickupDefs;

/* Only exists in the game manager */
public class PlayerStats
{
	public int[] keycardCount			= new int[MAX_KEYCARD_TYPE + 1];
	public int[] usableItemCount		= new int[MAX_USABLE_ITEM_TYPE + 1];
	public Sprite[] usableItemSprite	= new Sprite[MAX_USABLE_ITEM_TYPE + 1];

	public int[] savedKeycardCount = new int[MAX_KEYCARD_TYPE + 1];
	public int[] savedUsableItemCount = new int[MAX_USABLE_ITEM_TYPE + 1];
	public Sprite[] savedUsableItemSprite = new Sprite[MAX_USABLE_ITEM_TYPE + 1];

	/* TODO: this is a weird way of doing this */
	//public UsableItem[] usableItems = new UsableItem[MAX_USABLE_TYPE + 1];
	//public UsableItem activeItem;
	public int activeItemIndex;
	public int savedActiveItemIndex;

	public int cells = 0;
	public int savedCells = 0;
	public int petriCellAmount = 0;
	private MutationDefs.MutationData mutationData;
	private MutationDefs.MutationData savedMutationData;

	private ActorDefs.ActorData playerData;

	private GameObject[] mutationList;
	public GameObject equippedWeaponObject;
	public BasicWeapon equippedWeapon;
	public GameObject limbGrabbedObject;
	public int weaponCharge;

	public PlayerHUD playerHUD;

	public Actor player;

	public GameManager gameManager = null;

	public PlayerStats()
	{
		mutationData = new MutationDefs.MutationData();
		mutationData.maxMutationBar = 100;
		mutationData.mutationBar = 0;
	}

	public bool addItem(PickupInterface pickup)
	{
		bool resetIcon = false;
		if (playerHUD.activeItemIcon != null && playerHUD.activeItemIcon.enabled == false)
		{
			resetIcon = true;
		}

		pickupType type = pickup.getPickupType();
		int usableIndex = PickupDefs.pickupToUsable(type);

		/* only increment usables if not at max */
		if (usableIndex >= 0)
		{
			if (usableIndex < usableItemCount.Length && usableItemCount[usableIndex] < gameManager.maxPickups[usableIndex])
			{
				usableItemCount[usableIndex]++;
			}
			else
			{
				/* at max count, don't pick up item */
				return false;
			}
		}

		switch (type)
		{
			case pickupType.PETRI_DISH:
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
				changeMutationBar(toAdd);
				break;
		}

		if (resetIcon)
		{
			cycleActiveItem();
		}

		return true;
		//TODO: add the item to a usable item slot if it doesn't exist yet
	}

	public void changeMutationBar(int change)
	{
		if (mutationData.mutationBar + change < 0)
		{
			mutationData.mutationBar = 0;
		}
		else if (mutationData.mutationBar + change > mutationData.maxMutationBar)
		{
			mutationData.mutationBar = mutationData.maxMutationBar;
		}
		else
		{
			mutationData.mutationBar = mutationData.mutationBar + change;
		}
		gameManager.signalUpdateCellCount(cells);
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
			MutationHandler mutHandle = player.mutationHolder.GetComponent<MutationHandler>();
			switch (mut.getId())
			{
				case "MBeast":
					mutPrefab = player.instantiateActive(gameManager.prefabManager.mutPBeast);
					break;
				case "MBladeWing":
					mutPrefab = player.instantiateActive(gameManager.prefabManager.mutPBladeWing);
					break;
				case "MLimb":
					mutPrefab = player.instantiateActive(gameManager.prefabManager.mutPLimb);
					break;
				case "MRaptor":
					mutPrefab = player.instantiateActive(gameManager.prefabManager.mutPRaptor);
					break;
				case "MSpider":
					mutPrefab = player.instantiateActive(gameManager.prefabManager.mutPSpider);
					break;
				case "MSpore":
					mutPrefab = player.instantiateActive(gameManager.prefabManager.mutPSpore);
					break;
				default:
					return;
			}

			MutationInterface newMut = (MutationInterface)mutPrefab.GetComponentInChildren(mut.GetType());
			if (null != (newMut = newMut.mEquip(player)))
			{
				/* this is basically all major mutations
				 *  'minor' mutations exist just for saving purposes */
				if (newMut.getMutationType() == mutationTrigger.ACTIVE_SLOT)
				{
					if (player.activeSlots[0] == null)
					{
						player.activeSlots[0] = newMut;
						mutHandle.majorMutation = newMut;
						showMutationBar();
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

	public void loadPlayerData(Actor playerActor)
	{
		player = playerActor;

		if (playerData.maxHealth != 0)
		{
			playerActor.actorData = ActorDefs.copyData(playerData);
			playerActor.actorData.maxSpeed = playerActor._actorScriptable.maxSpeed;
		}

		mutationData.maxMutationBar = savedMutationData.maxMutationBar;
		mutationData.mutationBar = savedMutationData.mutationBar;

		savedKeycardCount.CopyTo(keycardCount, 0);
		savedUsableItemCount.CopyTo(usableItemCount, 0);
		savedUsableItemSprite.CopyTo(usableItemSprite, 0);
		activeItemIndex = savedActiveItemIndex;
		if (usableItemCount[activeItemIndex] > 0)
		{
			playerHUD.activeItemIcon.enabled = true;
			playerHUD.changeActiveItemIcon(usableItemSprite[activeItemIndex]);
		}

		cells = savedCells;
		gameManager.signalUpdateCellCount(cells);

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
						showMutationBar();

						MLimb mLimb = playerActor.mutationHolder.GetComponentInChildren<MLimb>();
						if (mLimb != null && limbGrabbedObject != null)
						{
							GameObject spawnedObject = GameObject.Instantiate(limbGrabbedObject);
							spawnedObject.SetActive(true);
							mLimb.grabber.addObjectToGrabber(spawnedObject.transform);
						}
					}
				}
			}
		}

		if (equippedWeaponObject != null)
		{
			GameObject spawnedWeapon = GameObject.Instantiate(equippedWeaponObject);
			if (spawnedWeapon != null)
			{
				spawnedWeapon.SetActive(true);
				player.equip(spawnedWeapon);
			}
		}
	}

	public void savePlayerData(Actor playerActor)
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}

		playerData = playerActor.actorData;

		savedMutationData.maxMutationBar = mutationData.maxMutationBar;
		savedMutationData.mutationBar = mutationData.mutationBar;

		savedCells = cells;

		keycardCount.CopyTo(savedKeycardCount, 0);
		usableItemCount.CopyTo(savedUsableItemCount, 0);
		usableItemSprite.CopyTo(savedUsableItemSprite, 0);
		savedActiveItemIndex = activeItemIndex;

		int mutCount = playerActor.mutationHolder.transform.childCount;
		mutationList = new GameObject[mutCount];
		for (int i = 0; i < mutCount; i++)
		{
			mutationList[i] = GameObject.Instantiate(playerActor.mutationHolder.transform.GetChild(i).gameObject);
			mutationList[i].SetActive(false);
			mutationList[i].transform.SetParent(gameManager.gameObject.transform);
			MLimb limb = mutationList[i].GetComponentInChildren<MLimb>();
			if (limb != null)
			{
				if (limb.grabber != null && limb.grabber.heldRigidbody != null)
				{
					MGrabber grabber = limb.grabber;
					grabber.heldRigidbody.gameObject.transform.SetParent(null);
					if (limbGrabbedObject == null)
					{
						limbGrabbedObject = GameObject.Instantiate(grabber.heldRigidbody.gameObject);
						if (limbGrabbedObject != null)
						{
							limbGrabbedObject.SetActive(false);
							limbGrabbedObject.transform.SetParent(gameManager.gameObject.transform);
							GameObject.Destroy(grabber.heldRigidbody.gameObject);
						}
					}
				}
			}
		}

		if (!playerActor.isUnarmed())
		{
			equippedWeaponObject = GameObject.Instantiate(playerActor.getEquippedWeapon());
			if (equippedWeaponObject != null)
			{
				equippedWeaponObject.SetActive(false);
				equippedWeaponObject.transform.SetParent(gameManager.gameObject.transform);
				BasicWeapon weap = equippedWeaponObject.GetComponentInChildren<BasicWeapon>();
				if (weap != null)
				{
					equippedWeapon = weap;
				}
				SwingBatteryWeapon swing = equippedWeaponObject.GetComponentInChildren<SwingBatteryWeapon>();
				if (swing != null)
				{
					weaponCharge = swing.filledBatteries;
					swing.startingCharge = weaponCharge;
				}
			}
		}
		else
		{
			equippedWeaponObject = null;
		}
	}

	public void copySavedPlayerData(PlayerSaveData data)
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}

		data.playerData = ActorDefs.copyData(playerData);

		data.savedMutationData.maxMutationBar = savedMutationData.maxMutationBar;
		data.savedMutationData.mutationBar = savedMutationData.mutationBar;

		data.savedCells = savedCells;

		savedKeycardCount.CopyTo(data.savedKeycardCount, 0);
		savedUsableItemCount.CopyTo(data.savedUsableItemCount, 0);
		savedUsableItemSprite.CopyTo(data.savedUsableItemSprite, 0);
		data.savedActiveItemIndex = savedActiveItemIndex;

		data.mutationPrefabList = new int[mutationList.Length];
		int i = 0;
		foreach (GameObject mutObj in mutationList)
		{
			MutationInterface mut = mutObj.GetComponentInChildren<MutationInterface>();
			if (mut != null)
			{
				switch (mut.getId())
				{
					case "MBeast":
						data.mutationPrefabList[i] = (int)mutationType.mBeast;
						break;
					case "MLimb":
						data.mutationPrefabList[i] = (int)mutationType.mLimb;
						break;
					case "MBladeWing":
						data.mutationPrefabList[i] = (int)mutationType.mWing;
						break;
					case "MRaptor":
						data.mutationPrefabList[i] = (int)mutationType.mRaptor;
						break;
					case "MSpider":
						data.mutationPrefabList[i] = (int)mutationType.mSpider;
						break;
					case "MSpore":
						data.mutationPrefabList[i] = (int)mutationType.mSpore;
						break;
					default:
						return;
				}
			}
			i++;
		}

		if (limbGrabbedObject != null)
		{
			Obstacle obs = limbGrabbedObject.GetComponentInChildren<Obstacle>();
			if (obs != null)
			{
				data.limbGrabbedObjectPrefab = obs.basicPrefabIndex;
			}
		}
		if (equippedWeapon != null)
		{
			data.equippedWeaponPrefab = (int)equippedWeapon._weaponScriptable.prefabIndex;
			data.weaponCharge = weaponCharge;
			data.weaponDurability = equippedWeapon.durability;
		}
	}

	/* TODO: player stats/HUD are poorly connected with one another*/
	public void showMutationBar()
	{
		gameManager.levelManager.stationShowMut = true;
		playerHUD.mutationFillCanvas.enabled = true;
		playerHUD.mutationBGFillCanvas.enabled = true;
		playerHUD.mutationOutline.enabled = true;
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
		usableItemCount[activeItemIndex]--;
		cells += petriCellAmount;
		changeMutationBar(petriCellAmount);
	}
}