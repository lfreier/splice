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
		if (actor != null && actor.tag == ActorDefs.playerTag)
		{
			triggered = true;
			if (tutorialImage != null)
			{
				actor.gameManager.loadPausedScene(actor, SceneDefs.SCENE.TUTORIAL);
			}
		}
	}
}