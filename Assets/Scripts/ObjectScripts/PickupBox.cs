using System.Collections;
using UnityEngine;

public class PickupBox : MonoBehaviour
{
	public GameObject[] pickup;
	public Collider2D pickupCollider;
	public int pickupSendForce = 700;

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

		if (pickupHolder != null)
		{
			PickupInterface pickup = pickupHolder.GetComponentInChildren<PickupInterface>();
			if (pickup != null)
			{
				pickupHolder.transform.SetPositionAndRotation(gameObject.transform.position, Quaternion.identity);
				Rigidbody2D body = pickupHolder.GetComponentInChildren<Rigidbody2D>();
				if (body != null)
				{
					body.bodyType = RigidbodyType2D.Dynamic;
					Vector2 pushDirection = Vector2.ClampMagnitude((-transform.up) + (transform.right * Random.Range(0, 1F)) + (transform.right * Random.Range(-1F, 0)), 1);
					Debug.Log("Push direction: " + pushDirection);
					body.AddForce(pickupSendForce * pushDirection);
					body.AddTorque(Random.Range(-20F, 20F));
					pickupHolder = null;
				}
			}
		}

		return pickupHolder;
	}

	public bool hasPickup()
	{
		return (pickup != null && pickup.Length > 0 && pickup[0] != null);
	}
}