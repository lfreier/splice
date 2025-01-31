using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour
{
	private bool powerOn;
	public Vector3 gridPosition;
	public LevelData currLevelData;

	public GameObject playerPrefab;

	public int lastSavedSpawn;

	public PlayerSpawnScriptable[] levelSpawns;

	public enum levelSpawnIndex
	{
		levelStartSpawn = 0,
		levelStartSaveSpawn = 1,
		levelOfficeSpawn = 2,
		levelOfficeSaveSpawn = 3
	}

	public CameraHandler camHandler;

	private GameManager gameManager;

	public void startNewLevel(int spawnIndex)
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}
		powerOn = true;
		//int i = 0;
		foreach (LevelData levelData in FindObjectsByType<LevelData>(FindObjectsSortMode.None))
		{
			camHandler = levelData.gameObject.GetComponent<CameraHandler>();
			gameManager.signalStartMusicEvent(levelData.sceneMusic);
			if (gameManager.currentScene < 0)
			{
				gameManager.currentScene = levelData.levelSceneIndex;
			}
			/* spawn player if one isn't there */
			if (spawnIndex >= 0)
			{
				foreach (Actor actor in FindObjectsByType<Actor>(FindObjectsSortMode.None))
				{
					if (actor.tag == ActorDefs.playerTag)
					{
						Destroy(actor.gameObject);
					}
				}
				GameObject tempPlayer = Instantiate(playerPrefab);
				tempPlayer.transform.SetPositionAndRotation(levelSpawns[spawnIndex].spawnPosition, Quaternion.identity);
				tempPlayer.transform.Rotate(new Vector3(0, 0, levelSpawns[spawnIndex].spawnRotation));
				
				camHandler.player = tempPlayer;

				Actor playerActor = tempPlayer.GetComponent<Actor>();
				if (playerActor != null)
				{
					gameManager.playerStats.player = playerActor;
					gameManager.playerStats.loadPlayerData(playerActor);
				}
			}
			return;
			/*
			gridPosition = currLevelData.transform.position;
			if (i > 0)
			{
				Debug.Log("Internal error: multiple PathGrid objects in a level");
				break;
			}
			i++;
			*/
		}
	}

	/* TODO: this definitely doesn't work */
	public static void dropItem(GameObject toDrop, Vector2 target)
	{
		Rigidbody2D body = toDrop.GetComponentInChildren<Rigidbody2D>();
		if (body != null)
		{
			Vector2 newTarget = new Vector2(target.x + Random.Range(-1F, 1F), target.y + Random.Range(-1F, 1F));
			body.transform.Translate((newTarget - body.position) * 500F);
			body.transform.RotateAround(body.transform.position, Vector3.forward, Random.Range(-100F, 100F));
		}
	}

	public bool hasPower()
	{
		return powerOn;
	}

	public void setPower(bool powerToSet)
	{
		powerOn = powerToSet;
	}
}