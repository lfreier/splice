using System.Collections;
using UnityEngine;

public class PickupBox : MonoBehaviour
{
	public GameObject pickup;

	private void Start()
	{
		checkGlowDisable();
	}

	private void checkGlowDisable()
	{
		PickupEngine engine = gameObject.GetComponent<PickupEngine>();
		if (pickup == null && engine != null)
		{
			engine.disableGlow();
		}
	}

	public GameObject getPickup()
	{
		if (pickup == null)
		{
			return null;
		}
		GameObject pickupHolder = Instantiate(pickup);
		pickup = null;

		checkGlowDisable();

		return pickupHolder;
	}

	public bool hasPickup()
	{
		return pickup != null;
	}
}