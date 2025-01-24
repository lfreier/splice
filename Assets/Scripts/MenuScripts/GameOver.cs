using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
	public int sceneBuildIndex;

	public Camera gameOverCam;

	public Image deathHeartImage;

	public Image[] fadeInImages;
	public TextMeshProUGUI[] fadeInText;

	private float currentAlpha = 0;
	public float desiredAlpha = 1;

	public float fadeInChange = 0.25F;

	private float startPumpScale = 1;
	private float currPumpScale = 1;
	public float finalPumpScale;
	public float pumpMult = 2;

	private float timer;
	public float pumpTimer;

	public bool transitioning;
	private GameManager gameManager;


	void Start()
	{
		gameManager = GameManager.Instance;
		if (fadeInImages.Length > 0)
		{
			transitioning = true;
		}
		Scene mainScene = SceneManager.GetSceneByBuildIndex(gameManager.currentScene);
		for (int i = 0; i < SceneManager.sceneCount; i ++)
		{
			Scene curr = SceneManager.GetSceneAt(i);
			if (gameOverCam != null && curr.buildIndex == SceneDefs.PLAYER_HUD_SCENE)
			{
				SceneManager.UnloadSceneAsync(curr.buildIndex);
			}
		}
		GameObject[] objectList = mainScene.GetRootGameObjects();
		foreach (GameObject obj in objectList)
		{
			Camera cam = obj.GetComponent<Camera>();
			if (cam != null && gameOverCam != null)
			{
				gameOverCam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, gameOverCam.transform.position.z);
				Destroy(cam);
				break;
			}
		}
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneBuildIndex));
		currentAlpha = 0;
		if (deathHeartImage != null)
		{
			startPumpScale = currPumpScale = deathHeartImage.rectTransform.localScale.x;
		}
		timer = pumpTimer;
	}

	void LateUpdate()
	{
		//fade in the game over menu
		if (transitioning)
		{
			if (gameOverCam != null)
			{
				foreach (Actor actor in FindObjectsByType<Actor>(FindObjectsSortMode.None))
				{
					if (actor.tag == ActorDefs.playerTag)
					{
						gameOverCam.transform.position = actor.transform.position;
					}
				}
			}

			currentAlpha = Mathf.MoveTowards(currentAlpha, desiredAlpha, fadeInChange * Time.unscaledDeltaTime);

			for (int i = 0; i < fadeInImages.Length; i++)
			{
				fadeInImages[i].color = new Color(fadeInImages[i].color.r, fadeInImages[i].color.g, fadeInImages[i].color.b, currentAlpha);
			}

			for (int i = 0; fadeInText != null && i < fadeInText.Length; i++)
			{
				if (fadeInText[i] == null)
				{
					continue;
				}
				fadeInText[i].color = new Color(fadeInText[i].color.r, fadeInText[i].color.g, fadeInText[i].color.b, currentAlpha);
			}

			if (currentAlpha == desiredAlpha)
			{
				transitioning = false;
			}
		}
		if (deathHeartImage != null)
		{
			pumpHeart();
		}
	}

	void pumpHeart()
	{
		if (timer > 0)
		{
			timer -= Time.deltaTime;
			return;
		}

		if (currPumpScale >= finalPumpScale)
		{
			currPumpScale = startPumpScale;
			timer = pumpTimer;
		}

		currPumpScale += pumpMult * Time.unscaledDeltaTime;
		if (currPumpScale >= finalPumpScale)
		{
			currPumpScale = finalPumpScale;
			timer = pumpTimer / 2;
		}

		deathHeartImage.rectTransform.localScale = new Vector3(currPumpScale, currPumpScale, 1);
	}
}