using System.Collections;
using UnityEngine;

public interface PickupInterface
{
	public int getCount();
	public PickupDefs.pickupType getPickupType();
	public void init();

	public void pickup(Actor actorTarget);
}