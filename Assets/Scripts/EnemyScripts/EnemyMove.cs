using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

	public float moveTargetError;

	private Vector2 eyeStart;

	private detectMode _detection;
	private detectMode _oldDetection;

	float currentSpeed;
	private float stateSpeedIncrease;
	private float maxStateSpeed;

	/* moveTarget will always be where the actor moves. Takes priority over the actor's hostile target. */
	private Vector3 moveTarget;
	private Vector3 attackTarget;
	private Actor attackTargetActor;

	private Vector2 moveInput;
	private Vector2 oldMoveInput;

	private ActorData _actorData;

	public GameManager gameManager;

	void Start()
	{
		gameManager = GameManager.Instance;
		_detection = detectMode.idle;
		attackTargetActor = null;
	}

	void Update()
	{
		_actorData = actor.actorData;
		_oldDetection = _detection;
		attackTargetActor = null;
		eyeStart = transform.position + transform.up * 0.5F;

		//functions for noticing hostiles
		/* IMPORTANT NOTE: 
		 *
		 * attackTargetActor is going to be null if the target can no longer be detected, e.g. the player moves out of sight range
		 * Then the NPC should move to the last known location
		 */
		attackTargetActor = seeHostiles();
		attackTargetActor = attackTargetActor != null ? attackTargetActor : hearHostiles();

		/*  determine move speed based on current state */
		if (_oldDetection != _detection)
		{
			stateSpeedIncrease = _actorData.acceleration * _actorData.moveSpeed;
			maxStateSpeed = _actorData.maxSpeed;

			setStateMoveSpeed();
		}

		/*  determine move target based on current state */

		if (_detection != detectMode.idle)
		{
			/* first, pickup a weapon if needed */
			if (actor.isUnarmed())
			{
				Collider2D weaponColl;
				/* Then, move to it */
				weaponColl = findNearestWeapon(_actorData.sightRange);
				if (weaponColl != null)
				{
					_detection = detectMode.getWeapon;
					moveTarget = weaponColl.transform.position;
					actorBody.rotation = aimAngle(moveTarget);
				}
				else
				{
					_detection = detectMode.frightened;
				}
			}
			else
			{
				if (_detection == detectMode.hostile)
				{
					moveTarget = attackTarget;
					actorBody.rotation = aimAngle(moveTarget);
				}
				else if (_detection == detectMode.suspicious || _detection == detectMode.seeking)
				{
					actorBody.rotation = aimAngle(attackTarget);
				}
				else
				{
					moveTarget = actor.transform.position;
				}
			}
		}

		if (_detection == detectMode.getWeapon)
		{
			if (Vector3.Magnitude(moveTarget - this.transform.position) <= ActorDefs.GLOBAL_PICKUP_RANGE)
			{
				actor.pickupItem();
				_detection = detectMode.hostile;
				/* face attack target */
				actorBody.rotation = aimAngle(attackTarget);
			}
		}

		if (_detection == detectMode.frightened)
		{
			if (attackTargetActor != null)
			{
				/* face target of fright */
				actorBody.rotation = aimAngle(attackTargetActor.transform.position);

				/* move away from target */
				if (_actorData.frightenedDistance > (attackTargetActor.transform.position - actor.transform.position).magnitude)
				{
					Vector3 backupTarget = attackTargetActor.transform.position - (actor.transform.up * _actorData.frightenedDistance);
					Vector3 clamped = Vector3.ClampMagnitude(backupTarget - actor.transform.position, _actorData.frightenedDistance);
					moveTarget = clamped + actor.transform.position;
				}
				else
				{
					moveTarget = actor.transform.position;
				}
			}
			else
			{
				moveTarget = actor.transform.position;
			}
		}

		if (_detection == detectMode.idle)
		{
			moveTarget = actor.transform.position;
		}

		/* Move to last known location if hostile is not detected */

		/* Make sure to save to the actor */
		actor.detection = _detection;
		actor.oldDetection = _oldDetection;
	}

	void FixedUpdate()
	{
		calcMoveInput();

		if (moveInput.magnitude > 0)
		{
			oldMoveInput = moveInput;
			currentSpeed += stateSpeedIncrease;
		}
		else
		{
			currentSpeed -= _actorData.deceleration * _actorData.moveSpeed;
		}

		currentSpeed = Mathf.Clamp(currentSpeed, 0, maxStateSpeed);

		actor.Move(new Vector3(oldMoveInput.x * currentSpeed * Time.deltaTime, oldMoveInput.y * currentSpeed * Time.deltaTime));
	}

	private float aimAngle(Vector2 aimTarget)
	{
		return (Mathf.Atan2((aimTarget - (Vector2)actor.transform.position).y, (aimTarget - (Vector2)actor.transform.position).x) * Mathf.Rad2Deg) - 90F;
	}

	private void calcMoveInput()
	{
		if (moveTarget == null)
		{
			return;
		}

		Vector2 diff = moveTarget - this.transform.position;
		moveInput = Vector2.ClampMagnitude(diff, 1F); 
	}

	private Collider2D findNearestWeapon(float withinRange)
	{
		RaycastHit2D[] noticedWeapons = Physics2D.CircleCastAll(new Vector2(this.transform.position.x, this.transform.position.y), withinRange, Vector2.zero, withinRange, actor.pickupLayer);
		foreach (RaycastHit2D target in noticedWeapons)
		{
			if (WeaponDefs.canWeaponBePickedUp(target.transform.gameObject))
			{
				return target.collider;
			}
		}

		return null;
	}

	private Actor hearHostiles()
	{
		RaycastHit2D[] noticedActors = Physics2D.CircleCastAll(new Vector2(this.transform.position.x, this.transform.position.y), _actorData.hearingRange, Vector2.zero, _actorData.hearingRange, gameManager.actorLayers);

		foreach (RaycastHit2D target in noticedActors)
		{
			Actor targetActor = target.transform.gameObject.GetComponent<Actor>();
			if (targetActor != null && actor.isTargetHostile(targetActor))
			{
				/* If they can go through a door, go through it */
				RaycastHit2D rayHit = Physics2D.Raycast(eyeStart, targetActor.transform.position - transform.position, actor.actorData.sightRange, gameManager.lineOfSightLayers);
				//Debug.DrawRay(eyeStart, targetActor.transform.position - transform.position);
				if (rayHit.collider != null && rayHit.collider.gameObject.transform.parent != null)
				{
					AutoDoor door = rayHit.collider.gameObject.GetComponentInParent<AutoDoor>();
					if (door != null && !door.locked)
					{
						_detection = detectMode.seeking;
						moveTarget = targetActor.transform.position;
					}
				}

				if (_detection == detectMode.idle || _detection == detectMode.suspicious)
				{
					_detection = detectMode.suspicious;
				}
				else if (_detection == detectMode.hostile)
				{
					_detection = detectMode.hostile;
				}
				actor.setAttackTarget(targetActor);
				attackTarget = targetActor.transform.position;
				return targetActor;
			}
		}

		return null;
	}

	private Actor seeHostiles()
	{
		Vector2 center = transform.up * _actorData.sightRange;
		float degrees = _actorData.sightRange > 12F ? 2.5F : 5F;
		float increment = Math.Abs((float)Math.Tan(degrees * Math.PI / 180) * _actorData.sightRange);
		float numIncrements = _actorData.sightAngle / degrees;
		float maxLength = numIncrements * increment / 2;

		for (float i= 0; i < numIncrements + 1; i ++)
		{
			RaycastHit2D rayHit = Physics2D.Raycast(eyeStart, center - (Vector2)((maxLength - (i * increment)) * transform.right), actor.actorData.sightRange, gameManager.lineOfSightLayers);
			Debug.DrawRay(eyeStart, center - (Vector2)((maxLength - (i * increment)) * transform.right));

			if (rayHit.rigidbody != null)
			{
				Actor targetActor = rayHit.rigidbody.gameObject.GetComponent<Actor>();
				if (targetActor != null && actor.isTargetHostile(targetActor))
				{
					_detection = detectMode.hostile;
					actor.setAttackTarget(targetActor);
					attackTarget = targetActor.transform.position;
					return targetActor;
				}
			}
		}
		return null;
	}

	private void setStateMoveSpeed()
	{
		switch (_detection)
		{
			case detectMode.idle:
				stateSpeedIncrease /= 3;
				maxStateSpeed /= 3;
				break;
			case detectMode.seeking:
			case detectMode.suspicious:
				stateSpeedIncrease /= 2;
				maxStateSpeed /= 2;
				break;
			case detectMode.frightened:
			case detectMode.hostile:
			default:
				break;
		}
	}
}
