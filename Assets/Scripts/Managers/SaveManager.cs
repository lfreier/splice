using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Profiling;

/* code in this file is based heavily on: https://github.com/shapedbyrainstudios/save-load-system/blob/5-bug-fixes-and-polish/Assets/Scripts/DataPersistence/FileDataHandler.cs */

public class SaveManager
{
	public static string dataPath = Application.dataPath;
	public static string dataFileName = "splice_level_save_";
	public static string playerDataFileName = "splice_player_save";
	public SaveData[] levelSaveData = new SaveData[SceneDefs.NUM_SCENES];

	private GameManager gameManager;
	private PrefabManager prefabManager;

	public SaveManager()
	{
		gameManager = GameManager.Instance;
		prefabManager = gameManager.prefabManager;

		for (int i = 0; i < levelSaveData.Length; i ++)
		{
			levelSaveData[i] = null;
		}
	}

	public void loadAllData()
	{
		for (int i = (int)SceneDefs.SCENE.LEVEL_START; i < SceneDefs.NUM_SCENES; i++)
		{
			levelSaveData[i] = loadDataFromDisk((SceneDefs.SCENE)i, gameManager.currentSaveSlot);
		}
	}

	public void loadPlayerSaveData(PlayerSaveData data)
	{
		if (data.mutationPrefabList != null)
		{
			foreach (int prefabIndex in data.mutationPrefabList)
			{
				if (prefabIndex >= 0)
				{
					GameObject prefab = null;
					switch ((mutationType)prefabIndex)
					{
						case mutationType.mBeast:
							prefab = prefabManager.mutPBeast;
							break;
						case mutationType.mLimb:
							prefab = prefabManager.mutPLimb;
							break;
						case mutationType.mWing:
							prefab = prefabManager.mutPBladeWing;
							break;
						case mutationType.mRaptor:
							prefab = prefabManager.mutPRaptor;
							break;
						case mutationType.mSpider:
							prefab = prefabManager.mutPSpore;
							break;
						case mutationType.mSpore:
							prefab = prefabManager.mutPSpore;
							break;
						default:
							break;
					}

					if (prefab == null)
					{
						continue;
					}
					MutationInterface mutInt = prefab.GetComponentInChildren<MutationInterface>();
					if (mutInt != null)
					{
						gameManager.playerStats.equipMutation(mutInt);
						gameManager.playerStats.showMutationBar();

						MLimb mLimb = gameManager.playerStats.player.mutationHolder.GetComponentInChildren<MLimb>();
						if (mLimb != null && data.limbGrabbedObjectPrefab >= 0 && data.limbGrabbedObjectPrefab < prefabManager.basicPrefabs.Length)
						{
							GameObject spawnedObject = GameObject.Instantiate(prefabManager.basicPrefabs[data.limbGrabbedObjectPrefab]);
							spawnedObject.SetActive(true);
							mLimb.grabber.addObjectToGrabber(spawnedObject.transform);
						}
					}
				}
			}
		}

		if (data.equippedWeaponPrefab >= 0 && data.equippedWeaponPrefab < prefabManager.weaponPrefabs.Length)
		{
			GameObject spawnedWeapon = GameObject.Instantiate(prefabManager.weaponPrefabs[data.equippedWeaponPrefab]);
			if (spawnedWeapon != null)
			{
				spawnedWeapon.SetActive(true);
				gameManager.playerStats.player.equip(spawnedWeapon);

				BasicWeapon weap = spawnedWeapon.GetComponentInChildren<BasicWeapon>();
				if (weap != null)
				{
					weap.reduceDurability(weap._weaponScriptable.durability - data.weaponDurability);
					SwingBatteryWeapon swing = spawnedWeapon.GetComponentInChildren<SwingBatteryWeapon>();
					if (swing != null)
					{
						swing.setBatteries(data.weaponCharge);
					}
				}
			}
		}
	}

