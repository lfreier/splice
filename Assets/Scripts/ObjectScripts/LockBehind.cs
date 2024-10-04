using System.Collections;
using UnityEngine;

public class LockBehind : MonoBehaviour
{
	public AutoDoor doorToLock;
	public PickupDefs.keycardType newLockType;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actor = collision.transform.GetComponent<Actor>();
		if (actor != null && actor.tag == ActorDefs.playerTag)
		{
			doorToLock.lockType = newLockType;
			doorToLock.doorLock();
		}
	}
}