using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static SceneDefs;

public class StationMenuManager : MonoBehaviour
{
	public AudioListener audioListener;
	public EventSystem eventSystem;

	[field: HideInInspector]
	public StationScreen currentScreen;

	public GameObject menuScreen;
	public GameObject scanScreen;
	public GameObject databaseScreen;
	public GameObject mutationScreen;

	[field: HideInInspector]
	public SaveStation station;

	private GameManager gameManager;

	void Start()
	{
		gameManager = GameManager.Instance;
		if (currentScreen == null)
		{
			currentScreen = GetComponentInChildren<StationScreen>();
		}
		if (currentScreen != null)
		{
			currentScreen.menuManager = this;
		}

		foreach (SaveStation allStation in FindObjectsByType<SaveStation>(FindObjectsSortMode.None))
		{
			if (allStation.inUse)
			{
				station = allStation;
				break;
			}
		}

		currentScreen.init(this);
	}

	public void backToMenu()
	{
		changeScreen(menuScreen);
	}

	public void changeScreen(GameObject screenPrefab)
	{
		GameObject newScreen = Instantiate(screenPrefab, this.transform);
		newScreen.SetActive(true);
		currentScreen.gameObject.SetActive(false);
		Destroy(currentScreen.gameObject);

		StationScreen screenScript = newScreen.GetComponentInChildren<StationScreen>();
		if (screenScript == null)
		{
			return;
		}
		currentScreen = screenScript;
		currentScreen.init(this);
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