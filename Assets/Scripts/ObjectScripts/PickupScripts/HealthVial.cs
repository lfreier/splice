using System.Collections;
using UnityEngine;

public class HealthVial : MonoBehaviour, PickupInterface
{
	public int count = 1;
	public float healAmount = 0;

	// Use this for initialization
	void Start()
	{

	}
	public int getCount()
	{
		return count;
	}

	public PickupDefs.pickupType getPickupType()
	{
		return PickupDefs.pickupType.HEALTH_VIAL;
	}

	public void init()
	{

	}

	public void pickup(Actor actorTarget)
	{
		actorTarget.takeHeal(healAmount);
		Destroy(this.gameObject);
	}
}