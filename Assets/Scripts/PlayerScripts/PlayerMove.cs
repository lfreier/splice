using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using static ActorDefs;

public class PlayerMove : MonoBehaviour
{
	private float currentSpeed;

	private ActorData playerData;

	public Actor player;

	private Vector2 moveInput;
	private Vector2 lastMoveInput;

	private float walkInput;
	private float lastWalkInput;

	private float footstepTimer;
	private float soundTimer;
	private bool firstStep = false;

	private bool createSoundNext = false;
	private Vector2 nextPosition = Vector2.zero;

	private bool movementLocked;

	public PlayerInputs inputs;

	public SoundScriptable footstepScriptable;

	private void OnDestroy()
	{
		GameManager gameManager = GameManager.Instance;
		if (gameManager == null)
		{
			return;
		}
		gameManager.movementLockedEvent -= lockMovement;
		gameManager.movementUnlockedEvent -= unlockMovement;
	}

	private void Start()
	{
		playerData = player.actorData;
		GameManager gameManager = GameManager.Instance;
		if (gameManager == null)
		{
			Debug.LogError("Game Manager is null when it should not be");
			return;
		}
		gameManager.movementLockedEvent += lockMovement;
		gameManager.movementUnlockedEvent += unlockMovement;
		movementLocked = false;
		createSoundNext = false;
		currentSpeed = 0;
	}

	private void Update()
	{
		moveInput = inputs.moveInput();
		walkInput = inputs.walkInput();
	}

	private void FixedUpdate()
	{
		// move
		if (moveInput.magnitude > 0 && !movementLocked)
		{
			playerData = player.actorData;
			if (walkInput > 0 && lastWalkInput <= 0)
			{
				player.setSpeed(ActorDefs.PLAYER_WALK_SPEED * playerData.maxSpeed);
			}
			else if (lastWalkInput > 0 && walkInput <= 0)
			{
				player.setSpeed(playerData.maxSpeed / ActorDefs.PLAYER_WALK_SPEED);
			}
			lastWalkInput = walkInput;
			lastMoveInput = moveInput;
			currentSpeed += playerData.acceleration * playerData.moveSpeed;

			if (footstepScriptable != null && currentSpeed > footstepScriptable.triggerSpeed)
			{
				footstepTimer += currentSpeed * Time.deltaTime;
				soundTimer += Time.deltaTime;

				if (footstepTimer > footstepScriptable.threshold)
				{
					footstepTimer = 0;

					if (soundTimer > playerData.maxSpeed / 45 || firstStep)
					{
						soundTimer = 0;
						firstStep = false;

						/* play audio */
						AudioClip footstepClip;
						if (player.actorAudioSource != null && footstepScriptable.audioClip != null
							&& player.gameManager.audioManager.soundHash.TryGetValue(footstepScriptable.audioClip.name, out footstepClip))
						{
							player.actorAudioSource.PlayOneShot(footstepClip, (footstepScriptable.volume * player.gameManager.effectsVolume));
						}
					}

					createSoundNext = true;
					nextPosition = player.transform.position;
				}
			}
			else
			{
				footstepTimer = 0;
				soundTimer = 0;
				firstStep = true;
			}
		}
		else
		{
			currentSpeed -= playerData.deceleration * playerData.moveSpeed;
		}

		if (createSoundNext)
		{
			createSoundNext = false;
			SoundDefs.createSound(player.transform.position, footstepScriptable, null);
			nextPosition = Vector2.zero;
		}

		if (!movementLocked)
		{
			currentSpeed = Mathf.Clamp(currentSpeed, 0, playerData.maxSpeed);
			player.Move(new Vector3(lastMoveInput.x * currentSpeed, lastMoveInput.y * currentSpeed));
		}
	}

	private void lockMovement()
	{
		movementLocked = true;
	}

	private void unlockMovement()
	{
		movementLocked = false;
	}
}