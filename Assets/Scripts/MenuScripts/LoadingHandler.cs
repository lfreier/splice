using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingHandler : MonoBehaviour
{
	public Canvas loadingCanvas;

	public MenuHandler menu;
	public Image loadingBar;
	public float fillSpeed;

	public bool reloadHUD;
	public bool resetPlayerData;

	private bool isLoading;

	private float targetProgress;

	private GameManager gameManager;
	private SceneLoadManager sceneLoadManager;

	private AsyncOperation newSceneOp;

	private void Awake()
	{
		sceneLoadManager = new SceneLoadManager();
		gameManager = GameManager.Instance;
		if (gameManager != null && gameManager.loadingHandler == null)
		{
			gameManager.loadingHandler = this;
		}
		sceneLoadManager.OnSceneLoaded += scene => Debug.Log("Scene loaded: " + scene);
		sceneLoadManager.OnSceneUnloaded += scene => Debug.Log("Scene unloaded: " + scene);
		sceneLoadManager.OnSceneGroupLoaded += sceneLoadFinished;
	}

	void Start()
	{
		reset();
	}

	private void Update()
	{
		if (isLoading)
		{
			float currentFill = loadingBar.fillAmount;
			float fillDiff = Mathf.Abs(currentFill - targetProgress);

			float dynamicFillSpeed = fillDiff * fillSpeed;

			loadingBar.fillAmount = Mathf.Lerp(currentFill, targetProgress, Time.deltaTime * dynamicFillSpeed);
		}
	}

	public void reset()
	{
		isLoading = false;
		loadingCanvas.gameObject.SetActive(false);
	}

	public async Task LoadSceneGroup(int[] scenes, bool showLoadingScreen, bool unloadLevel)
	{
		LoadingProgress loadProgress = new LoadingProgress();
		targetProgress = 1F;
		isLoading = true;
		loadingBar.fillAmount = 0;

		loadProgress.progressed += target => targetProgress = Mathf.Max(target, targetProgress);

		loadingCanvas.gameObject.SetActive(showLoadingScreen);
		if (showLoadingScreen)
		{
			SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(SceneDefs.SCENE_INDEX_MASK[(int)SceneDefs.SCENE.LOADING]));
		}

		if (unloadLevel)
		{
			await unloadAllScenes(false);
		}

		await sceneLoadManager.LoadScenes(scenes, loadProgress);

		if (resetPlayerData)
		{
			foreach (Actor actor in FindObjectsByType<Actor>(FindObjectsSortMode.None))
			{
				if (actor.tag == ActorDefs.playerTag)
				{
					gameManager.playerStats.savePlayerData(actor);
					break;
				}
			}
		}
		resetPlayerData = false;

		if (reloadHUD)
		{
			gameManager.signalInitHudEvent();
			reloadHUD = false;
		}

		if (showLoadingScreen)
		{
			menu.fadeOutScene();
		}
	}

	public void sceneLoadFinished()
	{
		Debug.Log("Scene group loaded");
	}

	public async Task unloadAllScenes(bool unloadBgScenes)
	{
		await sceneLoadManager.UnloadScenes(unloadBgScenes);
	}

	public async Task forceUnloadScenes(int[] scenes)
	{
		await sceneLoadManager.UnloadScenes(true, scenes);
	}

	public class LoadingProgress : IProgress<float>
	{
		public event Action<float> progressed;

		const float ratio = 1F;

		public void Report(float value)
		{
			progressed?.Invoke(value / ratio);
		}
	}
}