using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
	private bool powerOn;
	public Vector3 gridPosition;
	public LevelData currLevelData;

	private GameManager gameManager;

	public void startNewLevel()
	{
		if (gameManager == null)
		{
			gameManager = GameManager.Instance;
		}
		powerOn = true;
		//int i = 0;
		foreach (LevelData levelData in FindObjectsByType<LevelData>(FindObjectsSortMode.None))
		{
			
			currLevelData = levelData;
			gameManager.signalStartMusicEvent(currLevelData.sceneMusic);
			if (gameManager.currentScene < 0)
			{
				gameManager.currentScene = levelData.levelSceneIndex;
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