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

	private float currentAlpha;
	private float desiredAlpha = 0;

	private float currentAlpha2;
	private float desiredAlpha2 = 0;

	private float currentAlpha3;
	private float desiredAlpha3 = 0F;

	public float fadeChange = 0.5F;

	public Camera menuCam;
	public EventSystem eventSystem;
	public AudioListener audioListener;

	AsyncOperation op;

	public void Update()
	{
		if (transitioning)
		{
			if (!loadDone && op.isDone)
			{
				menuCam.enabled = false;
				SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(SceneDefs.GAME_START_SCENE));
				loadDone = true;
			}

			currentAlpha = Mathf.MoveTowards(currentAlpha, desiredAlpha, fadeChange * Time.deltaTime);
			currentAlpha2 = Mathf.MoveTowards(currentAlpha2, desiredAlpha2, fadeChange / 2 * Time.deltaTime);
			currentAlpha3 = Mathf.MoveTowards(currentAlpha3, desiredAlpha3, fadeChange / 2 * Time.deltaTime);

			backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, currentAlpha2);
			filterImage.color = new Color(filterImage.color.r, filterImage.color.g, filterImage.color.b, currentAlpha3);
			for (int i = 0; i < menuOptions.Length; i ++)
			{
				menuOptions[i].color = new Color(menuOptions[i].color.r, menuOptions[i].color.g, menuOptions[i].color.b, currentAlpha2);
			}

			if (currentAlpha == desiredAlpha && currentAlpha2 == desiredAlpha2 && currentAlpha3 == desiredAlpha3)
			{
				SceneManager.UnloadSceneAsync(SceneDefs.MAIN_MENU_SCENE);
			}
		}
	}
	public void startGame()
	{
		op = SceneManager.LoadSceneAsync(SceneDefs.GAME_START_SCENE, LoadSceneMode.Additive);
		transitioning = true;

		op.allowSceneActivation = true;
		audioListener.enabled = false;
		eventSystem.gameObject.SetActive(false);

		currentAlpha = 1;
		currentAlpha2 = backgroundImage.color.a;
		currentAlpha3 = filterImage.color.a;
	}

	public void quitGame()
	{
		Application.Quit();
	}
}
