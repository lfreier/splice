using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static LevelManager;
using static PickupDefs;

[System.Serializable]
public class SaveData
{
	public int sceneIndex;

	public SerializeDictionary<string, ActorSaveData> actorTable;
	public SerializeDictionary<string, BasicSaveData> doorTable;
	public SerializeDictionary<string, WeaponSaveData> weaponTable;
	public SerializeDictionary<string, BasicSaveData> pickupBoxTable;
	public SerializeDictionary<string, BasicSaveData> saveTable;
	public SerializeDictionary<string, BasicSaveData> basicsTable;

	public int[] elevatorAvailable;

	public SaveData()
	{
		actorTable = new SerializeDictionary<string, ActorSaveData>();
		doorTable = new SerializeDictionary<string, BasicSaveData>();
		weaponTable = new SerializeDictionary<string, WeaponSaveData>();
		pickupBoxTable = new SerializeDictionary<string, BasicSaveData>();
		saveTable = new SerializeDictionary<string, BasicSaveData>();
		basicsTable = new SerializeDictionary<string, BasicSaveData>();

		elevatorAvailable = new int[NUM_ELEVATORS];
	}
}