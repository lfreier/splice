using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
	public Image backgroundImage;
	public Image filterImage;
	public Image[] menuOptions;

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

	private int nextScene = SceneDefs.LEVEL_START_SCENE;

	private GameManager gameManager = null;

	private async void Awake()
	{
		for (int i = 0; i <  SceneManager.sceneCount; i++)
		{
			if (SceneManager.GetSceneAt(i).buildIndex == SceneDefs.MANAGER_SCENE)
			{
				return;
			}
		}

		await SceneManager.LoadSceneAsync(SceneDefs.MANAGER_SCENE, LoadSceneMode.Additive);
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

			currentAlpha = Mathf.MoveTowards(currentAlpha, desiredAlpha, fadeChange * Time.deltaTime);
			currentAlpha2 = Mathf.MoveTowards(currentAlpha2, desiredAlpha2, fadeChange / 2 * Time.deltaTime);
			currentAlpha3 = Mathf.MoveTowards(currentAlpha3, desiredAlpha3, fadeChange * Time.deltaTime);

			if (backgroundImage != null)
			{
				backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, currentAlpha2);
			}
			if (filterImage != null)
			{
				filterImage.color = new Color(filterImage.color.r, filterImage.color.g, filterImage.color.b, currentAlpha3);
			}
			for (int i = 0; i < menuOptions.Length; i ++)
			{
				menuOptions[i].color = new Color(menuOptions[i].color.r, menuOptions[i].color.g, menuOptions[i].color.b, currentAlpha);
			}

			if (currentAlpha == desiredAlpha && currentAlpha2 == desiredAlpha2 && currentAlpha3 == desiredAlpha3)
			{
				transitioning = false;
				/* only disable the loading screen, don't destroy it */
				if (this.gameObject.scene.buildIndex != SceneDefs.LOADING_SCENE)
				{
					int[] temp = { this.gameObject.scene.buildIndex };
					await gameManager.loadingHandler.forceUnloadScenes(temp);
				}
				gameManager.loadingHandler.reset();
			}
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
		gameManager.signalMuteEvent();
	}

	public async void restartGame()
	{
		gameManager = GameManager.Instance;
		/* Unload extra scenes, force unload the HUD scene, then reload the HUD scene */
		int[] hudScene = new int[] { SceneDefs.PLAYER_HUD_SCENE };
		await gameManager.loadingHandler.unloadAllScenes(false);
		await gameManager.loadingHandler.forceUnloadScenes(hudScene);
		showLoading = true;
		await gameManager.loadingHandler.LoadSceneGroup(hudScene, showLoading, false);
		nextScene = gameManager.currentScene;
		startGame();
	}

	public void resumeGame()
	{
		GameManager gManager = GameManager.Instance;
		nextScene = gManager.currentScene;
		Time.timeScale = 1;
		fadeOutScene();
	}

	public async void startGame()
	{
		gameManager = GameManager.Instance;
		Time.timeScale = 1;
		int[] nextLevel = { nextScene };
		gameManager.loadingHandler.reloadHUD = true;
		gameManager.loadingHandler.resetPlayerData = true;
		await gameManager.loadingHandler.LoadSceneGroup(nextLevel, showLoading, false);
		showLoading = false;
		gameManager.resetLevel();

		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(nextScene));

		fadeOutScene();
	}

	public void quitGame()
	{
		Application.Quit();
	}
}
