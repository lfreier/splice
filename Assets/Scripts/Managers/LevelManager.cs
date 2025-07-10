using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;
using System.Collections.Generic;
using static ActorDefs;
using Unity.VisualScripting;

public class LevelManager : MonoBehaviour
{
	public enum idType
	{
		NONE = 0,
		ACTOR = 1,
		DOOR = 2,
		WEAPON = 3,
		BASIC = 4,
		BOX = 5,
		SAVE = 6
	}

	[System.Serializable]
	public struct BasicSaveData
	{
		public float xPosition;
		public float yPosition;
		public float rotation;
		public bool isPresent;
		public int option;
		public int prefabIndex;
	};

	[System.Serializable]
	public struct ActorSaveData
	{
		public ActorData actorData;
		public BasicSaveData basicData;
		public int detectionState;
		public int idlePathIndex;
	};

	[System.Serializable]
	public struct WeaponSaveData
	{
		public BasicSaveData basicData;
		public float durability;
		public string actorGUID;
		public int prefabIndex;
	};

	private bool powerOn;
	public Vector3 gridPosition;
	public LevelData currLevelData;

	public bool stationShowMut = false;

	public GameObject playerPrefab;

	public levelSpawnIndex lastSavedSpawn;
	public int lastSavedLevelIndex;

	public PlayerSpawnScriptable[] levelSpawns;

	public bool lastSavedAtStation = false;

	public GameObject[] savedActors;

	public static int NUM_SAVE_STATIONS = 5;
	public enum saveStationIndex
	{
		hub = 0,
		start = 1,
		office = 2,
		warehouse = 3,
		arch = 4,
		rnd = 5,
		iso = 6
	}

	public int[] saveStationUses = new int[NUM_SAVE_STATIONS];

	public PlayerSpawnScriptable[] elevatorSpawns;
	public enum elevatorIndex
	{
		nul = 0,
		hub = 1,
		warehouse = 2,
		warehouseExit = 3,
		rnd = 4,
		rndExit = 5,
		start = 6,
		iso = 7,
		isoExit = 8,
		office = 9,
		officeExit = 10,
		TOTAL = 11
	}
	public static int NUM_ELEVATORS = (int)elevatorIndex.TOTAL;
	public int[] elevatorAvailable = new int[NUM_ELEVATORS];

	public Dictionary<string, GUIDWatcher>	guidTable = new Dictionary<string, GUIDWatcher>();
	public Dictionary<string, BasicSaveData> basicSpawnTable = new Dictionary<string, BasicSaveData>();
	public Dictionary<string, WeaponSaveData> weaponSpawnTable = new Dictionary<string, WeaponSaveData>();

	public SaveData currSaveData;
	public PlayerSaveData currPlayerSaveData;

	public enum levelSpawnIndex
	{
		levelStartSpawn = 0,
		levelStartSaveSpawn = 1,
		levelStartExitSpawn = 2,
		levelOfficeSpawn = 3,
		levelOfficeSaveSpawn = 4,
		levelOfficeExitSpawn = 5,
		levelHubSpawn = 6,
		levelHubSaveSpawn = 7,
		levelWarehouseSpawn = 8,
		levelWarehouseSaveSpawn = 9,
		levelWarehouseExitSpawn = 10,
		levelArchSpawn = 11,
		levelArchSaveSpawn = 12,
		levelRndSpawn = 13,
		levelRndSaveSpawn = 14,
		levelRndExitSpawn = 15,
		levelIsoSpawn = 16,
		levelIsoSaveSpawn = 17,
		levelIsoExitSpawn = 18
	}

	public CameraHandler camHandler;

	private GameManager gameManager;

