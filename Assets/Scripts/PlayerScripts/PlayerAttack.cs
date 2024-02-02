using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
	private float speedCheck;

	public Actor player;
	public PlayerInputs inputs;

	// Use this for initialization
	void Start()
	{
		speedCheck = 0;
	}

	// Update is called once per frame
	void Update()
	{
		attackActions();
	}
	void attackActions()
	{
		if (speedCheck > 0)
		{
			speedCheck -= Time.deltaTime;
		}

		/* Don't give an input while an animation is playing */
		if (inputs.attackInput() > 0 && speedCheck <= 0)
		{
			player.attack();
		}
	}
}