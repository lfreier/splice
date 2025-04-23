using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneDefs;

public class StationMenuScreen : StationScreen
{
	public TextMeshProUGUI saveText;

	public override void init(StationMenuManager manager)
	{
		base.init(manager);
		setSaveText();
	}

	public void onSaveButton()
	{
		if (gameManager == null)
		{
			return;
		}

		LevelManager levelManager = gameManager.levelManager;

		if (levelManager.saveStationUses[station.saveStationNumIndex] > 0)
		{
			levelManager.lastSavedSpawn = station.playerSpawnIndex;
			levelManager.saveStationUses[station.saveStationNumIndex]--;
			gameManager.save(station.playerActor);
			levelManager.lastSavedAtStation = true;

			for (int j = 0; j < SceneManager.sceneCount; j++)
			{
				Scene curr = SceneManager.GetSceneAt(j);
				if (curr.buildIndex == SCENE_INDEX_MASK[(int)SCENE.SAVING])
				{
					SceneManager.UnloadSceneAsync(curr.buildIndex);
					continue;
				}
			}

			SceneManager.LoadSceneAsync(SCENE_INDEX_MASK[(int)SCENE.SAVING], LoadSceneMode.Additive);

			setSaveText();
		}
	}

	public void onScanButton()
	{
		if (menuManager != null && menuManager.scanScreen != null)
		{
			menuManager.changeScreen(menuManager.scanScreen);
		}
	}

	public void onDatabaseButton()
	{
		if (menuManager != null && menuManager.databaseScreen != null)
		{
			menuManager.changeScreen(menuManager.databaseScreen);
		}
	}

	public void onMutationButton()
	{
		if (menuManager != null && menuManager.mutationScreen != null)
		{
			menuManager.changeScreen(menuManager.mutationScreen);
		}
	}

	public override void onExitButton()
	{
		menuManager.exitMenu();
	}

	private void setSaveText()
	{
		saveText.text = "SAVE (" + gameManager.levelManager.saveStationUses[station.saveStationNumIndex] + ")";
	}
}