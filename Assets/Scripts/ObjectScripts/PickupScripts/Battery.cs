using System.Collections;
using UnityEngine;

public class Battery : MonoBehaviour, PickupInterface
{
	public int chargeAmt = 0;

	[SerializeField]
	private Sprite icon;

	// Use this for initialization
	void Start()
	{

	}
	public int getCount()
	{
		return chargeAmt;
	}

	public Sprite getIcon()
	{
		return icon;
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
		GameManager gameManager = GameManager.Instance;
		if (gameManager != null)
		{
			gameManager.signalUpdateItemCount(1, PickupDefs.usableType.BATTERY);
			gameManager.playerStats.addItem(this);
		}
		Destroy(this.gameObject);
	}
}