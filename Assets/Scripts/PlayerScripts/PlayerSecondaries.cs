using System.Collections;
using UnityEngine;
	
public class PlayerSecondaries : MonoBehaviour
{
	public Actor player;

	public PlayerInputs inputs;

	private float _throwInput;
	private float lastThrowInput;

	// Update is called once per frame
	void Update()
	{
		throwInputs();
	}

	void throwInputs()
	{
		_throwInput = inputs.throwInput();
		if (_throwInput > 0 && lastThrowInput == 0)
		{
			player.throwWeapon();
		}

		lastThrowInput = _throwInput;
	}
}