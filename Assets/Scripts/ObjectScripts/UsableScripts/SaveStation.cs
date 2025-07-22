using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LevelManager;
using static SceneDefs;

public class SaveStation : MonoBehaviour, UsableInterface
{
	public levelSpawnIndex playerSpawnIndex = 0;
	public saveStationIndex saveStationNumIndex = 0;

	public AudioSource saveStationPlayer;
	public AudioClip useSaveStationSound;

	public Actor playerActor;

	public bool inUse = false;

	public bool isHubStation = false;

	public bool use(Actor user)
	{
		inUse = true;
		playerActor = user;
		GameManager gm = GameManager.Instance;
		gm.playSound(saveStationPlayer, useSaveStationSound.name, 1F);
		user.gameManager.loadPausedScene(user, SCENE.STATION_MENU);
		return true;
	}
}