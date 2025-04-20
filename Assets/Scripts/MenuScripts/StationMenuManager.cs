using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static SceneDefs;

public class StationMenuManager : MonoBehaviour
{
	public AudioListener audioListener;
	public EventSystem eventSystem;

	public StationMenuScreen currentMenu;

	[field: HideInInspector]
	public SaveStation station;

	private GameManager gameManager;

	// Use this for initialization
	void Start()
	{
		gameManager = GameManager.Instance;
		if (currentMenu == null)
		{
			currentMenu = GetComponentInChildren<StationMenuScreen>();
		}
		if (currentMenu != null)
		{
			currentMenu.menuManager = this;
		}

		foreach (SaveStation allStation in FindObjectsByType<SaveStation>(FindObjectsSortMode.None))
		{
			if (allStation.inUse)
			{
				station = allStation;
				break;
			}
		}

		currentMenu.init();
	}

	public void enterDatabaseMenu()
	{

	}

	public void enterScanMenu()
	{

	}

	public void enterSaveMenu()
	{

	}

	public async void exitMenu()
	{
		station.inUse = false;
		station.playerActor = null;

		if (audioListener != null)
		{
			audioListener.enabled = false;
		}
		if (eventSystem != null)
		{
			eventSystem.gameObject.SetActive(false);
		}

		Time.timeScale = 1;

		/* only disable the loading screen, don't destroy it */
		if (this.gameObject.scene.buildIndex != (int)SCENE.LOADING)
		{
			int[] temp = { SCENE_BUILD_MASK[this.gameObject.scene.buildIndex] };
			await gameManager.loadingHandler.forceUnloadScenes(temp);
		}
		gameManager.loadingHandler.reset();
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(SCENE_INDEX_MASK[gameManager.currentScene]));
	}
}