	public SaveData loadDataFromDisk(SceneDefs.SCENE sceneIndex, int saveSlot)
	{
		if (!SceneDefs.isLevelScene(sceneIndex))
		{
			Debug.Log("Missing parameters when trying to load data from disk");
			return null;
		}
		//TODO: get data into levelSaveData

		SaveData loadSave = null;

		string fullPath = Path.Combine(dataPath, saveSlot.ToString(), dataFileName + "" + (int)sceneIndex);
		if (File.Exists(fullPath))
		{
			try
			{
				// load the serialized data from the file
				string dataToLoad = "";
				using (FileStream stream = new FileStream(fullPath, FileMode.Open))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						dataToLoad = reader.ReadToEnd();
					}
				}

				loadSave = JsonUtility.FromJson<SaveData>(dataToLoad);
			}
			catch (Exception e)
			{
				Debug.LogError("Error occured when trying to load file at path: " + fullPath + "\nException: \n" + e);
			}
		}
		return loadSave;
	}

	public PlayerSaveData loadPlayerDataFromDisk(int saveSlot)
	{
		if (dataPath == null || playerDataFileName == null)
		{
			return null;
		}

		PlayerSaveData loadSave = null;

		string fullPath = Path.Combine(dataPath, saveSlot.ToString(), playerDataFileName);
		if (File.Exists(fullPath))
		{
			try
			{
				// load the serialized data from the file
				string dataToLoad = "";
				using (FileStream stream = new FileStream(fullPath, FileMode.Open))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						dataToLoad = reader.ReadToEnd();
					}
				}

				loadSave = JsonUtility.FromJson<PlayerSaveData>(dataToLoad);
			}
			catch (Exception e)
			{
				Debug.LogError("Error occured when trying to load file at path: " + fullPath + "\nException: \n" + e);
			}
		}
		return loadSave;
	}

	public void saveDataToDisk(SaveData dataToSave, int saveSlot)
	{
		if (dataPath == null || dataFileName == null)
		{
			return;
		}

		string fullPath = Path.Combine(dataPath, saveSlot.ToString(), dataFileName + "" + dataToSave.sceneIndex);
		string backupFilePath = fullPath + ".bak";

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

			string dataString = JsonUtility.ToJson(dataToSave, true);

			using (FileStream stream = new FileStream(fullPath, FileMode.Create))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					writer.Write(dataString);
				}
			}

			// verify the newly saved file can be loaded successfully
			//GameData checkValid = Load(profileId);
			// if the data can be verified, back it up
			SaveData checkData = loadDataFromDisk((SceneDefs.SCENE)dataToSave.sceneIndex, saveSlot);
			if (null != checkData)
			{
				File.Copy(fullPath, backupFilePath, true);
				levelSaveData[dataToSave.sceneIndex] = checkData;
			}
			else
			{
				throw new Exception("Save file could not be verified and backup could not be created.");
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
		}
	}

	public void savePlayerDataToDisk(int saveSlot)
	{
		if (dataPath == null || playerDataFileName == null)
		{
			return;
		}

		gameManager.playerStats.copySavedPlayerData(gameManager.levelManager.currPlayerSaveData);

		string fullPath = Path.Combine(dataPath, saveSlot.ToString(), playerDataFileName);
		string backupFilePath = fullPath + ".bak";

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

			string dataString = JsonUtility.ToJson(gameManager.levelManager.currPlayerSaveData, true);

			using (FileStream stream = new FileStream(fullPath, FileMode.Create))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					writer.Write(dataString);
				}
			}

			// verify the newly saved file can be loaded successfully
			//GameData checkValid = Load(profileId);
			// if the data can be verified, back it up
			PlayerSaveData checkData = loadPlayerDataFromDisk(saveSlot);
			if (null != checkData)
			{
				File.Copy(fullPath, backupFilePath, true);
			}
			else
			{
				throw new Exception("Save file could not be verified and backup could not be created.");
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
		}
	}
}