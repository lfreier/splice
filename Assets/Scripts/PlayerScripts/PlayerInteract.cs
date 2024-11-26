using System.Collections;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
	private float _interactInput;
	private float lastInteractInput;

	public short numActions = MutationDefs.MAX_SLOTS;

	public Actor player;

	public PlayerInputs inputs;

	private GameObject mutateHolder;

	public LayerMask interactLayer;

	private void Start()
	{
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
				player.pickup();
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

			AutoDoor doorInteract = target.transform.GetComponentInParent<AutoDoor>();
			if (doorInteract != null)
			{
				PlayerStats stats = player.gameManager.playerStats;
				if (stats.keycardCount[(int)doorInteract.lockType] > 0)
				{
					stats.useKeycard((int)doorInteract.lockType);
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

			GameObject mutPrefab;
			switch(mut.getId())
			{
				case "MBeast":
					mutPrefab = player.instantiateActive(player.gameManager.mutPBeast);
					break;
				case "MBladeWing":
					mutPrefab = player.instantiateActive(player.gameManager.mutPBladeWing);
					break;
				case "MLimb":
					mutPrefab = player.instantiateActive(player.gameManager.mutPLimb);
					break;
				default:
					return;
			}

			MutationInterface newMut = (MutationInterface)mutPrefab.GetComponentInChildren(mut.GetType());
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