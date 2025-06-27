using System.Collections;
using UnityEngine;

public class RemoteComputer : MonoBehaviour, UsableInterface
{
	public GameObject[] worldObjects;
	public AudioSource computerAudioPlayer;
	public AudioClip computerUseSound;
	GameManager gm;

	void Start()
	{
		gm = GameManager.Instance;
		if (gm != null)
		{
			gm.powerChangedEvent += toggleComputer;
		}
	}

	void OnDestroy()
	{
		gm = GameManager.Instance;
		if (gm != null)
		{
			gm.powerChangedEvent -= toggleComputer;
		}
	}

	private void toggleComputer(bool powerOn)
	{
		if (gm == null && null == (gm = GameManager.Instance))
		{
			return;
		}

		if (worldObjects != null && worldObjects.Length > 0)
		{
			for (int i = 0; i < worldObjects.Length; i++)
			{
				AutoDoor door = worldObjects[i].GetComponentInChildren<AutoDoor>();
				if (door != null)
				{

					/* power is turning on after being off - close open doors */
					if (powerOn && !gm.levelManager.hasPower() && door.isOpen())
					{
						door.doorToggle(true);
					}
					/* power is turning off after being on - open closed doors */
					else if (!powerOn && gm.levelManager.hasPower() && !door.isOpen())
					{
						door.doorToggle(true);
					}
				}
			}
		}
	}

	public bool use(Actor user)
	{
		gm.playSound(computerAudioPlayer, computerUseSound.name, 1F);

		if (!gm.levelManager.hasPower())
		{
			return false;
		}

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
		return true;
	}
}