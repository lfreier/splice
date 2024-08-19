using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using static Actor;

public class PlayerMove : MonoBehaviour
{
	private float currentSpeed;

	private ActorData playerData;

	public Actor player;

	private Vector2 moveInput;
	private Vector2 lastMoveInput;

	private float walkInput;

	private float footstepTimer;

	public PlayerInputs inputs;

	private SoundScriptable _scriptable;

	private void Start()
	{
		playerData = player.actorData;
		currentSpeed = 0;
	}

	private void Update()
	{
		playerData = player.actorData;
		moveInput = inputs.moveInput();
		walkInput = inputs.walkInput();

		if (_scriptable == null && player.gameManager != null)
		{
			_scriptable = player.gameManager.getSoundScriptable(SoundDefs.SOUND_FOOTSTEP);
		}
	}

	private void FixedUpdate()
	{
		// move
		if (moveInput.magnitude > 0)
		{
			if (walkInput > 0)
			{
				player.setSpeed(ActorDefs.PLAYER_WALK_SPEED * player._actorScriptable.maxSpeed);
			}
			else
			{
				player.setSpeed(player._actorScriptable.maxSpeed);
			}
			lastMoveInput = moveInput;
			currentSpeed += playerData.acceleration * playerData.moveSpeed;

			if (_scriptable != null && currentSpeed > _scriptable.triggerSpeed)
			{
				footstepTimer += currentSpeed * Time.deltaTime;

				if (footstepTimer > _scriptable.threshold)
				{
					SoundDefs.createSound(player.transform.position, _scriptable);
					footstepTimer = 0;
				}
			}
			else
			{
				footstepTimer = 0;
			}
		}
		else
		{
			currentSpeed -= playerData.deceleration * playerData.moveSpeed;
		}

		currentSpeed = Mathf.Clamp(currentSpeed, 0, playerData.maxSpeed);
		player.Move(new Vector3(lastMoveInput.x * currentSpeed * Time.deltaTime, lastMoveInput.y * currentSpeed * Time.deltaTime));
	}
}