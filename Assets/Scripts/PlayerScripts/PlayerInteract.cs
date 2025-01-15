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

			UsableInterface usable = target.transform.GetComponentInParent<UsableInterface>();
			if (usable != null)
			{
				usable.use(player);
			}

			AutoDoor doorInteract = target.transform.GetComponentInParent<AutoDoor>();
			if (doorInteract != null)
			{
				PlayerStats stats = player.gameManager.playerStats;
				if (stats.keycardCount[(int)doorInteract.lockType] > 0)
				{
					if (doorInteract._doorType != AutoDoor.doorType.REMOTE)
					{
						doorInteract.doorUnlock();
						stats.useKeycard((int)doorInteract.lockType);
						return true;
					}
				}
			}
		}
		return false;
	}

	public void equipMutation(MutationInterface mut)
	{
		player.gameManager.playerStats.equipMutation(mut);
	}
}