using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static GameManager;
using static SceneDefs;

public class StationMenuManager : MonoBehaviour
{
	public AudioListener audioListener;
	public EventSystem eventSystem;

	[field: HideInInspector]
	public StationScreen currentScreen;

	public GameObject menuScreen;
	public GameObject scanScreen;
	public GameObject scanScreenDB;
	public GameObject databaseScreen;
	public GameObject mutationScreen;

	public GameObject elevatorScreen;
	public GameObject elevatorScreenAtFinale;
	public GameObject elevatorScreenToEnding;

	[field: HideInInspector]
	public SaveStation station;

	[field: HideInInspector]
	public Elevator elevator;

	public AudioSource stationClickPlayer;

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

		if (station == null)
		{
			foreach (Elevator trigger in FindObjectsByType<Elevator>(FindObjectsSortMode.None))
			{
				if (trigger.inUse)
				{
					elevator = trigger;
					switch(elevator.specialType)
					{
						case Elevator.elevatorSpecialType.ending:
							changeScreen(elevatorScreenToEnding);
							break;
						case Elevator.elevatorSpecialType.atFinale:
							changeScreen(elevatorScreenAtFinale);
							break;
						case Elevator.elevatorSpecialType.basic:
						default:
							changeScreen(elevatorScreen);
							break;
					}
					/* make sure not to init twice */
					return;
				}
			}
		}

		currentScreen.init(this);
	}

	public void backToMenu()
	{
		changeScreen(menuScreen);
	}

	public void changeScreen(GameObject screenPrefab, int weapIndex)
	{
		GameObject newScreen = Instantiate(screenPrefab, this.transform);
		newScreen.SetActive(true);
		currentScreen.gameObject.SetActive(false);
		Destroy(currentScreen.gameObject);

		ScanScreen screenScript = newScreen.GetComponentInChildren<ScanScreen>();
		if (screenScript == null)
		{
			return;
		}
		screenScript.preInit(weapIndex);
		currentScreen = screenScript;
		currentScreen.init(this);
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
		if (station != null)
		{
			station.inUse = false;
			station.playerActor = null;
		}
		else if (elevator != null)
		{
			elevator.inUse = false;
			elevator.playerActor = null;
		}

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

	public void playClickSound()
	{
		if (stationClickPlayer != null)
		{
			gameManager.playSound(stationClickPlayer, gameManager.audioManager.menuClick.name, 1F);
		}
	}

	public IEnumerator waitForClick(BasicFunc funcToRun)
	{
		while (stationClickPlayer.isPlaying)
		{
			yield return null;
		}

		if (funcToRun != null)
		{
			funcToRun.Invoke();
		}
	}
}