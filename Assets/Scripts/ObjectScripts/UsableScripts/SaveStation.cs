using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneDefs;

public class SaveStation : MonoBehaviour, UsableInterface
{
	public int saveStationSpawnIndex = 0;
	public int saveStationNumIndex = 0;

	public void use(Actor user)
	{
		LevelManager levelManager = user.gameManager.levelManager;
		if (user.gameManager != null && levelManager.usedSavedStations[saveStationNumIndex] == false)
		{
			levelManager.lastSavedSpawn = saveStationSpawnIndex;
			levelManager.usedSavedStations[saveStationNumIndex] = true;
			user.gameManager.save(user);
			levelManager.lastSavedAtStation = true;

			for (int j = 0; j < SceneManager.sceneCount; j++)
			{
				Scene curr = SceneManager.GetSceneAt(j);
				if (curr.buildIndex == (int)SCENE.SAVING)
				{
					SceneManager.UnloadSceneAsync(curr.buildIndex);
					continue;
				}
			}

			SceneManager.LoadSceneAsync((int)SCENE.SAVING, LoadSceneMode.Additive);
		}
	}
}