using System.Collections;
using UnityEngine;

public class Refill : MonoBehaviour, PickupInterface
{
	public int cellAmount = 0;

	[SerializeField]
	private Sprite icon;

	public int getCount()
	{
		return cellAmount;
	}

	public Sprite getIcon()
	{
		return icon;
	}

	public PickupDefs.pickupType getPickupType()
	{
		return PickupDefs.pickupType.PETRI_DISH;
	}

	public void init()
	{

	}

	public void pickup(Actor actorTarget)
	{
		GameManager gameManager = GameManager.Instance;
		if (gameManager != null)
		{
			/* only destroy if actually picked up */
			if (gameManager.playerStats.addItem(this))
			{
				Destroy(this.gameObject);
			}
		}
	}
}