using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneDefs;

public class SaveStation : MonoBehaviour, UsableInterface
{
	public int playerSpawnIndex = 0;
	public int saveStationNumIndex = 0;

	public Actor playerActor;

	public bool inUse = false;

	public bool use(Actor user)
	{
		inUse = true;
		playerActor = user;
		user.gameManager.loadPausedScene(user, SCENE.STATION_MENU);
		return true;
	}
}