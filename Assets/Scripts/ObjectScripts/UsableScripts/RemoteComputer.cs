using System.Collections;
using UnityEngine;

public class RemoteComputer : MonoBehaviour, UsableInterface
{
	public GameObject[] worldObjects;

	public void use(Actor user)
	{
		if (worldObjects != null && worldObjects.Length > 0)
		{
			for (int i = 0; i < worldObjects.Length; i++)
			{
				AutoDoor door = worldObjects[i].GetComponentInChildren<AutoDoor>();
				if (door != null)
				{
					door.doorToggle(true);
				}
			}
		}
	}
}