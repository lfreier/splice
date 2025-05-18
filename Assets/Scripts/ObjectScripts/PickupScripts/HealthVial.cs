using System.Collections;
using UnityEngine;

public class HealthVial : MonoBehaviour, PickupInterface
{
	public int count = 1;
	public float healAmount = 0;

	[SerializeField]
	private Sprite icon;

	// Use this for initialization
	void Start()
	{
		PickupDefs.setLayer(gameObject);
	}
	public int getCount()
	{
		return count;
	}

	public Sprite getIcon()
	{
		return icon;
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