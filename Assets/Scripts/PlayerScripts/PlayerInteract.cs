using System.Collections;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
	private float _interactInput;
	private float lastInteractInput;

	public Actor player;

	public PlayerInputs inputs;

	private void Start()
	{
		lastInteractInput = 0;
	}
	void Update()
	{
		InteractInputs();
	}

	void InteractInputs()
	{
		_interactInput = inputs.interactInput();
		if (_interactInput > 0 && lastInteractInput == 0)
		{
			player.pickupItem();
		}
		lastInteractInput = _interactInput;
	}
}