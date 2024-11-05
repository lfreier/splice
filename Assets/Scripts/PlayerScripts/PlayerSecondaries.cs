using System.Collections;
using UnityEngine;
	
public class PlayerSecondaries : MonoBehaviour
{
	public Actor player;

	public PlayerInputs inputs;

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
			player.throwWeapon(new Vector3(_pointerPos.x, _pointerPos.y, 0));
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