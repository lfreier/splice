using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Profiling;

/* code in this file is based heavily on: https://github.com/shapedbyrainstudios/save-load-system/blob/5-bug-fixes-and-polish/Assets/Scripts/DataPersistence/FileDataHandler.cs */

public class SaveManager
{
	public struct OptionsSaveData
	{
		public float musicVolume;
		public float effectsVolume;
	}

	public static string dataPath = Application.dataPath;
	public static string dataFileName = "splice_level_save_";
	public static string playerDataFileName = "splice_player_save";
	public static string optionsDataFileName = "splice_options";
	public static int TOTAL_SAVES = 3;
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

	public static void deleteSaveSlot(int saveSlot)
	{
		string folderPath = Path.Combine(dataPath, saveSlot.ToString());
		if (Directory.Exists(folderPath))
		{
			Directory.Delete(folderPath, true);
		}
	}

	public void loadAllData()
	{
		for (int i = (int)SceneDefs.SCENE.LEVEL_START; i < SceneDefs.NUM_SCENES; i++)
		{
			levelSaveData[i] = loadDataFromDisk((SceneDefs.SCENE)i, gameManager.currentSaveSlot);
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

	public static OptionsSaveData loadOptionsDataFromDisk()
	{
		OptionsSaveData loadSave;
		loadSave.musicVolume = -1;
		loadSave.effectsVolume = -1;

		if (dataPath == null || optionsDataFileName == null)
		{
			return loadSave;
		}

		string fullPath = Path.Combine(dataPath, optionsDataFileName + "");
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

				loadSave = JsonUtility.FromJson<OptionsSaveData>(dataToLoad);
			}
			catch (Exception e)
			{
				Debug.LogError("Error occured when trying to load file at path: " + fullPath + "\nException: \n" + e);
			}
		}
		return loadSave;
	}

	public static PlayerSaveData loadPlayerDataFromDisk(int saveSlot)
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

	public static void saveOptionsDataToDisk(OptionsSaveData dataToSave)
	{
		if (dataPath == null || optionsDataFileName == null)
		{
			return;
		}

		string fullPath = Path.Combine(dataPath, optionsDataFileName + "");
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
			OptionsSaveData checkData = loadOptionsDataFromDisk();
			if (checkData.musicVolume >= 0 && checkData.effectsVolume >= 0)
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

	public void savePlayerDataToDisk(int saveSlot)
	{
		if (dataPath == null || playerDataFileName == null)
		{
			return;
		}

		gameManager.playerStats.copySavedPlayerData(gameManager.levelManager.currPlayerSaveData);
		gameManager.levelManager.currPlayerSaveData.lastSavedLevel = gameManager.levelManager.lastSavedLevelIndex;
		gameManager.levelManager.currPlayerSaveData.lastSavedSpawn = (int)gameManager.levelManager.lastSavedSpawn;

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