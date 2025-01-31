using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextLevel : MonoBehaviour
{
	public int nextSceneIndex;

	public LevelManager.levelSpawnIndex spawnIndex;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actor = collision.transform.GetComponent<Actor>();
		if (actor != null && actor.tag == ActorDefs.playerTag)
		{
			if (nextSceneIndex == SceneDefs.WIN_SCENE)
			{
				actor.gameManager.gameWin(actor);
				return;
			}
			actor.gameManager.save(actor);
			actor.gameManager.nextLevel(actor, nextSceneIndex, (int)spawnIndex);
		}
	}
}