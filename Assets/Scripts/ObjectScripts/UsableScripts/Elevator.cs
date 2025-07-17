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
	private PlayerStats stats;

	void Start()
	{
		gameManager = GameManager.Instance;
		stats = gameManager.playerStats;
		index = (int)elevatorIndex;
	}

	public bool use(Actor user)
	{
		/* unlock elevator for use */
		stats.elevatorAvailable[index] = 2;

		if (isExitElevator)
		{
			if (stats.elevatorAvailable[(int)nextSpawn.elevatorIndex] == 0)
			{
				stats.elevatorAvailable[(int)nextSpawn.elevatorIndex] = 1;
				gameManager.playerStats.saveStationUses[(int)saveStationIndex.hub] = 1;
			}
		}

		inUse = true;
		playerActor = user;

		if (isExitElevator || stats.elevatorAvailable[index] > 0)
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