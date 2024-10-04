using System.Collections;
using UnityEngine;

public class NextLevel : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actor = collision.transform.GetComponent<Actor>();
		if (actor != null && actor.tag == ActorDefs.playerTag)
		{
			actor.gameManager.nextLevel(actor);
		}
	}
}