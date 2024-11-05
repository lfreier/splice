using System.Collections;
using UnityEngine;

public class Battery : MonoBehaviour, PickupInterface
{
	public int chargeAmt = 0;

	// Use this for initialization
	void Start()
	{

	}
	public int getCount()
	{
		return chargeAmt;
	}

	public PickupDefs.pickupType getPickupType()
	{
		return PickupDefs.pickupType.BATTERY;
	}

	public void init()
	{

	}

	public void pickup(Actor actorTarget)
	{
		GameObject weapon = actorTarget.getEquippedWeapon();
		SwingBatteryWeapon batteryWeap = weapon.GetComponentInChildren<SwingBatteryWeapon>();
		if (batteryWeap != null)
		{
			batteryWeap.fillBatteries(chargeAmt);
		}
		Destroy(this.gameObject);
	}
}