using System.Collections;
using UnityEngine;

public class SaveStation : MonoBehaviour, UsableInterface
{
	public int saveStationSpawnIndex = 0;
	private bool used = false;

	public void use(Actor user)
	{
		if (!used)
		{
			user.gameManager.levelManager.lastSavedSpawn = saveStationSpawnIndex;
			user.gameManager.save(user);
			used = true;
		}
	}
}