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

	public LayerMask interactLayer;

	private void Start()
	{
		inventory = new PlayerInventory();
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
			interactWorld();
			player.pickupItem();
		}
		lastInteractInput = _interactInput;
	}

	void interactWorld()
	{
		Collider2D[] hitTargets = Physics2D.OverlapCircleAll(this.transform.position, ActorDefs.GLOBAL_PICKUP_RANGE, this.interactLayer);

		foreach (Collider2D target in hitTargets)
		{
			AutoDoor doorInteract = target.transform.parent.gameObject.GetComponent<AutoDoor>();
			if (doorInteract != null)
			{
				if (inventory.keycardCount[(int)doorInteract.lockType] > 0)
				{
					inventory.keycardCount[(int)doorInteract.lockType] --;
					doorInteract.doorUnlock();
				}
			}
		}
	}
}