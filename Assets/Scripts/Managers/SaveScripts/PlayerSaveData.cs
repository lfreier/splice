using System.Collections;
using UnityEngine;
using static LevelManager;
using static PickupDefs;

[System.Serializable]
public class PlayerSaveData
{
	public int lastSavedLevel = 0;
	public int lastSavedSpawn = 0;

	public MutationDefs.MutationData savedMutationData;
	public ActorDefs.ActorData playerData;

	public int[] savedKeycardCount = new int[MAX_KEYCARD_TYPE + 1];
	public int[] savedUsableItemCount = new int[MAX_USABLE_ITEM_TYPE + 1];
	public Sprite[] savedUsableItemSprite = new Sprite[MAX_USABLE_ITEM_TYPE + 1];

	public int savedActiveItemIndex;

	public int[] mutationPrefabList;

	public int savedCells;

	public int equippedWeaponPrefab;

	public bool[] weaponsScanned = new bool[WeaponDefs.MAX_PREFABS + 1];

	public int limbGrabbedObjectPrefab;
	public int weaponCharge;
	public float weaponDurability;

	public int[] elevatorAvailable;
	public int[] saveStationUses;

	public PlayerSaveData()
	{
		savedActiveItemIndex = -1;
		equippedWeaponPrefab = -1;
		limbGrabbedObjectPrefab = -1;

		playerData = new ActorDefs.ActorData();

		savedMutationData = new MutationDefs.MutationData();
		savedMutationData.maxMutationBar = 100;
		savedMutationData.mutationBar = 0;

		savedKeycardCount = new int[MAX_KEYCARD_TYPE + 1];
		savedUsableItemCount = new int[MAX_USABLE_ITEM_TYPE + 1];
		savedUsableItemSprite = new Sprite[MAX_USABLE_ITEM_TYPE + 1];
		weaponsScanned = new bool[WeaponDefs.MAX_PREFABS + 1];

		elevatorAvailable = new int[NUM_ELEVATORS];
		saveStationUses = new int[NUM_SAVE_STATIONS];
	}
}