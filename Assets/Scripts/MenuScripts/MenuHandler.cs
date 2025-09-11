using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameManager;
using static SceneDefs;

public class MenuHandler : MonoBehaviour
{
	public Image backgroundImage;
	public Image filterImage;
	public Image[] menuOptions;
	public TextMeshProUGUI[] fadeInText;

	public Image holdToFillImage;
	public float holdTimerLength = 0;
	private float holdTimer = 0;
	private bool isHeld = false;

	private bool transitioning = false;
	private bool loadDone = false;
	private bool showLoading = false;

	private float currentAlpha = 1;
	private float desiredAlpha = 0;

	private float currentAlpha2 = 1;
	private float desiredAlpha2 = 0;

	private float currentAlpha3 = 1;
	private float desiredAlpha3 = 0F;

	public float fadeChange = 0.5F;

	public Canvas menuCanvas;

	public Camera menuCam;
	public EventSystem eventSystem;
	public AudioListener audioListener;

	public AudioSource menuMusic;
	public AudioSource menuClickPlayer;

	public GameObject loadMenuObject;
	public GameObject optionsMenuObject;

	public Button[] menuButtons;

	private float volumeDecrement;

	private int nextScene = (int)SCENE.LEVEL_START;
	private bool buttonsLocked = false;

	private GameManager gameManager = null;

	private async void Awake()
	{
		for (int i = 0; i <  SceneManager.sceneCount; i++)
		{
			if (SceneManager.GetSceneAt(i).buildIndex == SCENE_INDEX_MASK[(int)SCENE.MANAGER])
			{
				return;
			}
		}

		await SceneManager.LoadSceneAsync(SCENE_INDEX_MASK[(int)SCENE.MANAGER], LoadSceneMode.Additive);
		buttonsLocked = false;

		if (menuButtons != null && menuButtons.Length > 0)
		{
			foreach (Button button in menuButtons)
			{
				button.interactable = true;
			}
		}
	}

	private void Start()
	{

		gameManager = GameManager.Instance;
		if (gameManager != null)
		{
			gameManager.closeMenusEvent += killMenuScenes;
			gameManager.startMusicEvent += killMenuMusic;
		}

		if (optionsMenuObject != null)
		{
			SaveManager.OptionsSaveData options = SaveManager.loadOptionsDataFromDisk();

			if (options.musicVolume < 0 || options.effectsVolume < 0)
			{
				options.musicVolume = 0.5F;
				options.effectsVolume = 0.5F;
			}

			gameManager.musicVolume = options.musicVolume;
			gameManager.effectsVolume = options.effectsVolume;

			if (menuMusic != null)
			{
				menuMusic.volume = options.musicVolume;
				if (menuMusic.volume == 0)
				{
					menuMusic.Pause();
				}
				else
				{
					menuMusic.UnPause();
				}
			}
			else
			{
				GameManager.Instance.signalVolumeChangeEvent(options.musicVolume);
			}
		}
	}

	private void OnDestroy()
	{
		gameManager = GameManager.Instance;
		if (gameManager != null)
		{
			gameManager.closeMenusEvent -= killMenuScenes;
			gameManager.startMusicEvent -= killMenuMusic;
		}
	}

	public void Update()
	{
		/* only used for hold to click menu buttons */

		//increment timer
		if (isHeld && holdTimerLength > 0 && holdToFillImage != null && Time.timeScale <= 0)
		{
			holdTimer += Time.fixedUnscaledDeltaTime;
			holdToFillImage.fillAmount = holdTimer / holdTimerLength;
			if (holdTimer >= holdTimerLength)
			{
				holdTimer = 0;
				isHeld = false;
				PlayerInputs inputs = GameManager.Instance.playerStats.player.GetComponentInChildren<PlayerInputs>();
				if (inputs != null)
				{
					inputs.locked = false;
				}
				resumeGame();
			}
		}
		//decrement timer
		else if (holdTimer > 0)
		{
			holdTimer -= Time.fixedUnscaledDeltaTime;
			holdToFillImage.fillAmount = holdTimer / holdTimerLength;
		}
	}

	public void onHoldButtonClick()
	{
		isHeld = true;
	}
	public void onHoldButtonRelease()
	{
		isHeld = false;
	}

