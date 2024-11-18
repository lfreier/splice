using System.Collections;
using UnityEngine;

public class Keycard : MonoBehaviour, PickupInterface
{
	public PickupDefs.keycardType _keycardType;
	public SpriteRenderer keycardColor;
	public int keycardUses;
	private GameManager gameManager;

	void Start()
	{
		 gameManager = GameManager.Instance;
		init();
	}

	public int getCount()
	{
		return 1;
	}

	public void init()
	{
		keycardColor.color = PickupDefs.getKeycardColor(_keycardType);
	}

	public PickupDefs.pickupType getPickupType()
	{
		return PickupDefs.pickupType.KEYCARD;
	}

	public void pickup(Actor actorTarget)
	{
		if (gameManager != null)
		{
			gameManager.playerStats.addItem(this);
		}
		Destroy(this.gameObject);
	}
}