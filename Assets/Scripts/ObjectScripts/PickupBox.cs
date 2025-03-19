using System.Collections;
using UnityEngine;

public class PickupBox : MonoBehaviour
{
	public GameObject[] pickup;
	public Collider2D pickupCollider;

	private int pickupIndex;

	private void Start()
	{
		pickupIndex = 0;
		checkGlowDisable();
	}

	private void checkGlowDisable()
	{
		PickupEngine engine = gameObject.GetComponent<PickupEngine>();
		if ((pickup == null || pickup.Length <= 0 || pickup[0] == null) && engine != null)
		{
			engine.disableGlow();
			if (pickupCollider != null)
			{
				pickupCollider.enabled = false;
			}
		}
	}

	public void clearPickup()
	{
		pickup = null;
		checkGlowDisable();
	}

	public GameObject getPickup()
	{
		if (pickup == null || pickupIndex >= pickup.Length)
		{
			return null;
		}
		GameObject pickupHolder = Instantiate(pickup[pickupIndex]);
		if (pickupIndex < pickup.Length - 1)
		{
			pickupIndex++;
		}
		else
		{
			pickup = null;
		}

		checkGlowDisable();

		return pickupHolder;
	}

	public bool hasPickup()
	{
		return (pickup != null && pickup.Length > 0 && pickup[0] != null);
	}
}