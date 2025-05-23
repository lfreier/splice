using System.Collections;
using UnityEngine;
using static LevelManager;
using static PickupDefs;

[System.Serializable]
public class PlayerSaveData
{
	public MutationDefs.MutationData savedMutationData;
	public ActorDefs.ActorData playerData;

	public int[] savedKeycardCount = new int[MAX_KEYCARD_TYPE + 1];
	public int[] savedUsableItemCount = new int[MAX_USABLE_ITEM_TYPE + 1];
	public Sprite[] savedUsableItemSprite = new Sprite[MAX_USABLE_ITEM_TYPE + 1];

	public int savedActiveItemIndex;

	public int[] mutationPrefabList;

	public int savedCells;

	public int equippedWeaponPrefab;

	public int limbGrabbedObjectPrefab;
	public int weaponCharge;
	public float weaponDurability;

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
	}
}