	public async void LateUpdate()
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}

		if (transitioning)
		{
			if (!loadDone)
			{
				if (menuCam != null)
				{
					menuCam.enabled = false;
				}
				loadDone = true;
			}

			currentAlpha = Mathf.MoveTowards(currentAlpha, desiredAlpha, fadeChange * Time.unscaledDeltaTime);
			currentAlpha2 = Mathf.MoveTowards(currentAlpha2, desiredAlpha2, fadeChange / 2 * Time.unscaledDeltaTime);
			currentAlpha3 = Mathf.MoveTowards(currentAlpha3, desiredAlpha3, fadeChange * Time.unscaledDeltaTime);

			if (backgroundImage != null)
			{
				backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, currentAlpha2);
			}
			if (filterImage != null)
			{
				filterImage.color = new Color(filterImage.color.r, filterImage.color.g, filterImage.color.b, currentAlpha3);
			}
			if (menuMusic != null)
			{
				menuMusic.volume -= volumeDecrement;
			}

			for (int i = 0; i < menuOptions.Length; i ++)
			{
				if (menuOptions[i] == null)
				{
					continue;
				}
				menuOptions[i].color = new Color(menuOptions[i].color.r, menuOptions[i].color.g, menuOptions[i].color.b, currentAlpha);
			}
			for (int i = 0; fadeInText != null && i < fadeInText.Length; i++)
			{
				if (fadeInText[i] == null)
				{
					continue;
				}
				fadeInText[i].color = new Color(fadeInText[i].color.r, fadeInText[i].color.g, fadeInText[i].color.b, currentAlpha);
			}

			if (currentAlpha == desiredAlpha && currentAlpha2 == desiredAlpha2 && currentAlpha3 == desiredAlpha3)
			{
				transitioning = false;
				/* only disable the loading screen, don't destroy it */
				if (this.gameObject.scene.buildIndex != (int)SCENE.LOADING)
				{
					int[] temp = { SCENE_BUILD_MASK[this.gameObject.scene.buildIndex] };
					await gameManager.loadingHandler.forceUnloadScenes(temp);
				}
				gameManager.loadingHandler.reset();
				Scene curr = SceneManager.GetSceneByBuildIndex(SCENE_INDEX_MASK[gameManager.currentScene]);
				while (!curr.isLoaded)
				{
					await Task.Delay(100);
				}
				SceneManager.SetActiveScene(curr);
			}
		}
	}

	public void killMenuMusic(MusicScriptable music)
	{
		if (menuMusic != null)
		{
			volumeDecrement = menuMusic.volume / 128;
		}
	}

	public void killMenuScenes()
	{
		SCENE sceneIndex = (SCENE)SCENE_BUILD_MASK[this.gameObject.scene.buildIndex];
		if (sceneIndex == SCENE.PAUSE || sceneIndex == SCENE.TUTORIAL)
		{
			currentAlpha = desiredAlpha;
			currentAlpha2 = desiredAlpha2;
			currentAlpha3 = desiredAlpha3;
		}
	}

	public void fadeOutScene()
	{
		if (menuCanvas != null)
		{
			menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		}
		transitioning = true;
		loadDone = false;

		if (audioListener != null)
		{
			audioListener.enabled = false;
		}
		if (eventSystem != null)
		{
			eventSystem.gameObject.SetActive(false);
		}

		currentAlpha = 1;
		currentAlpha2 = 0;
		currentAlpha3 = 0;
		if (backgroundImage != null)
		{
			currentAlpha2 = backgroundImage.color.a;
		}
		if (filterImage != null)
		{
			currentAlpha3 = filterImage.color.a;
		}
	}

	public void mute()
	{
		gameManager.signalVolumeChangeEvent(0);
	}

	public async void returnToMainMenu()
	{
		await gameManager.loadingHandler.unloadAllScenes(true);
		int[] mainMenuScene = new int[] { (int)SCENE.MAIN_MENU };
		await gameManager.loadingHandler.LoadSceneGroup(mainMenuScene, false, false);
	}

	public async void restartGame()
	{
		buttonsLocked = true;
		gameManager = GameManager.Instance;
		/* Unload extra scenes, force unload the HUD scene, then reload the HUD scene */
		int[] hudScene = new int[] { (int)SCENE.PLAYER_HUD };
		await gameManager.loadingHandler.unloadAllScenes(false);
		await gameManager.loadingHandler.forceUnloadScenes(hudScene);
		showLoading = true;
		gameManager.levelManager.guidTable.Clear();
		await gameManager.loadingHandler.LoadSceneGroup(hudScene, showLoading, false);
		nextScene = gameManager.levelManager.lastSavedLevelIndex;

		gameManager.saveManager.loadAllData();
		SaveManager.loadPlayerDataFromDisk(gameManager.currentSaveSlot);

		buttonsLocked = false;
		startGame(true);
	}

	public void resumeGame()
	{
		GameManager gManager = GameManager.Instance;
		nextScene = gManager.currentScene;
		Time.timeScale = 1;
		fadeOutScene();
	}

	public async void startGameFromSaveSlot(int saveSlot)
	{
		GameManager gManager = GameManager.Instance;
		showLoading = true;

		bool fromMenu = saveSlot < 0;
		if (fromMenu)
		{
			saveSlot = 0;
			int i = 0;
			for (i = 0; i < SaveManager.TOTAL_SAVES; i++)
			{
				if (null == SaveManager.loadPlayerDataFromDisk(i))
				{
					saveSlot = i;
					break;
				}
			}
			/* just quit if we're going to overwrite */
			if (i >= SaveManager.TOTAL_SAVES)
			{
				gManager.playSound(menuClickPlayer, gManager.audioManager.errorSound.name, 1F);
				return;
			}
		}

		gManager.currentSaveSlot = saveSlot;
		gManager.saveManager.loadAllData();
		PlayerSaveData playerData = SaveManager.loadPlayerDataFromDisk(saveSlot);
		if (playerData == null)
		{
			gManager.playerStats = new PlayerStats();
			gManager.playerStats.gameManager = gManager;
			gManager.levelManager.lastSavedLevelIndex = 0;
			nextScene = (int)SCENE.LEVEL_START;
			gManager.levelManager.lastSavedSpawn = LevelManager.levelSpawnIndex.levelStartSpawn;
		}
		else
		{
			gManager.playerStats.savePlayerDataToMemory(playerData);
			gManager.levelManager.lastSavedLevelIndex = playerData.lastSavedLevel;
			nextScene = playerData.lastSavedLevel;
			gManager.levelManager.lastSavedSpawn = (LevelManager.levelSpawnIndex)playerData.lastSavedSpawn;
		}
		if (!fromMenu)
		{
			await gameManager.loadingHandler.unloadAllScenes(false);
			startGame(true);
		}
		else
		{
			startGame(false);
			gameManager.levelManager.lastSavedLevelIndex = nextScene;
		}
	}

	public async void startGame(bool reset)
	{
		if (buttonsLocked)
		{
			return;
		}

		buttonsLocked = true;
		if (menuButtons != null && menuButtons.Length > 0)
		{
			foreach (Button button in menuButtons)
			{
				button.interactable = false;
			}
		}
		gameManager = GameManager.Instance;
		Time.timeScale = 1;
		int[] nextLevel = { nextScene };

		gameManager.loadingHandler.reloadHUD = true;
		gameManager.loadingHandler.resetPlayerData = !reset;
		gameManager.levelManager.guidTable.Clear();
		await gameManager.loadingHandler.LoadSceneGroup(nextLevel, showLoading, false);
		showLoading = false;

		gameManager.playerStats.resetCounts();
		await gameManager.levelManager.startNewLevel((int)gameManager.levelManager.lastSavedSpawn, (int)gameManager.levelManager.lastSavedLevelIndex);

		gameManager.currentScene = nextScene;

		if ((SCENE)nextScene == SCENE.MANAGER)
		{
			Debug.Log("ALERT 3");
		}
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(SCENE_INDEX_MASK[nextScene]));

		gameManager.signalVolumeChangeEvent(gameManager.musicVolume);

		fadeOutScene();
	}

	public void loadGameButton()
	{
		//load save slot scene
		if (!buttonsLocked && loadMenuObject != null)
		{
			loadMenuObject.SetActive(true);
			gameObject.SetActive(false);
		}
	}

	public void optionsButton()
	{
		//load save slot scene
		if (!buttonsLocked && optionsMenuObject != null)
		{
			optionsMenuObject.SetActive(true);
			gameObject.SetActive(false);
		}
	}

	public void playClickSound()
	{
		if (menuClickPlayer != null)
		{
			gameManager.playSound(menuClickPlayer, gameManager.audioManager.menuClick.name, 1F);
		}
	}

	public IEnumerator waitForClick(BasicFunc funcToRun)
	{
		while (menuClickPlayer.isPlaying)
		{
			yield return null;
		}

		if (funcToRun != null)
		{
			funcToRun.Invoke();
		}
	}

	public void onQuitToMenu()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.quitToMenu();
		}
	}

	public void quitGame()
	{
		if (buttonsLocked)
		{
			return;
		}
		Application.Quit();
	}
}
