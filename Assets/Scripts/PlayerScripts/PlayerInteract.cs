using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerInteract : MonoBehaviour
{
	private float _interactInput;
	private float lastInteractInput;

	public short numActions = MutationDefs.MAX_SLOTS;

	public Actor player;

	public PlayerInputs inputs;

	public CircleCollider2D interactCollider;
	public List<Collider2D> highlightList;
	public Collider2D highlightedCollider = null;

	private void Start()
	{
		lastInteractInput = 0;
	}
	void Update()
	{
		interactInputs();
		/*
		Collider2D[] hitTargets1 = Physics2D.OverlapCircleAll(this.transform.position + (transform.up * ActorDefs.GLOBAL_PICKUP_OFFSET), ActorDefs.GLOBAL_PICKUP_RADIUS, player.pickupLayer);
		Collider2D[] hitTargets2 = Physics2D.OverlapCircleAll(this.transform.position, ActorDefs.GLOBAL_PICKUP_RADIUS, player.pickupLayer);
		Collider2D[] hitTargets = new Collider2D[hitTargets1.Length + hitTargets2.Length];
		hitTargets1.CopyTo(hitTargets, 0);
		hitTargets2.CopyTo(hitTargets, hitTargets1.Length);
		*/
	}

	void interactInputs()
	{
		_interactInput = inputs.interactInput();
		if (_interactInput > 0 && lastInteractInput == 0)
		{
			Collider2D[] currColl = new Collider2D[] { highlightedCollider };
			//if unlocking a door, do not pick up weapons;
			if (!interactWorld(currColl))
			{
				if (player.pickup(currColl))
				{
					highlightedCollider = null;
					highlightList.Remove(highlightedCollider);
				}
			}
			else
			{
				highlightedCollider = null;
				highlightList.Remove(highlightedCollider);
			}
		}
		lastInteractInput = _interactInput;
	}

	bool interactWorld(Collider2D[] highlighted)
	{
		foreach (Collider2D target in highlighted)
		{
			bool ret = false;
			if (target == null)
			{
				continue;
			}
			if (MutationDefs.isMutationSelect(target.gameObject))
			{
				//TODO: implement mutation selection
				MutationSelect mutateSelect = target.gameObject.GetComponent<MutationSelect>();
				if (mutateSelect != null && !mutateSelect.isActivated)
				{
					mutateSelect.activateSelectMenu(this);
					ret = true;
				}
			}

			UsableInterface usable = target.transform.GetComponentInParent<UsableInterface>();
			if (usable != null)
			{
				if (usable.use(player))
				{
					ret = true;
				}
			}
			if (ret)
			{
				PickupEngine engine = target.gameObject.GetComponent<PickupEngine>();
				if (engine != null)
				{
					engine.disableHighlight();
				}
				return true;
			}
		}
		return false;
	}
	private bool doorShouldHighlight(AutoDoor door)
	{
		return !(door != null && (!door.locked || (door.locked && !door.playerHasKey(player.gameManager.playerStats))));
	}

	public void equipMutation(MutationInterface mut)
	{
		player.gameManager.playerStats.equipMutation(mut);
	}

	private void updateHighlightedItem()
	{
		float min = 500;
		Collider2D closest = null;

		if (highlightList.Count <= 0)
		{
			highlightedCollider = null;
			return;
		}

		//highlight the one to pick up
		foreach (Collider2D coll in highlightList)
		{
			if (coll != null)
			{
				float diffMag = Mathf.Abs(((Vector2)player.transform.position - coll.ClosestPoint(interactCollider.transform.position)).magnitude);
				if (diffMag <= min)
				{
					min = diffMag;
					closest = coll;
				}
				AutoDoor door = coll.gameObject.GetComponentInParent<AutoDoor>();
				if (diffMag > 2 * interactCollider.radius || coll.tag.Equals(WeaponDefs.EQUIPPED_WEAPON_TAG)
					|| !doorShouldHighlight(door))
					{
					PickupEngine engine = coll.gameObject.GetComponent<PickupEngine>();
					if (engine != null)
					{
						//disable highlight
						engine.disableHighlight();
					}
					highlightList.Remove(coll);
					updateHighlightedItem();
					return;
				}
			}
			else
			{
				highlightList.Remove(coll);
				updateHighlightedItem();
				return;
			}
		}

		if (highlightList.Count <= 0)
		{
			highlightedCollider = null;
			return;
		}

		foreach (Collider2D coll in highlightList)
		{
			PickupEngine engine = coll.GetComponent<PickupEngine>();
			if (engine != null)
			{
				if (coll == closest)
				{
					//enable highlight
					engine.enableHighlight();
					highlightedCollider = coll;
				}
				else
				{
					//disable highlight
					engine.disableHighlight();
				}
			}
		}
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision != null && !collision.tag.Equals(WeaponDefs.EQUIPPED_WEAPON_TAG))
		{
			AutoDoor door = collision.gameObject.GetComponentInParent<AutoDoor>();
			if (!doorShouldHighlight(door))
			{
				return;
			}
			if (!highlightList.Contains(collision))
			{
				highlightList.Add(collision);
			}
			updateHighlightedItem();
		}
	}

	public void OnTriggerStay2D(Collider2D collision)
	{
		if (collision != null && !collision.tag.Equals(WeaponDefs.EQUIPPED_WEAPON_TAG))
		{
			AutoDoor door = collision.gameObject.GetComponentInParent<AutoDoor>();
			if (!doorShouldHighlight(door))
			{
				return;
			}
			if (!highlightList.Contains(collision))
			{
				highlightList.Add(collision);
			}
			updateHighlightedItem();
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		if (collision != null && !collision.tag.Equals(WeaponDefs.EQUIPPED_WEAPON_TAG))
		{
			AutoDoor door = collision.gameObject.GetComponentInParent<AutoDoor>();
			if (!doorShouldHighlight(door))
			{
				return;
			}
			PickupEngine engine = collision.gameObject.GetComponent<PickupEngine>();
			if (engine != null)
			{
				//disable highlight
				engine.disableHighlight();
			}
			
			highlightList.Remove(collision);
			updateHighlightedItem();
		}
	}
}