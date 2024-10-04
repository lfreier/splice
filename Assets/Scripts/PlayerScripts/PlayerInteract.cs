using System.Collections;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
	private float _interactInput;
	private float lastInteractInput;

	public short numActions = MutationDefs.MAX_SLOTS;

	public Actor player;

	public PlayerInputs inputs;
	public PlayerInventory inventory;

	private GameObject mutateHolder;

	public LayerMask interactLayer;

	private void Start()
	{
		inventory = new PlayerInventory();
		mutateHolder = player.mutationHolder;
		lastInteractInput = 0;
	}
	void Update()
	{
		interactInputs();
	}

	void interactInputs()
	{
		_interactInput = inputs.interactInput();
		if (_interactInput > 0 && lastInteractInput == 0)
		{
			//if unlocking a door, do not pick up weapons;
			if (!interactWorld())
			{
				player.pickupItem();
			}
		}
		lastInteractInput = _interactInput;
	}

	bool interactWorld()
	{
		Collider2D[] hitTargets = Physics2D.OverlapCircleAll(this.transform.position, ActorDefs.GLOBAL_PICKUP_RANGE, this.interactLayer);

		foreach (Collider2D target in hitTargets)
		{
			if (MutationDefs.isMutationSelect(target.gameObject))
			{
				//TODO: implement mutation selection
				MutationSelect mutateSelect = target.gameObject.GetComponent<MutationSelect>();
				if (mutateSelect != null)
				{
					mutateSelect.activateSelectMenu(this);
					return true;
				}
			}

			AutoDoor doorInteract = target.transform.parent.gameObject.GetComponent<AutoDoor>();
			if (doorInteract != null)
			{
				if (inventory.keycardCount[(int)doorInteract.lockType] > 0)
				{
					inventory.keycardCount[(int)doorInteract.lockType] --;
					doorInteract.doorUnlock();
					return true;
				}
			}
		}
		return false;
	}

	public void equipMutation(MutationInterface mut)
	{
		if (mut != null)
		{
			var existingMuts = mutateHolder.GetComponents(mut.GetType());
			if (existingMuts.Length > 0)
			{
				/* the mutation already exists - return */
				return;
			}
			MutationInterface newMut = (MutationInterface)mutateHolder.AddComponent(mut.GetType());
			if (null != (newMut = newMut.mEquip(player)))
			{
				if (newMut.getMutationType() == mutationTrigger.ACTIVE_SLOT)
				{
					if (player.activeSlots[0] == null)
					{
						player.activeSlots[0] = newMut;
						return;
					}
				}
			}
		}
	}
}