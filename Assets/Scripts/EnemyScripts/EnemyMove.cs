using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static ActorDefs;

/* Includes logic for basic enemy movement.
 * Also has to interact with 'AI' for detection and where to move.
 */
public class EnemyMove : MonoBehaviour
{
	public Actor actor;
	public Rigidbody2D actorBody;

	public float moveTargetError;

	public Vector2[] idlePath;
	public float[] idlePathPauseTime;
	private float idlePauseTimer;
	private int pathIndex;

	private Vector2 idleLookTarget;
	private Vector2 eyeStart;

	public detectMode _detection;
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

	private float stateTimer;
	private static float HOSTILE_TIMER_LENGTH = 4;
	private static float LOST_TIMER_LENGTH = 2;
	private static float SUS_TIMER_LENGTH = 5;

	public GameManager gameManager;

	void Start()
	{
		gameManager = GameManager.Instance;
		_detection = detectMode.idle;
		attackTargetActor = null;
		pathIndex = 0;
		idlePauseTimer = 0;
		idleLookTarget = transform.position + transform.up * 0.5F;
	}

	void FixedUpdate()
	{
		_actorData = actor.actorData;
		attackTargetActor = null;
		eyeStart = transform.position + transform.up * 0.5F;
		_oldDetection = _detection;

		//functions for noticing hostiles
		/* IMPORTANT NOTE: 
		 *
		 * attackTargetActor is going to be null if the target can no longer be detected, e.g. the player moves out of sight range
		 * Then the NPC should move to the last known location
		 */
		hearHostiles();
		attackTargetActor = seeHostiles();

		/*  determine move target based on current state */
		if (_detection == detectMode.idle && idlePath.Length > 0)
		{
			/* If at a path position, go to next */
			if (((Vector2)actorBody.transform.position -  idlePath[pathIndex]).magnitude < moveTargetError)
			{
				if (pathIndex < idlePathPauseTime.Length)
				{
					idlePauseTimer = idlePathPauseTime[pathIndex];
				}

				pathIndex++;
				/* reset at at the end of the array */
				if (pathIndex >= idlePath.Length)
				{
					pathIndex = 0;
				}
			}
			
			if (idlePauseTimer <= 0)
			{
				moveTarget = idlePath[pathIndex];
				actorBody.rotation = actor.aimAngle(moveTarget);
			}
			else
			{
				idlePauseTimer -= Time.deltaTime;
			}
		}
		else if (_detection != detectMode.idle)
		{
			/* first, pickup a weapon if needed */
			if (actor.isUnarmed())
			{
				handleUnarmedStates();
			}
			/* Armed - perform state logic */
			else
			{
				handleArmedStates();
			}
		}
		else
		{
			moveTarget = actor.transform.position;
		}

		if (_detection == detectMode.frightened)
		{
			if (attackTargetActor != null)
			{
				/* face target of fright */
				actorBody.rotation = actor.aimAngle(attackTargetActor.transform.position);

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

		/* Move to last known location if hostile is not detected */
		//TODO

		handleEdges();

		/*  determine move speed based on current state */
		moveUpdate();
	}

	void moveUpdate()
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

		actor.Move(new Vector3(oldMoveInput.x * currentSpeed, oldMoveInput.y * currentSpeed));
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

	/* TODO: A* pathfinding implementation */
	private Vector2 findNextInput(Vector2 target)
	{
		//float gCost, hCost, Fcost;
		return target;
	}

	private Collider2D findNearestWeapon(float withinRange)
	{
		RaycastHit2D[] noticedWeapons = Physics2D.CircleCastAll(new Vector2(this.transform.position.x, this.transform.position.y), withinRange, Vector2.zero, withinRange, actor.pickupLayer);
		Collider2D closest = null;
		float closestDistance = withinRange + 1;
		foreach (RaycastHit2D target in noticedWeapons)
		{
			/* Check for wall collisions */
			Vector2 targetDir = (target.collider.transform.position - transform.position).normalized;
			RaycastHit2D rayHit = Physics2D.Raycast(transform.position, targetDir, withinRange, gameManager.findWeaponLayers);

			if (rayHit.collider != target.collider)
			{
				continue;
			}

			float currDistance = Vector3.Distance(actor.transform.position, target.transform.position);
			if (WeaponDefs.canWeaponBePickedUp(target.transform.gameObject) && closestDistance > currDistance)
			{
				closestDistance = currDistance;
				closest = target.collider;
			}
		}

		return closest;
	}
	private void handleEdges()
	{
		if (!(_oldDetection != _detection || maxStateSpeed == 0))
		{
			return;
		}

		switch (_oldDetection)
		{
			case detectMode.getWeapon:
				if (actor.isUnarmed() && findNearestWeapon(_actorData.sightRange) != null)
				{
					_detection = detectMode.getWeapon;
				}
				return;
			default:
				break;
		}
		switch (_detection)
		{
			case detectMode.idle:
				actorBody.rotation = actor.aimAngle(idleLookTarget);
				break;
			case detectMode.hostile:
				_actorData.sightAngle = actor._actorScriptable.sightAngle * 1.5F;
				stateTimer = HOSTILE_TIMER_LENGTH;
				break;
			case detectMode.seeking:
				_actorData.sightAngle = actor._actorScriptable.sightAngle * 1.5F;
				break;
			case detectMode.lost:
				_actorData.sightAngle = actor._actorScriptable.sightAngle * 1.5F;
				moveTarget = transform.position + transform.up;
				stateTimer = LOST_TIMER_LENGTH;
				break;
			case detectMode.suspicious:
				_actorData.sightAngle = actor._actorScriptable.sightAngle * 1.5F;
				stateTimer = SUS_TIMER_LENGTH;
				break;
			default:
				_actorData.sightAngle = actor._actorScriptable.sightAngle;
				stateTimer = 0;
				break;
		}

		stateSpeedIncrease = _actorData.acceleration * _actorData.moveSpeed;
		maxStateSpeed = _actorData.maxSpeed;

		setStateMoveSpeed();
	}

	private void handleArmedStates()
	{
		switch (_detection)
		{
			case detectMode.hostile:
				moveTarget = attackTarget;
				if (stateTimer <= 0)
				{
					_detection = detectMode.seeking;
					stateTimer = HOSTILE_TIMER_LENGTH;
				}
				else
				{
					stateTimer -= Time.deltaTime;
				}
				actorBody.rotation = actor.aimAngle(moveTarget);
				break;
			case detectMode.seeking:
				/* Keep moving in the direction of the last known location if it's reached */
				if ((moveTarget - transform.position).magnitude < 0.5F)
				{
					_detection = detectMode.lost;
				}
				actorBody.rotation = actor.aimAngle(attackTarget);
				break;
			case detectMode.lost:
				/* Move in a random direction */
				if (stateTimer <= 0)
				{
					float radAngle = Random.Range(0F, 2F * Mathf.PI);
					float temp = Random.Range(2F, 4F);
					moveTarget = transform.position + new Vector3(temp * Mathf.Sin(radAngle), temp * Mathf.Cos(radAngle), 0);
					actorBody.rotation = actor.aimAngle(moveTarget);
					stateTimer = LOST_TIMER_LENGTH;
				}
				else
				{
					stateTimer -= Time.deltaTime;
				}
				break;
			case detectMode.suspicious:
				handleSuspiciousState();
				break;
			default:
				moveTarget = actor.transform.position;
				break;
		}
	}

	private void handleUnarmedStates()
	{
		switch (_detection)
		{
			case detectMode.idle:
				break;
			case detectMode.frightened:
			case detectMode.getWeapon:
			case detectMode.hostile:
			case detectMode.seeking:
			case detectMode.lost:
				Collider2D weaponColl;
				/* Then, move to it */
				weaponColl = findNearestWeapon(_actorData.sightRange);
				if (weaponColl != null)
				{
					_detection = detectMode.getWeapon;
					moveTarget = weaponColl.transform.position;
					actorBody.rotation = actor.aimAngle(moveTarget);
				}
				else
				{
					_detection = detectMode.frightened;
					break;
				}

				if (Vector3.Magnitude(moveTarget - this.transform.position) <= ActorDefs.NPC_TRY_PICKUP_RANGE)
				{
					if (actor.pickup())
					{
						_detection = detectMode.hostile;
						/* face attack target */
						actorBody.rotation = actor.aimAngle(attackTarget);
					}
				}
				break;
			case detectMode.suspicious:
				handleSuspiciousState();
				break;
			default:
				break;
		}
	}

	private void handleSuspiciousState()
	{
		moveTarget = actor.transform.position;
		actorBody.rotation = actor.aimAngle(attackTarget);
		if (stateTimer <= 0)
		{
			_detection = detectMode.idle;
		}
		else
		{
			stateTimer -= Time.deltaTime;
		}
	}

	private void hearHostiles()
	{
		RaycastHit2D[] heardSound = Physics2D.CircleCastAll(new Vector2(this.transform.position.x, this.transform.position.y), _actorData.hearingRange, Vector2.zero, _actorData.hearingRange, gameManager.soundLayer);

		/* States that should override hearing */
		if (_detection == detectMode.getWeapon || _detection == detectMode.hostile || heardSound.Length == 0)
		{
			return;
		}

		/* by default, handle the edge case since a new sound was heard */
		_oldDetection = detectMode.idle;

		float loudest = 0;
		foreach (RaycastHit2D target in heardSound)
		{
			Sound soundScript = target.transform.gameObject.GetComponent<Sound>();
			Actor targetActor = target.transform.gameObject.GetComponent<Actor>();
			/*
			if (targetActor != null && actor.isTargetHostile(targetActor))
			{
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
			*/
			if (soundScript != null && soundScript.scriptable.volume >= loudest)
			{
				attackTarget = target.transform.position;
				loudest = soundScript.scriptable.volume;

				switch (soundScript.scriptable.type)
				{
					case SoundDefs.SoundType.CLANG:
						if (_detection != detectMode.hostile && _detection != detectMode.getWeapon && _detection != detectMode.frightened)
						{
							_detection = detectMode.seeking;
						}
						break;
					case SoundDefs.SoundType.THUD:
					case SoundDefs.SoundType.TAP:
					default:
						if (_detection == detectMode.lost || _detection == detectMode.seeking || _detection == detectMode.hostile)
						{
							_detection = detectMode.seeking;
						}
						else
						{
							_detection = detectMode.suspicious;
							handleEdges();
						}
						break;
				}
			}
		}
	}

	private Actor handleSightRays(Vector2 center, float maxLength, float increment, float sightRange, float index)
	{
		Vector2 viewRay = center - (Vector2)((maxLength - (index * increment)) * transform.right);
		RaycastHit2D rayHit = Physics2D.Raycast(eyeStart, viewRay, sightRange, gameManager.lineOfSightLayers);
		Debug.DrawRay(eyeStart, Vector2.ClampMagnitude(viewRay, 1) * sightRange);

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

		return null;
	}

	/* IMPORTANT NOTE
	 * Math is wrong here, but it works enough. Angles are basically fantasy land and not indicitive of reality.
	 */
	private Actor seeHostiles()
	{
		Vector2 center = transform.up * _actorData.sightRange;
		float degrees = _actorData.sightRange > 12F ? 2.5F : 5F;
		float increment = Mathf.Abs((float)Mathf.Tan(degrees * Mathf.PI / 180) * _actorData.sightRange);
		float numIncrements = _actorData.sightAngle / degrees;
		float maxLength = numIncrements * increment / 2;

		for (float i= 0; i < numIncrements + 1; i ++)
		{
			Actor returnActor = handleSightRays(center, maxLength, increment, _actorData.sightRange, i);
			if (returnActor != null)
			{
				return returnActor;
			}
		}

		/* Extra peripheral vision */
		/* This doesn't work how I thought, but it's fine */
		float peripheralRange = _actorData.sightRange / 2;
		float peripheralMult = 5;
		/* 100 angle - 2 extra rays, 140 angle - 3 extra,  180 angle = 4 extra */
		int peripheralRayCount = ((Mathf.RoundToInt(_actorData.sightAngle) - 100) / 40) + 2;
		for (float i = -(peripheralRayCount * peripheralMult); i < 0; i += peripheralMult)
		{
			Actor returnActor = handleSightRays(center, maxLength, increment, peripheralRange, i);
			if (returnActor != null)
			{
				return returnActor;
			}
		}
		for (float i = numIncrements + 1 + peripheralMult; i < numIncrements + 1 + peripheralRayCount + (peripheralRayCount * peripheralMult); i += peripheralMult)
		{
			Actor returnActor = handleSightRays(center, maxLength, increment, peripheralRange, i);
			if (returnActor != null)
			{
				return returnActor;
			}
		}

		return null;
	}

	private void setStateMoveSpeed()
	{
		switch (_detection)
		{
			case detectMode.idle:
				stateSpeedIncrease /= 2;
				maxStateSpeed /= 2;
				break;
			case detectMode.seeking:
			case detectMode.suspicious:
				stateSpeedIncrease /= 1.5F;
				maxStateSpeed /= 1.5F;
				break;
			case detectMode.frightened:
			case detectMode.hostile:
			default:
				break;
		}
	}
}
