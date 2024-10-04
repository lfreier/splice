using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public Image backgroundImage;
	public Image filterImage;
	public Image[] menuOptions;

	private bool transitioning = false;
	private bool loadDone = false;

	private float currentAlpha = 1;
	private float desiredAlpha = 0;

	private float currentAlpha2 = 1;
	private float desiredAlpha2 = 0;

	private float currentAlpha3 = 1;
	private float desiredAlpha3 = 0F;

	private bool hack = false;

	public float fadeChange = 0.5F;

	public Canvas menuCanvas;

	public Camera menuCam;
	public EventSystem eventSystem;
	public AudioListener audioListener;

	AsyncOperation op;

	public void LateUpdate()
	{
		if (transitioning)
		{
			if (!loadDone && (op == null || op.isDone))
			{
				if (menuCam != null)
				{
					menuCam.enabled = false;
				}
				SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(SceneDefs.GAME_START_SCENE));
				loadDone = true;
				if (hack == true)
				{
					GameManager gm = GameManager.Instance;
					if (gm != null)
					{
						gm.startMutationSelect();
					}
				}
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
				for (int i = 0; i < SceneManager.sceneCount; i ++)
				{
					Scene curr = SceneManager.GetSceneAt(i);
					if (curr.buildIndex == SceneDefs.GAME_START_SCENE || curr.buildIndex == SceneDefs.PLAYER_HUD_SCENE)
					{
						continue;
					}
					SceneManager.UnloadSceneAsync(curr.buildIndex);
				}
			}
		}
	}

	public void enterMainScene()
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
		eventSystem.gameObject.SetActive(false);

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
		PlayerHUD hud = Camera.main.transform.GetComponentInChildren<PlayerHUD>();

		if (hud != null)
		{
			hud.mute();
		}
	}

	public void restartWithSelection()
	{
		hack = true;
		startGame();
	}

	public void restartGame()
	{
		Time.timeScale = 1;
		SceneManager.UnloadSceneAsync(SceneDefs.PLAYER_HUD_SCENE);
		startGame();
	}

	public void resumeGame()
	{
		Time.timeScale = 1;
		enterMainScene();
	}

	public void startGame()
	{
		Time.timeScale = 1;
		for (int i = 0; i < SceneManager.loadedSceneCount; i++)
		{
			Scene curr = SceneManager.GetSceneAt(i);
			if (curr.buildIndex == SceneDefs.GAME_START_SCENE)
			{
				SceneManager.UnloadSceneAsync(curr.buildIndex);
				break;
			}
		}

		op = SceneManager.LoadSceneAsync(SceneDefs.GAME_START_SCENE, LoadSceneMode.Additive);
		op.allowSceneActivation = true;

		enterMainScene();
	}

	public void quitGame()
	{
		Application.Quit();
	}
}
