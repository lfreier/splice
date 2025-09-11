using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static SceneDefs;

public class StationMenuScreen : StationScreen
{
	public TextMeshProUGUI saveText;
	public Button mutationButton;

	public override void init(StationMenuManager manager)
	{
		base.init(manager);
		if (gameManager.levelManager.stationShowMut)
		{
			mutationButton.gameObject.SetActive(true);
		}
		else
		{
			mutationButton.gameObject.SetActive(false);
		}
		setSaveText();

		foreach (TextMeshProUGUI text in this.GetComponentsInChildren<TextMeshProUGUI>())
		{
			if (text == null)
			{
				continue;
			}
			GameManager.updateCellFontSize(text, 1);
		}
	}

	public void onSaveButton()
	{
		if (gameManager == null)
		{
			return;
		}

		LevelManager levelManager = gameManager.levelManager;
		PlayerStats stats = gameManager.playerStats;

		if (stats.saveStationUses[(int)station.saveStationNumIndex] > 0)
		{
			if (station.isHubStation)
			{
				station.playerActor.takeHeal(station.playerActor.actorData.maxHealth);
			}
			levelManager.lastSavedSpawn = station.playerSpawnIndex;
			stats.saveStationUses[(int)station.saveStationNumIndex]--;
			gameManager.save(station.playerActor, gameManager.currentScene);
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

			menuManager.playClickSound();

			SceneManager.LoadSceneAsync(SCENE_INDEX_MASK[(int)SCENE.SAVING], LoadSceneMode.Additive);

			setSaveText();
		}
		else
		{
			gameManager.playSound(menuManager.stationClickPlayer, gameManager.audioManager.errorSound.name, 1F);
		}
	}

	public void onScanButton()
	{
		if (menuManager != null && menuManager.scanScreen != null)
		{
			menuManager.playClickSound();
			StartCoroutine(menuManager.waitForClick(playKeyboardSound));
			menuManager.changeScreen(menuManager.scanScreen);
		}
	}

	public void onDatabaseButton()
	{
		if (menuManager != null && menuManager.databaseScreen != null)
		{
			menuManager.playClickSound();
			menuManager.changeScreen(menuManager.databaseScreen);
		}
	}

	public void onMutationButton()
	{
		if (menuManager != null && menuManager.mutationScreen != null)
		{
			menuManager.playClickSound();
			menuManager.changeScreen(menuManager.mutationScreen);
		}
	}

	public override void onBackButton()
	{
		menuManager.playClickSound();
		StartCoroutine(menuManager.waitForClick(menuManager.exitMenu));
	}

	public void playKeyboardSound()
	{
		if (menuManager != null && menuManager.station != null && menuManager.station.useSaveStationSound != null)
		{
			gameManager.playSound(menuManager.stationClickPlayer, menuManager.station.useSaveStationSound.name, 1);
		}
	}

	private void setSaveText()
	{
		if (station.isHubStation)
		{
			saveText.text = "SAVE & HEAL (" + gameManager.playerStats.saveStationUses[(int)station.saveStationNumIndex] + ")";
		}
		else
		{
			saveText.text = "SAVE (" + gameManager.playerStats.saveStationUses[(int)station.saveStationNumIndex] + ")";
		}
	}
}