using System.Collections;
using UnityEngine;
using static LevelManager;
using static SceneDefs;

public class Elevator : MonoBehaviour, UsableInterface
{
	public Actor playerActor;

	public PlayerSpawnScriptable nextSpawn;

	public bool isExitElevator = false;
	public bool inUse = false;

	public elevatorIndex elevatorIndex;
	private int index;

	private GameManager gameManager;
	private LevelManager levelManager;

	void Start()
	{
		gameManager = GameManager.Instance;
		levelManager = gameManager.levelManager;
		index = (int)elevatorIndex;
	}

	public bool use(Actor user)
	{
		/* unlock elevator for use */
		levelManager.elevatorAvailable[index] = true;

		if (isExitElevator)
		{
			if (!levelManager.elevatorAvailable[(int)nextSpawn.elevatorIndex] && elevatorIndex != elevatorIndex.hub)
			{
				levelManager.saveStationUses[(int)saveStationIndex.hub]++;
			}
			levelManager.elevatorAvailable[(int)nextSpawn.elevatorIndex] = true;
		}

		inUse = true;
		playerActor = user;

		if (isExitElevator || levelManager.elevatorAvailable[index])
		{
			/* show the elevator menu now */
			gameManager.loadPausedScene(user, SCENE.STATION_MENU);
		}
		else if (nextSpawn != null && nextSpawn.sceneIndex == SCENE.WIN)
		{
			user.gameManager.gameWin(user);
			return true;
		}

		return true;
	}
}