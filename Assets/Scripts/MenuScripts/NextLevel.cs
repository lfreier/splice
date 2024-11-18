using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextLevel : MonoBehaviour
{
	public int nextSceneIndex;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actor = collision.transform.GetComponent<Actor>();
		if (actor != null && actor.tag == ActorDefs.playerTag)
		{
			actor.gameManager.save(actor);
			actor.gameManager.nextLevel(actor, nextSceneIndex);
		}
	}
}