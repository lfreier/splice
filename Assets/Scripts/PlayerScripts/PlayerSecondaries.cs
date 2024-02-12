using System.Collections;
using UnityEngine;
	
public class PlayerSecondaries : MonoBehaviour
{
	public Actor player;

	public PlayerInputs inputs;

	private float _throwInput;
	private float lastThrowInput;

	private Vector2 _pointerPos;

	// Update is called once per frame
	void Update()
	{
		throwInputs();
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
}