using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
	public int pathIndex;

	public float wanderScale = 4F;

	private Vector2 idleLookTarget;
	private Vector2 eyeStart;

	public detectMode _detection;
	private detectMode _oldDetection;
	public detectMode _startingDetection;
	public detectMode _nextDetection;
	public detectMode _nextForcedDetection;

	float currentSpeed;
	private float stateSpeedIncrease;
	private float maxStateSpeed;

	public LockBehind lockWhenSpotted;

	/* moveTarget will always be where the actor moves. Takes priority over the actor's hostile target. */
	public Vector3 moveTarget;
	private Vector3 lastMoveTarget;
	private Vector3 attackTarget;
	private Actor attackTargetActor;

	private Vector2 moveInput;
	private Vector2 oldMoveInput;

	private ActorData _actorData;

	private float stateTimer;
	private float stateTimerReset;
	private static float HOSTILE_TIMER_LENGTH = 4;
	private static float LOST_TIMER_LENGTH = 2;
	private static float PATH_TIMER_LENGTH = 0.3F;
	private static float SUS_TIMER_LENGTH = 5;

	private float delayTimer;
	private static float DELAY_TIMER_LENGTH = 0.35F;

	public Pathfinding pathfinder;
	public bool usingPathfinding;

	private bool showNotice = true;
	private bool showSus = true;
	public float forcedTimer = 0;

	public bool summoned = false;

	public GameManager gameManager;

	void Start()
	{
		gameManager = GameManager.Instance;
		pathfinder = new Pathfinding();
		pathfinder.startPathfinding = false;
		usingPathfinding = false;
		LevelData levelData = gameManager.levelManager.currLevelData;
		//pathfinder.grid = new PathGrid();
		//pathfinder.grid.init(levelData.gridWorldSize, levelData.nodeRadius);

		PathColliderHelper colliderHelper = GetComponentInChildren<PathColliderHelper>();
		if (colliderHelper != null)
		{
			colliderHelper.pathfinder = this.pathfinder;
		}

		/*
		for (int i = 0; i < idlePath.Length; i ++)
		{
			PathNode temp = pathfinder.grid.nodeFromWorldPosition(idlePath[i]);
			idlePath[i] = temp.position;
		}
		*/

		if (_detection != detectMode.wandering && _detection != detectMode.nul)
		{
			_detection = detectMode.idle;
		}
		_startingDetection = _detection;
		_nextDetection = detectMode.nul;
		_nextForcedDetection = summoned || _startingDetection != detectMode.wandering ? detectMode.idle : detectMode.wandering;
		attackTargetActor = null;

		pathIndex = 0;
		idlePauseTimer = 0;
		pathfinder.pathingTimer = 0;
		idleLookTarget = transform.position + transform.up * 0.5F;

		moveTarget = lastMoveTarget = actorBody.transform.position;
	}

	void FixedUpdate()
	{
		_actorData = actor.actorData;
		attackTargetActor = null;
		eyeStart = transform.position + transform.up * 0.25F;
		_oldDetection = _detection;

		if (forcedTimer != 0)
		{
			forcedTimer -= Time.deltaTime;
			if (forcedTimer <= 0)
			{
				_detection = _nextForcedDetection;
				forcedTimer = 0;
				idlePathPauseTime[0] = 1;
				if (!summoned)
				{
					idlePath = new Vector2[] { };
					idlePathPauseTime = new float[] { };
				}
			}
		}

		//functions for noticing hostiles
		/* IMPORTANT NOTE: 
		 *
		 * attackTargetActor is going to be null if the target can no longer be detected, e.g. the player moves out of sight range
		 * Then the NPC should move to the last known location
		 */
		if (_detection != detectMode.nul && _detection != detectMode.forced)
		{
			hearHostiles();
			attackTargetActor = seeHostiles();
		}

		/* if on delay timer, wait to transition */
		if (_detection != detectMode.hostile)
		{
			if (delayTimer >= 0)
			{
				delayTimer -= Time.deltaTime;
			}
			if (delayTimer < 0 && attackTargetActor != null)
			{
				// create a new notice effect
				if ((actor.displayedEffect == null || actor.displayedEffect.effectScriptable.constantEffectType > EffectDefs.constantType.NOTICE) && showNotice && !summoned)
				{
					EffectDefs.effectApply(actor, gameManager.effectManager.notice1);
					showNotice = false;
					/*Vector3 vpPos = Camera.main.WorldToViewportPoint(transform.position);
					if (vpPos.x >= 0f && vpPos.x <= 1f && vpPos.y >= 0f && vpPos.y <= 1f && vpPos.z > 0f)
					{

					}*/
				}

				actor.setAttackTarget(attackTargetActor);
				attackTarget = attackTargetActor.transform.position;
				delayTimer = 0;

				_detection = detectMode.hostile;
				handleEdges();
				_oldDetection = _detection;
			}
		}

		/*  determine move target based on current state */
		if ((_detection == detectMode.idle || _detection == detectMode.forced) && idlePath.Length > 0)
		{
			/* If at a path position, go to next */
			if (((Vector2)actorBody.transform.position - idlePath[pathIndex]).magnitude < moveTargetError)
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
					//this is done to make the forced state work
					_detection = detectMode.idle;
				}

				if (idlePathPauseTime.Length == 1)
				{
					moveTarget = actor.transform.position;
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
		else if (_detection != detectMode.idle && _detection != detectMode.forced)
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
			showSus = false;
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

		stateSpeedIncrease = _actorData.acceleration * _actorData.moveSpeed;
		maxStateSpeed = _actorData.maxSpeed;

		setStateMoveSpeed();

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

		lastMoveTarget = moveTarget;

		currentSpeed = Mathf.Clamp(currentSpeed, 0, maxStateSpeed);

		if (!actor.movementLocked)
		{
			actor.Move(new Vector3(oldMoveInput.x * currentSpeed, oldMoveInput.y * currentSpeed));
		}
	}

	private void calcMoveInput()
	{
		if (moveTarget == null)
		{
			return;
		}

		/* only start pathfinding on a collision
		 * when starting pathfinding, wait for the timer before changing moveTarget
		 *
		 */
		Vector2 diff;
		if (pathfinder != null && pathfinder.startPathfinding && pathfinder.pathingTimer <= 0)
		{
			Debug.Log("Finding path for: " + gameObject.name);
			var tempTimer = System.Diagnostics.Stopwatch.StartNew();
			pathfinder.findPath(actorBody.transform.position, moveTarget);
			tempTimer.Stop();
			Debug.Log("Pathfinding took " + tempTimer.ElapsedMilliseconds + " milliseconds");
			usingPathfinding = true;
			pathfinder.pathingTimer = PATH_TIMER_LENGTH;
		}

		if (usingPathfinding)
		{
			Vector3 newTarget;
			/* if move target changes, then stop using pathfinding */
			if (pathfinder.startPathfinding == false && (lastMoveTarget - moveTarget).magnitude > moveTargetError)
			{
				usingPathfinding = false;
				diff = moveTarget - this.transform.position;
				moveInput = Vector2.ClampMagnitude(diff, 1F);
				return;
			}
			if (pathfinder.pathingTimer > 0)
			{
				pathfinder.pathingTimer -= Time.deltaTime;
			}

			newTarget = pathfinder.getNextMove(actorBody.transform.position, moveTargetError);
			if (newTarget == null || (newTarget - actorBody.transform.position).magnitude <= moveTargetError)
			{
				diff = moveTarget - this.transform.position;
				moveInput = Vector2.ClampMagnitude(diff, 1F);
				return;
			}

			pathfinder.startPathfinding = false;
			diff = newTarget - this.transform.position;
			if (diff.magnitude > moveTargetError)
			{
				diff *= 1 / diff.magnitude;
			}
		}
		else
		{
			diff = moveTarget - this.transform.position;
		}

		moveInput = Vector2.ClampMagnitude(diff, 1F);
		
		/* smooths out idle pathing */
		if (_detection == detectMode.idle && idlePauseTimer == 0)
		{
			moveInput = diff.normalized;
		}
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
		if (_nextDetection != detectMode.nul)
		{
			_oldDetection = _detection;
			_detection = _nextDetection;
			_nextDetection = detectMode.nul;
		}

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
					stateTimer = HOSTILE_TIMER_LENGTH;
				}
				return;
			case detectMode.idle:
				break;
			default:
				break;
		}
		switch (_detection)
		{
			case detectMode.idle:
				idleLookTarget = transform.position + transform.up * 0.5F;
				actorBody.rotation = actor.aimAngle(idleLookTarget);
				break;
			case detectMode.hostile:
				showSus = false;
				_actorData.sightAngle = actor._actorScriptable.sightAngle * 1.5F;
				stateTimer = HOSTILE_TIMER_LENGTH;
				break;
			case detectMode.seeking:
				showSus = true;
				showNotice = true;
				_actorData.sightAngle = actor._actorScriptable.sightAngle * 1.5F;
				stateTimer = HOSTILE_TIMER_LENGTH;
				actorBody.rotation = actor.aimAngle(attackTarget);
				break;
			case detectMode.lost:
				showSus = true;
				showNotice = true;
				_actorData.sightAngle = actor._actorScriptable.sightAngle * 1.5F;
				moveTarget = transform.position + transform.up;
				stateTimer = LOST_TIMER_LENGTH;
				break;
			case detectMode.suspicious:
				_actorData.sightAngle = actor._actorScriptable.sightAngle * 1.5F;
				stateTimer = SUS_TIMER_LENGTH;
				break;
			case detectMode.wandering:
				_actorData.sightAngle = actor._actorScriptable.sightAngle;
				moveTarget = transform.position + transform.up;
				stateTimer = LOST_TIMER_LENGTH;
				break;
			default:
				_actorData.sightAngle = actor._actorScriptable.sightAngle;
				stateTimer = 0;
				break;
		}

		stateTimerReset = stateTimer;

		if (_detection == detectMode.seeking || _detection == detectMode.lost || _detection == detectMode.suspicious)
		{
			// create a new Sus effect
			if ((actor.displayedEffect == null || actor.displayedEffect.effectScriptable.constantEffectType > EffectDefs.constantType.SEEKING) && showSus)
			{
				if ((_detection == detectMode.seeking || _detection == detectMode.lost) && !summoned)
				{
					EffectDefs.effectApply(actor, gameManager.effectManager.seeking1);
				}
				else if ((actor.displayedEffect == null || actor.displayedEffect.effectScriptable.constantEffectType > EffectDefs.constantType.SUS) && !summoned)
				{
					EffectDefs.effectApply(actor, gameManager.effectManager.sus1);
				}
			}
		}
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
					stateTimerReset = stateTimer;
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
				if (stateTimer <= 0)
				{
					_detection = detectMode.lost;
					stateTimer = LOST_TIMER_LENGTH;
					stateTimerReset = stateTimer;
				}
				else
				{
					stateTimer -= Time.deltaTime;
				}
				actorBody.rotation = actor.aimAngle(attackTarget);
				break;
			case detectMode.lost:
			case detectMode.wandering:
				/* Move in a random direction */
				if (stateTimer <= 0)
				{
					float radAngle = Random.Range(0F, 2F * Mathf.PI);
					float temp = Random.Range(wanderScale / 2, wanderScale);
					moveTarget = transform.position + new Vector3(temp * Mathf.Sin(radAngle), temp * Mathf.Cos(radAngle), 0);
					actorBody.rotation = actor.aimAngle(moveTarget);
					if (_detection == detectMode.wandering)
					{
						stateTimer = SUS_TIMER_LENGTH + Random.Range(0, SUS_TIMER_LENGTH);
						stateTimerReset = stateTimer;
					}
					else
					{
						stateTimer = LOST_TIMER_LENGTH;
						stateTimerReset = stateTimer;
					}
				}
				else
				{
					stateTimer -= Time.deltaTime;
					if (_detection == detectMode.wandering)
					{
						moveTarget = eyeStart;
					}
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

				if (stateTimer <= 0)
				{
					_detection = detectMode.lost;
					stateTimer = LOST_TIMER_LENGTH;
					stateTimerReset = stateTimer;
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
				break;
		}
	}

	private void handleSuspiciousState()
	{
		moveTarget = actor.transform.position;
		actorBody.rotation = actor.aimAngle(attackTarget);
		if (stateTimer <= 0)
		{
			_detection = _startingDetection;
			if (actor.displayedEffect != null)
			{
				Destroy(actor.displayedEffect.gameObject);
				actor.displayedEffect = null;
			}
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
		if (_detection == detectMode.hostile || _detection == detectMode.getWeapon || _detection == detectMode.frightened || heardSound.Length == 0 || summoned)
		{
			return;
		}

		/* by default, handle the edge case since a new sound was heard */
		if (heardSound.Length > 0)
		{
			_oldDetection = _startingDetection;
		}

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
					case SoundDefs.SoundType.THUD:
					case SoundDefs.SoundType.TAP:
					default:
						if (_detection == detectMode.lost || _detection == detectMode.seeking)
						{
							_detection = detectMode.seeking;
						}
						else
						{
							_detection = detectMode.suspicious;
							handleEdges();
							_oldDetection = _detection;
						}
						break;
				}
			}
		}
	}

	private Actor handleSightRays(Vector2 center, float maxLength, float increment, float sightRange, float index)
	{
		Vector2 viewRay = center - (Vector2)((maxLength - (index * increment)) * transform.right);
		RaycastHit2D[] rayHit = Physics2D.RaycastAll(eyeStart, viewRay, sightRange, gameManager.lineOfSightLayers);
		Debug.DrawRay(eyeStart, Vector2.ClampMagnitude(viewRay, 1) * sightRange);

		if (rayHit == null || rayHit.Length == 0)
		{
			return null;
		}

		foreach (RaycastHit2D hit in rayHit)
		{
			if (hit.rigidbody != null)
			{
				Actor targetActor = hit.rigidbody.gameObject.GetComponent<Actor>();
				if (targetActor == null)
				{
					/* hit a wall, stop looking */
					break;
				}
				if (actor.isTargetHostile(targetActor))
				{
					if ((_detection == detectMode.idle || _detection == detectMode.wandering || _detection == detectMode.suspicious) && delayTimer <= 0)
					{
						delayTimer = DELAY_TIMER_LENGTH;
						// create a new Sus effect
						if ((actor.displayedEffect == null || actor.displayedEffect.effectScriptable.constantEffectType > EffectDefs.constantType.SUS) && showSus && !summoned)
						{
							EffectDefs.effectApply(actor, gameManager.effectManager.sus1);
						}
						_detection = detectMode.suspicious;
						handleEdges();
						_oldDetection = _detection;
					}
					else if (_detection == detectMode.seeking)
					{
						// create a new notice effect
						if ((actor.displayedEffect == null || actor.displayedEffect.effectScriptable.constantEffectType > EffectDefs.constantType.NOTICE) && showNotice && !summoned)
						{
							EffectDefs.effectApply(actor, gameManager.effectManager.notice1);
							showNotice = false;
						}
						_detection = detectMode.hostile;
						handleEdges();
						_oldDetection = _detection;
					}

					stateTimer = stateTimerReset;
					actor.setAttackTarget(targetActor);
					attackTarget = targetActor.transform.position;

					if (lockWhenSpotted != null)
					{
						Collider2D[] actorColliders = new Collider2D[hit.rigidbody.attachedColliderCount];
						hit.rigidbody.GetAttachedColliders(actorColliders);
						if (actorColliders.Length > 0)
						{
							lockWhenSpotted.OnTriggerEnter2D(actorColliders[0]);
						}
					}

					return targetActor;
				}
			}
			else
			{
				break;
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

	public void disableStun()
	{
		// create a new Sus effect
		if (showSus && _detection == detectMode.suspicious && !summoned)
		{
			EffectDefs.effectApply(actor, gameManager.effectManager.sus1);
		}
	}

	public void setStunResponse(Vector2 target)
	{
		attackTarget = target;
		_nextDetection = detectMode.seeking;
	}

	public void setStunResponse(Actor sourceActor)
	{
		if (sourceActor == null)
		{
			return;
		}
		attackTarget = sourceActor.transform.position;
		_nextDetection = detectMode.seeking;
		attackTargetActor = sourceActor;
	}
	
	private void clearPathSummon()
	{
		idlePath = new Vector2[] { };
		idlePathPauseTime = new float[] { };
	}

	private void setStateMoveSpeed()
	{
		if (summoned)
		{
			return;
		}

		switch (_detection)
		{
			case detectMode.wandering:
				stateSpeedIncrease /= 4;
				maxStateSpeed /= 4;
				break;
			case detectMode.idle:
				stateSpeedIncrease /= 2.5F;
				maxStateSpeed /= 2.5F;
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

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (_detection == detectMode.hostile || _detection == detectMode.getWeapon || _detection == detectMode.frightened)
		{
			return;
		}

		if (collision != null)
		{
			Actor actorHit = collision.gameObject.GetComponentInChildren<Actor>();
			if (actorHit != null && actor.isTargetHostile(actorHit))
			{
				_nextDetection = detectMode.suspicious;
				attackTarget = actorHit.transform.position;
			}
		}
	}
}
