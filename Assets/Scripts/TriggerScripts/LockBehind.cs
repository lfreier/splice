using System.Collections;
using UnityEngine;

public class LockBehind : MonoBehaviour
{
	public AutoDoor doorToLock;
	public PickupDefs.keycardType newLockType;
	public MusicScriptable musicToPlay;

	private bool isLocked = false;
	private GameManager gm;

	private void Start()
	{
		gm = GameManager.Instance;
		isLocked = false;
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actor = collision.transform.GetComponent<Actor>();
		if (actor != null && actor.tag == ActorDefs.playerTag && isLocked == false)
		{
			doorToLock.lockType = newLockType;
			doorToLock.doorLock();
			if (musicToPlay != null)
			{
				gm.signalStartMusicEvent(musicToPlay);
				isLocked = true;
			}
		}
	}
}