using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Actor;

/* Includes logic for basic enemy movement.
 * Also has to interact with 'AI' for detection and where to move.
 */
[RequireComponent(typeof(Controller2D))]
public class EnemyMove : MonoBehaviour
{
	public Actor actor;
	public Rigidbody2D actorBody;

	private detectMode _detection;
	private detectMode _oldDetection;

	float currentSpeed;
	private float stateSpeedIncrease;
	private float maxStateSpeed;

	private Vector2 moveTarget;

	private Vector2 moveInput;
	private Vector2 oldMoveInput;

	private ActorScriptable _actorData;

	void Start()
	{
		_detection = detectMode.idle;
		_actorData = actor.actorData;
	}

	void Update()
	{
		_oldDetection = _detection;
		
		//functions for noticing hostiles

		/*  determine move speed based on current state */
		if (_oldDetection != _detection)
		{
			stateSpeedIncrease = _actorData.acceleration * _actorData.moveSpeed;
			maxStateSpeed = _actorData.maxSpeed;

			switch (_detection)
			{
				case detectMode.idle:
					stateSpeedIncrease /= 3;
					maxStateSpeed /= 3;
					break;
				case detectMode.cautious:
				case detectMode.suspicious:
					stateSpeedIncrease /= 2;
					maxStateSpeed /= 2;
					break;
				case detectMode.frightened:
				case detectMode.hostile:
					break;
			}
		}

		/*  determine move target based on current state */
		switch (_detection)
		{
			case detectMode.idle:
				stateSpeedIncrease /= 3;
				maxStateSpeed /= 3;
				break;
			case detectMode.cautious:
			case detectMode.suspicious:
				stateSpeedIncrease /= 2;
				maxStateSpeed /= 2;
				break;
			case detectMode.frightened:
			case detectMode.hostile:
				break;
		}

		/* Make sure to save to the actor */
		actor.detection = _detection;
		actor.oldDetection = _oldDetection;

		/*
		if (detection == detectMode.hostile)
		{
			if (idleTimer > 0)
			{
				//when idle just ended
				if ((idleTimer -= Time.deltaTime) <= 0)
				{
					Vector2 aimDir = playerBody.position - actorBody.position;
					float aimAngle = (Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg) - 90F;
					float difference = Vector2.Distance(playerBody.position, actorBody.position);
					actorBody.rotation = aimAngle;
					destination = Vector2.MoveTowards(actorBody.position, playerBody.position, Random.Range(0.5F, difference));
					idleTimer = 0;
				}
			}

			if (idleTimer <= 0)
			{
				if (Vector2.Distance(actorBody.position, destination) < 0.1F)
				{
					//wait for a bit before moving again
					idleTimer = 1F;
					moveInput = new Vector2(0, 0);
				}
				else
				{
					Vector2 aimDir = destination - actorBody.position;
					float aimAngle = (Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg) - 90F;
					moveInput = Vector2.MoveTowards(actorBody.position, destination, 1F) - actorBody.position;
				}
			}
		}*/
	}

    void FixedUpdate()
	{
		if (moveInput.magnitude > 0)
		{
			oldMoveInput = moveInput;
			currentSpeed += stateSpeedIncrease;
		}
		else
		{
			currentSpeed -= _actorData.deceleration * _actorData.moveSpeed;
		}


		if (_detection != detectMode.frightened)
		{
			float aimAngle = (Mathf.Atan2(oldMoveInput.y, oldMoveInput.x) * Mathf.Rad2Deg) - 90F;
			actorBody.rotation = aimAngle;
		}

		currentSpeed = Mathf.Clamp(currentSpeed, 0, maxStateSpeed);

		actor.Move(new Vector3(oldMoveInput.x * currentSpeed * Time.deltaTime, oldMoveInput.y * currentSpeed * Time.deltaTime));
	}
}
