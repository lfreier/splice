using System.Collections;
using UnityEngine;
	
public class PlayerSecondaries : MonoBehaviour
{
	public Actor player;

	public PlayerInputs inputs;
	public PlayerInteract playerInteract;

	private float _throwInput;
	private float lastThrowInput;

	private float[] _specialActions = new float[MutationDefs.MAX_SLOTS];
	private float[] _lastSpecialActions = new float[MutationDefs.MAX_SLOTS];

	private Vector2 _pointerPos;

	// Update is called once per frame
	void Update()
	{
		throwInputs();
		//activeInputs();
	}

	void throwInputs()
	{
		_throwInput = inputs.throwInput();
		_pointerPos = inputs.pointerPos();
		if (_throwInput > 0 && lastThrowInput == 0)
		{
			PickupEngine engine = player.getEquippedWeapon().GetComponentInChildren<PickupEngine>();
			
			player.throwWeapon(new Vector3(_pointerPos.x, _pointerPos.y, 0));
			
			if (engine != null)
			{
				Collider2D pickupColl = engine.GetComponent<Collider2D>();
				if (pickupColl != null)
				{
					playerInteract.OnTriggerExit2D(pickupColl);
				}
			}
		}

		lastThrowInput = _throwInput;
	}

	void activeInputs()
	{
		_specialActions = inputs.actionInputs();

		short i = 0;
		foreach (var action in _specialActions)
		{
			_lastSpecialActions[i] = action;
			i++;
		}
	}
}