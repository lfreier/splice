using System.Collections;
using UnityEngine;

public class Keycard : MonoBehaviour, PickupInterface
{
	public PickupDefs.keycardType _keycardType;
	public SpriteRenderer keycardColor;
	public int keycardUses;

	void Start()
	{
		init();
	}

	public int getCount()
	{
		return 1;
	}

	public void init()
	{
		switch (_keycardType)
		{
			case PickupDefs.keycardType.BLUE:
				keycardColor.color = GameManager.COLOR_BLUE;
				break;
			case PickupDefs.keycardType.RED:
			default:
				keycardColor.color = GameManager.COLOR_RED;
				break;
		}
	}

	public PickupDefs.pickupType getPickupType()
	{
		return PickupDefs.pickupType.KEYCARD;
	}

	public void pickup(Actor actorTarget)
	{
		PlayerInteract interact = actorTarget.gameObject.GetComponent<PlayerInteract>();
		if (interact != null)
		{
			interact.inventory.addItem(this);
			Destroy(this.gameObject);
		}
	}
}