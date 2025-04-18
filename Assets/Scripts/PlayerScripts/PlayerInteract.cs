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

	public LayerMask interactLayer;

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
			//if unlocking a door, do not pick up weapons;
			if (!interactWorld())
			{
				if (player.pickup(new Collider2D[]{ highlightedCollider}))
				{
					highlightedCollider = null;
					highlightList.Remove(highlightedCollider);
				}
			}
			else
			{
				//disable highlight?
			}
		}
		lastInteractInput = _interactInput;
	}

	bool interactWorld()
	{
		Collider2D[] hitTargets = Physics2D.OverlapCircleAll(interactCollider.transform.position, interactCollider.radius, this.interactLayer);

		foreach (Collider2D target in hitTargets)
		{
			if (MutationDefs.isMutationSelect(target.gameObject))
			{
				//TODO: implement mutation selection
				MutationSelect mutateSelect = target.gameObject.GetComponent<MutationSelect>();
				if (mutateSelect != null && !mutateSelect.isActivated)
				{
					mutateSelect.activateSelectMenu(this);
					return true;
				}
			}

			UsableInterface usable = target.transform.GetComponentInParent<UsableInterface>();
			if (usable != null)
			{
				usable.use(player);
				return true;
			}

			AutoDoor doorInteract = target.transform.GetComponentInParent<AutoDoor>();
			if (doorInteract != null)
			{
				PlayerStats stats = player.gameManager.playerStats;
				if (stats.keycardCount[(int)doorInteract.lockType] > 0
					&& doorInteract._doorType != AutoDoor.doorType.REMOTE 
					&& doorInteract.locked)
				{
						doorInteract.doorUnlock();
						stats.useKeycard((int)doorInteract.lockType);
						return true;
				}
			}
		}
		return false;
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
				if (diffMag > 2 * interactCollider.radius || coll.tag.Equals(WeaponDefs.EQUIPPED_WEAPON_TAG))
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