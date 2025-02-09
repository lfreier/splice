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
		PICKUP = 4,
		BOX = 5,
		SAVE = 6
	}

	public struct BasicSaveData
	{
		public float xPosition;
		public float yPosition;
		public float rotation;
		public bool isPresent;
		public int option;
	};

	public struct ActorSaveData
	{
		public ActorData actorData;
		public BasicSaveData basicData;
		public int detectionState;
		public int idlePathIndex;
	};

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

	public GameObject playerPrefab;

	public int lastSavedSpawn;
	public int lastSavedLevelIndex;

	public PlayerSpawnScriptable[] levelSpawns;

	public bool lastSavedAtStation = false;

	public GameObject[] savedActors;

	public static int NUM_SAVE_STATIONS = 2;
	public bool[] usedSavedStations = new bool[NUM_SAVE_STATIONS];

	public Dictionary<string, GUIDWatcher>	guidTable = new Dictionary<string, GUIDWatcher>();
	Dictionary<string, ActorSaveData>		actorTable = new Dictionary<string, ActorSaveData>();
	Dictionary<string, BasicSaveData>		doorTable = new Dictionary<string, BasicSaveData>();
	Dictionary<string, WeaponSaveData>		weaponTable = new Dictionary<string, WeaponSaveData>();
	Dictionary<string, WeaponSaveData>		weaponSpawnTable = new Dictionary<string, WeaponSaveData>();
	Dictionary<string, BasicSaveData>		pickupBoxTable = new Dictionary<string, BasicSaveData>();
	Dictionary<string, BasicSaveData>		pickupTable = new Dictionary<string, BasicSaveData>();
	Dictionary<string, BasicSaveData>		saveTable = new Dictionary<string, BasicSaveData>();

	public enum levelSpawnIndex
	{
		levelStartSpawn = 0,
		levelStartSaveSpawn = 1,
		levelOfficeSpawn = 2,
		levelOfficeSaveSpawn = 3
	}

	public CameraHandler camHandler;

	private GameManager gameManager;

	public async Task startNewLevel(int spawnIndex)
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

			if (lastSavedAtStation)
			{
				await loadLevelState(lastSavedLevelIndex);
			}

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

	public async Task loadLevelState(int sceneIndex)
	{
		weaponSpawnTable = new Dictionary<string, WeaponSaveData>(weaponTable);

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
					if (actorTable.TryGetValue(item.Key, out actorSave))
					{
						if (actor != null)
						{
							if (!actorSave.basicData.isPresent)
							{
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
							}
						}
					}
					else
					{
						Destroy(actor.gameObject);
					}
					continue;
				case idType.DOOR:
					AutoDoor door = item.Value.guid.GetComponent<AutoDoor>();
					if (door != null)
					{
						if (doorTable.TryGetValue(item.Key, out dataSave))
						{
							if (dataSave.isPresent && door.locked)
							{
								door.doorUnlock();
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
						if (weaponTable.TryGetValue(item.Key, out weapSave))
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
				case idType.PICKUP:
					PickupInterface pickup = item.Value.guid.GetComponent<PickupInterface>();
					GameObject pickupObj = item.Value.guid.gameObject;
					if (pickup != null)
					{
						if (pickupTable.TryGetValue(item.Key, out dataSave))
						{
							if (!dataSave.isPresent && pickupObj != null)
							{
								Destroy(pickupObj);
							}
						}
						else if (pickupObj != null)
						{
							Destroy(pickupObj.gameObject);
						}
					}
					continue;
				case idType.BOX:
					PickupBox box = item.Value.guid.GetComponentInChildren<PickupBox>();
					if (box != null)
					{
						if (pickupBoxTable.TryGetValue(item.Key, out dataSave))
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
						if (saveTable.TryGetValue(item.Key, out dataSave))
						{
							usedSavedStations[dataSave.option] = dataSave.isPresent;
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
			GameObject newWeap = Instantiate(gameManager.weaponPrefabs[weap.Value.prefabIndex]);
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
	}

	public async Task saveLevelState(int sceneIndex)
	{
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
				}

				actorTable.Add(item.Key, actorSave);
				continue;
			}
			AutoDoor door = guid.GetComponent<AutoDoor>();
			if (door != null)
			{
				dataSave.isPresent = !door.locked;
				doorTable.Add(item.Key, dataSave);
				continue;
			}
			PickupBox box = guid.GetComponentInChildren<PickupBox>();
			if (box != null)
			{
				dataSave.isPresent = box.hasPickup();
				pickupBoxTable.Add(item.Key, dataSave);
				continue;
			}
			PickupInterface pickup = guid.GetComponent<PickupInterface>();
			if (pickup != null)
			{
				dataSave.isPresent = true;
				pickupTable.Add(item.Key, dataSave);
				continue;
			}
			BasicWeapon weapon = guid.GetComponentInChildren<BasicWeapon>();
			if (weapon != null)
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

				weapSave.basicData.xPosition = parent.position.x;
				weapSave.basicData.yPosition = parent.position.y;
				weapSave.basicData.rotation = weapon._weaponPhysics.weaponBody.rotation;
				weaponTable.Add(item.Key, weapSave);
				continue;
			}
			SaveStation save = guid.GetComponentInChildren<SaveStation>();
			if (save != null)
			{
				dataSave.isPresent = usedSavedStations[save.saveStationNumIndex];
				dataSave.option = save.saveStationNumIndex;
				saveTable.Add(item.Key, dataSave);
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

		parent.transform.position = new Vector2(weapSave.basicData.xPosition, weapSave.basicData.yPosition);
		weapon._weaponPhysics.weaponBody.rotation = weapSave.basicData.rotation;

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