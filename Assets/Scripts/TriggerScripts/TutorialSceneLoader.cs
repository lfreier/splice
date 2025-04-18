using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSceneLoader : MonoBehaviour
{
	public Sprite tutorialImage;
	public bool triggered = false;

	//TODO: destroy this when done

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actor = collision.transform.GetComponent<Actor>();
		if (actor != null && actor.tag == ActorDefs.playerTag && tutorialImage != null)
		{
			triggered = true;
			Time.timeScale = 0;

			for (int j = 0; j < SceneManager.sceneCount; j++)
			{
				Scene curr = SceneManager.GetSceneAt(j);
				if (curr.buildIndex == SceneDefs.SCENE_INDEX_MASK[(int)SceneDefs.SCENE.TUTORIAL])
				{
					SceneManager.UnloadSceneAsync(curr.buildIndex);
					continue;
				}
			}

			SceneManager.LoadSceneAsync((int)SceneDefs.SCENE.TUTORIAL, LoadSceneMode.Additive);
			actor.gameManager.levelManager.camHandler.stopCam(true);

			PlayerInputs inputs = actor.GetComponentInChildren<PlayerInputs>();
			if (inputs != null)
			{
				inputs.paused = true;
			}
		}
	}
}