	public async Task startNewLevel(int spawnIndex, int levelIndex)
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}
		powerOn = true;
		//int i = 0;
		foreach (LevelData levelData in FindObjectsByType<LevelData>(FindObjectsSortMode.None))
		{
			camHandler = levelData.gameObject.GetComponent<CameraHandler>();
			gameManager.signalStartMusicEvent(levelData.sceneMusic);
			if (gameManager.currentScene < 0)
			{
				gameManager.currentScene = levelData.levelSceneIndex;
			}

			Actor playerActor = null;
			/* spawn player if one isn't there */
			if (spawnIndex >= 0)
			{
				Camera.main.transform.SetPositionAndRotation(new Vector3(levelSpawns[spawnIndex].spawnPosition.x, levelSpawns[spawnIndex].spawnPosition.y, -10), Quaternion.identity);
				foreach (Actor actor in FindObjectsByType<Actor>(FindObjectsSortMode.None))
				{
					if (actor.tag == ActorDefs.playerTag)
					{
						Destroy(actor.gameObject);
					}
				}
				GameObject tempPlayer = Instantiate(playerPrefab);
				tempPlayer.transform.SetPositionAndRotation(levelSpawns[spawnIndex].spawnPosition, Quaternion.identity);
				tempPlayer.transform.Rotate(new Vector3(0, 0, levelSpawns[spawnIndex].spawnRotation));
				tempPlayer.SetActive(true);

				camHandler.player = tempPlayer;

				playerActor = tempPlayer.GetComponent<Actor>();
				if (playerActor != null)
				{
					while (!playerActor.initialized)
					{
						await Task.Delay(10);
					}
					gameManager.playerStats.player = playerActor;
					gameManager.playerStats.loadPlayerData(playerActor);
				}
			}

			/* don't load the level state if it's a new level */
			if (SceneDefs.isLevelScene((SceneDefs.SCENE)levelIndex) && (lastSavedAtStation || gameManager.saveManager.levelSaveData[levelIndex] != null))
			{
				await loadLevelState(gameManager.saveManager.levelSaveData[levelIndex]);
			}

			Time.timeScale = 1;
			return;
			/*
			gridPosition = currLevelData.transform.position;
			if (i > 0)
			{
				Debug.Log("Internal error: multiple PathGrid objects in a level");
				break;
			}
			i++;
			*/
		}
	}

	public async Task loadLevelState(SaveData saveData)
	{
		weaponSpawnTable = new Dictionary<string, WeaponSaveData>(saveData.weaponTable);
		basicSpawnTable = new Dictionary<string, BasicSaveData>(saveData.basicsTable);

		//for each GUID
		//if it has a component that matches a table,
		//set its gameobject and component values
		foreach (var item in guidTable)
		{
			BasicSaveData dataSave = new();
			switch (item.Value.idType)
			{
				case idType.ACTOR:
					ActorSaveData actorSave = new();
					Actor actor = item.Value.guid.GetComponent<Actor>();
					if (saveData.actorTable.TryGetValue(item.Key, out actorSave))
					{
						if (actor != null)
						{
							if (!actorSave.basicData.isPresent)
							{
								BasicWeapon actorWeap = actor.GetComponentInChildren<BasicWeapon>();
								if (actorWeap != null)
								{
									actorWeap.transform.parent.SetParent(null, true);
									WeaponDefs.setWeaponTag(actorWeap.transform.parent.gameObject, WeaponDefs.OBJECT_WEAPON_TAG);
									WeaponDefs.setObjectLayer(WeaponDefs.SORT_LAYER_GROUND, actorWeap.transform.parent.gameObject);
								}
								Destroy(actor.gameObject);
								continue;
							}

							actor.actorData = ActorDefs.copyData(actorSave.actorData);
							actor.transform.position = new Vector2(actorSave.basicData.xPosition, actorSave.basicData.yPosition);
							actor.actorBody.rotation = actorSave.basicData.rotation;

							EnemyMove ai = actor.GetComponent<EnemyMove>();
							if (ai != null)
							{
								ai.pathIndex = actorSave.idlePathIndex;
								ai._detection = (detectMode)actorSave.detectionState;
							}
						}
					}
					else
					{
						BasicWeapon actorWeap = actor.GetComponentInChildren<BasicWeapon>();
						if (actorWeap != null)
						{
							actorWeap.transform.parent.SetParent(null, true);
							WeaponDefs.setWeaponTag(actorWeap.transform.parent.gameObject, WeaponDefs.OBJECT_WEAPON_TAG);
							WeaponDefs.setObjectLayer(WeaponDefs.SORT_LAYER_GROUND, actorWeap.transform.parent.gameObject);
						}
						Destroy(actor.gameObject);
					}
					continue;
				case idType.DOOR:
					AutoDoor door = item.Value.guid.GetComponent<AutoDoor>();
					if (door != null)
					{
						if (saveData.doorTable.TryGetValue(item.Key, out dataSave))
						{
							if (dataSave.isPresent && door.locked)
							{
								door.doorUnlockQuiet();
							}
						}
						else
						{
							Destroy(door.gameObject);
						}
					}
					continue;
				case idType.WEAPON:
					//be careful not to get equipped weapons?
					BasicWeapon weapon = item.Value.guid.GetComponentInChildren<BasicWeapon>();
					if (weapon != null)
					{
						WeaponSaveData weapSave;
						if (saveData.weaponTable.TryGetValue(item.Key, out weapSave))
						{
							spawnWeapon(weapon, weapSave);
							/* necessary for cabinet pickups */
							weaponSpawnTable.Remove(item.Key);
						}
						else
						{
							//hacky but whatever
							weaponSpawnTable.Remove(item.Key);
							Destroy(weapon.transform.parent.gameObject);
						}
					}
					continue;
				case idType.BOX:
					PickupBox box = item.Value.guid.GetComponentInChildren<PickupBox>();
					if (box != null)
					{
						if (saveData.pickupBoxTable.TryGetValue(item.Key, out dataSave))
						{
							if (!dataSave.isPresent && box.hasPickup())
							{
								box.clearPickup();
							}
						}
						else
						{
							Destroy(box.gameObject);
						}
					}
					continue;
				case idType.SAVE:
					SaveStation save = item.Value.guid.GetComponentInChildren<SaveStation>();
					if (save != null)
					{
						if (saveData.saveTable.TryGetValue(item.Key, out dataSave))
						{
							saveStationUses[dataSave.option] = dataSave.isPresent ? 1 : 0;
						}
					}
					continue;
				case idType.BASIC:
					TutorialSceneLoader tut = item.Value.guid.GetComponent<TutorialSceneLoader>();
					Corpse corpse = item.Value.guid.GetComponentInChildren<Corpse>();
					PickupInterface pickup = item.Value.guid.GetComponent<PickupInterface>();
					GameObject basicObj = item.Value.guid.gameObject;
					if (tut != null || corpse != null || pickup != null)
					{
						if (saveData.basicsTable.TryGetValue(item.Key, out dataSave))
						{
							if (!dataSave.isPresent && basicObj != null)
							{
								Destroy(basicObj);
							}
							basicObj.transform.SetPositionAndRotation(new Vector2(dataSave.xPosition, dataSave.yPosition), Quaternion.Euler(0, 0, dataSave.rotation));
							basicSpawnTable.Remove(item.Key);
						}
						else if (basicObj != null)
						{
							Destroy(basicObj);
						}
					}
					continue;
				default:
					//not in any table, this shouldn't happen
					continue;
			}
		}

		foreach (var weap in weaponSpawnTable)
		{
			/* need to spawn these weapons */
			GameObject newWeap = Instantiate(gameManager.prefabManager.weaponPrefabs[weap.Value.prefabIndex]);
			BasicWeapon weapon = newWeap.GetComponentInChildren<BasicWeapon>();

			while (true)
			{
				while (weapon == null)
				{
					await Task.Delay(10);
					weapon = newWeap.GetComponentInChildren<BasicWeapon>();
				}
				if (weapon.isInit)
				{
					break;
				}
				await Task.Delay(10);
			}

			if (weapon != null)
			{
				spawnWeapon(weapon, weap.Value);
			}
			else
			{
				//hacky but whatever
				weaponSpawnTable.Remove(weap.Key);
				Destroy(weapon.transform.parent.gameObject);
			}
		}

		foreach (var basic in basicSpawnTable)
		{
			/* need to spawn these objects */
			GameObject newBasic = Instantiate(gameManager.prefabManager.basicPrefabs[basic.Value.prefabIndex]);

			newBasic.transform.SetPositionAndRotation(new Vector2(basic.Value.xPosition, basic.Value.yPosition), Quaternion.Euler(0, 0, basic.Value.rotation));
		}
	}

	public void saveLevelState(int sceneIndex)
	{
		currSaveData = new SaveData();
		currSaveData.sceneIndex = sceneIndex;
		elevatorAvailable.CopyTo(currSaveData.elevatorAvailable, 0);

		//clear all component tables
		//for each GUID
		//if it has a component that matches a table,
		//save its component values
		//save gameObject values if its a weapon
		foreach (var item in guidTable)
		{
			BasicSaveData dataSave = new();
			GUID guid = item.Value.guid;

			if (guid == null)
			{
				/* no need to add to any table, will just be destroyed */
				continue;
			}

			/* save data for changed objects */
			Actor actor = guid.GetComponent<Actor>();
			if (actor != null)
			{
				ActorSaveData actorSave = new();
				actorSave.actorData = ActorDefs.copyData(actor.actorData);
				actorSave.actorData.maxSpeed = actor._actorScriptable.maxSpeed;

				actorSave.basicData.xPosition = actor.transform.position.x;
				actorSave.basicData.yPosition = actor.transform.position.y;
				actorSave.basicData.rotation = actor.actorBody.rotation;
				actorSave.basicData.isPresent = true;

				EnemyMove ai = actor.GetComponent<EnemyMove>();
				if (ai != null)
				{
					actorSave.idlePathIndex = ai.pathIndex;
					actorSave.detectionState = (int)ai._detection;
				}

				currSaveData.actorTable.Add(item.Key, actorSave);
				continue;
			}
			AutoDoor door = guid.GetComponent<AutoDoor>();
			if (door != null)
			{
				dataSave.isPresent = !door.locked;
				currSaveData.doorTable.Add(item.Key, dataSave);
				continue;
			}
			PickupBox box = guid.GetComponentInChildren<PickupBox>();
			if (box != null)
			{
				dataSave.isPresent = box.hasPickup();
				currSaveData.pickupBoxTable.Add(item.Key, dataSave);
				continue;
			}
			BasicWeapon weapon = guid.GetComponentInChildren<BasicWeapon>();
			if (weapon != null && weapon.getType() != WeaponType.UNARMED)
			{
				WeaponSaveData weapSave = new();
				/* equip weapon to actors */
				if (weapon.gameObject.tag == WeaponDefs.EQUIPPED_WEAPON_TAG && weapon.actorWielder != null)
				{
					if (weapon.actorWielder.tag == playerTag)
					{
						/* DON'T add player weapon to table since we need to always spawn a new one */
						continue;
					}
					else
					{
						GUID actorGuid = weapon.actorWielder.GetComponent<GUID>();
						if (null != actorGuid)
						{
							weapSave.actorGUID = System.String.Copy(actorGuid.guid);
						}
					}
				}
				weapSave.prefabIndex = (int)weapon._weaponScriptable.prefabIndex;
				weapSave.durability = weapon.durability;
				weapSave.basicData.isPresent = true;

				Transform parent = weapon.gameObject.transform.parent;

				/* subtract the data object's position
				 * because if the weapon is on the left side (data obj has a non-zero position)
				 * it will always load on the right */
				weapSave.basicData.xPosition = parent.position.x - weapon.transform.localPosition.x;
				weapSave.basicData.yPosition = parent.position.y - weapon.transform.localPosition.y;
				weapSave.basicData.rotation = weapon._weaponPhysics.weaponBody.rotation;

				SwingBatteryWeapon swing = weapon.gameObject.GetComponentInChildren<SwingBatteryWeapon>();
				if (swing != null)
				{
					weapSave.basicData.option = swing.filledBatteries;
				}

				currSaveData.weaponTable.Add(item.Key, weapSave);
				continue;
			}
			SaveStation save = guid.GetComponentInChildren<SaveStation>();
			if (save != null)
			{
				dataSave.isPresent = saveStationUses[(int)save.saveStationNumIndex] == 0 ? false : true;
				dataSave.option = (int)save.saveStationNumIndex;
				currSaveData.saveTable.Add(item.Key, dataSave);
				continue;
			}
			TutorialSceneLoader tut = guid.GetComponent<TutorialSceneLoader>();
			Corpse corpse = guid.GetComponent<Corpse>();
			PickupInterface pickup = guid.GetComponent<PickupInterface>();
			if (tut != null || corpse != null || pickup != null)
			{
				dataSave.isPresent = true;
				dataSave.xPosition = guid.transform.position.x;
				dataSave.yPosition = guid.transform.position.y;
				dataSave.rotation = guid.transform.rotation.eulerAngles.z;
				dataSave.prefabIndex = guid.basicPrefabIndex;
				currSaveData.basicsTable.Add(item.Key, dataSave);
				continue;
			}
		}
	}

	private void spawnWeapon(BasicWeapon weapon, WeaponSaveData weapSave)
	{
		Transform parent = weapon.gameObject.transform.parent;
		if (!weapSave.basicData.isPresent)
		{
			Destroy(parent.gameObject);
			return;
		}

		weapon.reduceDurability(weapon._weaponScriptable.durability - weapSave.durability);

		parent.transform.SetPositionAndRotation(new Vector2(weapSave.basicData.xPosition, weapSave.basicData.yPosition), Quaternion.Euler(0, 0, weapSave.basicData.rotation));

		/* equip to specified actor */
		if (!System.String.IsNullOrEmpty(weapSave.actorGUID))
		{
			GUIDWatcher watcher;
			guidTable.TryGetValue(weapSave.actorGUID, out watcher);
			if (watcher != null)
			{
				Actor equipActor = watcher.guid.GetComponent<Actor>();
				if (equipActor != null)
				{
					equipActor.equip(parent.gameObject);
				}
			}
		}
	}

	/* TODO: this definitely doesn't work */
	public static void dropItem(GameObject toDrop, Vector2 target)
	{
		Rigidbody2D body = toDrop.GetComponentInChildren<Rigidbody2D>();
		if (body != null)
		{
			Vector2 newTarget = new Vector2(target.x + Random.Range(-1F, 1F), target.y + Random.Range(-1F, 1F));
			body.transform.Translate((newTarget - body.position) * 500F);
			body.transform.RotateAround(body.transform.position, Vector3.forward, Random.Range(-100F, 100F));
		}
	}

	public void equipPlayerWeapon(Actor player, string weaponGUID)
	{
		GUIDWatcher watcher;
		if (guidTable.TryGetValue(weaponGUID, out watcher))
		{
			player.equip(watcher.guid.gameObject);
			weaponSpawnTable.Remove(weaponGUID);
		}
	}

	public bool hasPower()
	{
		return powerOn;
	}

	public void setPower(bool powerToSet)
	{
		powerOn = powerToSet;
	}
}