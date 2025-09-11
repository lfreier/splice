using System;
using System.Collections;
using UnityEngine;

public class SoftlockPrevention : AutoDoor
{
	public enum SOFTLOCK_TYPE
	{
		KEYCARD = 0
	}

	public SoftlockPrevention door1;
	public SoftlockPrevention door2;

	public GameObject prefabToSpawn;

	public int count = 0;
	public bool fix = false;

	public override bool use(Actor user)
	{
		count++;
		if (door1.count > 0 && door2.count > 0 && !fix)
		{
			GameObject.Instantiate(prefabToSpawn, this.gameObject.transform.position, Quaternion.identity, null);
			fix = true;
		}

		return base.use(user);
	}
}