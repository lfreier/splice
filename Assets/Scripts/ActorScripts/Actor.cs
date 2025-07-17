using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using static ActorDefs;
using static EffectDefs;
using static UnityEngine.GraphicsBuffer;
using static WeaponDefs;
using static SceneDefs;

/*
 * Actor class:
 * Contains data and data interface for basic actors. 
 * Will be used as an interface for interacting with the Actor's 'body'
 */

public class Actor : MonoBehaviour
{
	public AudioSource actorAudioSource;

	public ActorScriptable _actorScriptable;
	public ActorData actorData;
	public Rigidbody2D actorBody;

	private Actor attackTarget;
	private Vector3 moveTarget;
	public bool movementLocked = false;
	public Vector3 currMoveVector;

	public LayerMask pickupLayer;

	public List<MonoBehaviour> behaviorList = new List<MonoBehaviour>();

	public GameObject mutationHolder;
	public GameObject effectHolder;

	public MutationInterface[] activeSlots;

	public bool isKnockedBack = false;

	public GameObject[] droppedItems;

	public GameManager gameManager;

	private GameObject equippedWeapon;
	public WeaponInterface equippedWeaponInt;

	public Sprite corpseSprite;
	public GameObject corpsePrefab;

	public EConstant displayedEffect = null;

	private SpriteRenderer sprite;

	private float speedCheck;

	private string setSide = null;
	private string oneSidePermanent = null;

	public bool invincible = false;

	public bool initialized = false;

	public GameObject unarmedPrefab;

	public GameObject enableOnDeath;

	public void Start()
	{
		initialized = false;
		speedCheck = 0;
		attackTarget = null;
		activeSlots = new MutationInterface[MutationDefs.MAX_SLOTS];
		sprite = GetComponent<SpriteRenderer>();
		initActorData();
		initialized = true;
	}

	public void FixedUpdate()
	{
		if (speedCheck > 0)
		{
			speedCheck -= Time.deltaTime;
		}
	}

	private void initActorData()
	{
		gameManager = GameManager.Instance;

		isKnockedBack = false;

		if (this != gameManager.playerStats.player)
		{
			actorData.armor = _actorScriptable.armor;
			actorData.health = _actorScriptable.health;
			actorData.maxHealth = _actorScriptable.health;
			actorData.shield = 0;

			actorData.maxSpeed = _actorScriptable.maxSpeed;
			actorData.moveSpeed = _actorScriptable.moveSpeed;
			actorData.acceleration = _actorScriptable.acceleration;
			actorData.deceleration = _actorScriptable.deceleration;

			actorData.hearingRange = _actorScriptable.hearingRange;
			actorData.sightAngle = _actorScriptable.sightAngle;
			actorData.sightRange = _actorScriptable.sightRange;

			actorData.frightenedDistance = _actorScriptable.frightenedDistance;
		}
		else
		{
			PlayerInteract interact = GetComponentInChildren<PlayerInteract>();
			if (interact != null && gameManager.actorBehaviors.Contains(interact.GetType()))
			{
				this.behaviorList.Add(interact);
			}
		}

		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			Transform child = this.gameObject.transform.GetChild(i);
			if (child != null && child.tag == WeaponDefs.OBJECT_WEAPON_TAG)
			{
				this.equip(child.gameObject);
			}
		}

		MonoBehaviour[] foundScripts = GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour foundScript in foundScripts)
		{
			if (gameManager.actorBehaviors.Contains(foundScript.GetType()))
			{
				this.behaviorList.Add(foundScript);
			}
		}

		if (gameManager != null && (equippedWeapon == null || equippedWeaponInt == null))
		{
			equipEmpty();
		}
	}

	public float aimAngle(Vector2 aimTarget)
	{
		return (Mathf.Atan2((aimTarget - (Vector2)this.transform.position).y, (aimTarget - (Vector2)this.transform.position).x) * Mathf.Rad2Deg) - 90F;
	}

	public void attack()
	{
		if (equippedWeaponInt == null || !equippedWeaponInt.isActive())
		{
			if (speedCheck <= 0)
			{
				equippedWeaponInt.attack(gameManager.actorLayers);
				speedCheck = equippedWeaponInt.getSpeed();
			}
		}
	}

	public void drop()
	{
		if (equippedWeaponInt == null || !equippedWeaponInt.canBeDropped()) { return; }

		equippedWeaponInt.cancelAttack();

		Vector3 pointerPos = actorBody.transform.TransformDirection(transform.up);

		/* Literally just getting positive/negatives */
		Vector3 aimDir = new Vector3(pointerPos.x, pointerPos.y, 0) - this.transform.position;
		Vector3 translate = new Vector3(Random.Range(0.25F * Mathf.Sign(aimDir.x), 0.375F * Mathf.Sign(aimDir.x)), Random.Range(0.25F * Mathf.Sign(aimDir.y), 0.375F * Mathf.Sign(aimDir.y)), 0);

		if (equippedWeapon != null)
		{
			float rand = Random.Range(0, 1);
			rand = rand < 1 ? Random.Range(-60, -30) : Random.Range(30, 60);
			equippedWeapon.transform.SetParent(null, true);
			equippedWeapon.transform.SetPositionAndRotation(this.transform.position + translate, Quaternion.Euler(0, 0, equippedWeapon.transform.rotation.eulerAngles.z + rand));
			setObjectLayer(WeaponDefs.SORT_LAYER_GROUND, equippedWeapon);
			setWeaponTag(equippedWeapon, WeaponDefs.OBJECT_WEAPON_TAG);

			WeaponPhysics physics = equippedWeapon.GetComponentInChildren<WeaponPhysics>();
			if (physics != null)
			{
				BasicWeapon weap = equippedWeapon.GetComponentInChildren<BasicWeapon>();
				if (weap != null)
				{
					BoxCollider2D collider = physics.GetComponent<BoxCollider2D>();
					moveFromCollider(collider, weap.transform.localPosition, equippedWeapon);
					weap.actorWielder = null;
				}
			}

			resetEquip();
		}
	}

	public void dropItem()
	{
		Vector3 droppedPosition = new Vector3(this.transform.position.x, this.transform.position.y);
		foreach (GameObject item in droppedItems)
		{
			GameObject newDrop = Instantiate(item, droppedPosition, this.transform.rotation, this.transform.parent);
			newDrop.transform.Rotate(new Vector3(0, 0, Random.Range(-45, 45)), Space.Self);

			Cell newCell = newDrop.GetComponent<Cell>();
			if (newCell != null)
			{
				EnemyData npcData = this.gameObject.GetComponent<EnemyData>();
				if (npcData != null)
				{
					newCell.generateCount(npcData.cellDropMin, npcData.cellDropMax);
				}
			}
			else
			{
				/* cells should be on top layer */
				setObjectLayer(WeaponDefs.SORT_LAYER_GROUND, newDrop);
			}

			float randX = Random.Range(-0.5F, -.5F);
			float randY = Random.Range(-0.5F, -.5F);
			if (!Physics2D.BoxCast(new Vector2(droppedPosition.x + randX, droppedPosition.y + randY), new Vector2(0.01F, 0.01F), 0, Vector2.up, 0.01F, gameManager.unwalkableLayers))
			{
				droppedPosition.x += randX;
				droppedPosition.y += randY;
			}

			//don't do this for cells
			if (newCell == null)
			{
				BoxCollider2D collider = newDrop.GetComponentInChildren<BoxCollider2D>();
				//moveFromCollider(collider, newDrop);
			}
		}
	}

	private void moveFromCollider(BoxCollider2D collider, Vector2 offset, GameObject toMove)
	{
		if (collider != null)
		{
			Vector3 center = new Vector2(collider.bounds.min.x + collider.size.x / 2, collider.bounds.min.y + collider.size.y / 2);
			List<RaycastHit2D> wallList = new List<RaycastHit2D>();
			ContactFilter2D filter = new ContactFilter2D();
			filter.SetLayerMask(gameManager.unwalkableLayers);

			collider.Cast(collider.transform.position, collider.transform.rotation.eulerAngles.z, Vector2.left, filter, wallList, 0, false);

			if (wallList == null || wallList.Count <= 0)
			{
				return;
			}

			float mult = 0;

			for (int n = 0; n < 4; n++)
			{
				mult += 0.5F;
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						wallList = new List<RaycastHit2D>();
						collider.Cast((Vector2)collider.transform.position + new Vector2(i * mult, j * mult), collider.transform.rotation.eulerAngles.z, Vector2.left, filter, wallList, 0, false);
						
						if (wallList == null || wallList.Count <= 0)
						{
							toMove.transform.SetPositionAndRotation((Vector2)collider.transform.position + new Vector2(i * mult, j * mult), toMove.transform.rotation);
							Debug.Log("Moved " + toMove.name + " " + (n + 1) + " squares");
							return;
						}
					}
				}
			}
		}
	}

	/* 
	 * @param weaponToEquip
	 */
	public bool equipActive(GameObject activeToEquip)
	{
		/* TODO: more than blindly equip */



		return true;
	}

	/* 
	 * @param weaponToEquip
	 */
	public bool equip(GameObject weaponToEquip)
	{
		WeaponInterface tempWeapInt = null;

		/* This should make it so it doesn't matter if you equip the parent or child object */
		Transform tempWeap = weaponToEquip.transform;
		if (null == (tempWeapInt = weaponToEquip.GetComponent<WeaponInterface>()))
		{
			/* Only one place it can be if in children */
			if (tempWeap.childCount > 0)
			{
				for (int i = 0; i < tempWeap.childCount; i ++)
				{
					if (null != (tempWeapInt = tempWeap.GetChild(i).GetComponent<WeaponInterface>()))
					{
						break;
					}
				}
			}
			/* Has to be in the parent */
			else
			{
				while (tempWeap.parent != null)
				{
					tempWeap = tempWeap.parent;
					if (null != (tempWeapInt = tempWeap.GetComponent<WeaponInterface>()))
					{
						tempWeap = tempWeap.parent;
						break;
					}
				}
			}
		}
		else
		{
			/* tempWeap is the data object, so it needs to be the parent now */
			tempWeap = tempWeap.parent;
		}


		if (tempWeapInt == null)
		{
			return false;
		}

		if (equippedWeaponInt != null)
		{
			if (equippedWeaponInt.canBeDropped())
			{
				drop();
			}
			
			if (equippedWeaponInt.getType() == WeaponType.UNARMED)
			{
				/* only destroy copies of the basic fist weapon, not muations */
				if (equippedWeapon.GetComponentInChildren<MutationInterface>() == null)
				{
					Destroy(equippedWeapon);
				}
			}
			else
			{
				return false;
			}
		}
		equippedWeapon = tempWeap.gameObject;
		equippedWeaponInt = tempWeapInt;

		/* TODO: slightly hacky way for MBeast to work*/
		if (equippedWeapon.GetComponent<MutationInterface>() == null)
		{
			equippedWeapon.transform.SetParent(this.transform, true);
			setObjectLayer(WeaponDefs.SORT_LAYER_CHARS, equippedWeapon);
			equippedWeaponInt.setStartingPosition(true);
		}

		WeaponDefs.setWeaponTag(equippedWeapon, WeaponDefs.EQUIPPED_WEAPON_TAG);

		equippedWeaponInt.setActorToHold(this);

		return true;
	}

	public GameObject getEquippedWeapon()
	{
		return equippedWeapon;
	}

	public Actor getAttackTarget()
	{
		return attackTarget;
	}

	public GameObject instantiateActive(GameObject prefab)
	{
		GameObject activePrefab = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
		MutationInterface mutScript = activePrefab.GetComponentInChildren<MutationInterface>();

		activePrefab.transform.SetParent(mutationHolder.transform, false);
		mutScript.setStartingPosition();
		WeaponDefs.setWeaponTag(activePrefab, WeaponDefs.EQUIPPED_ACTIVE_TAG);

		return activePrefab;
	}

	public GameObject instantiateWeapon(GameObject prefab)
	{
		GameObject weaponPrefab = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

		weaponPrefab.transform.SetParent(this.gameObject.transform, true);
		weaponPrefab.transform.position = new Vector3(0.25F, 0.25F, 0);
		WeaponDefs.setWeaponTag(weaponPrefab, WeaponDefs.EQUIPPED_WEAPON_TAG);

		return weaponPrefab;
	}

	public bool inWeaponRange(Vector3 target)
	{
		if (equippedWeaponInt == null || equippedWeapon == null)
		{
			return false;
		}
		return equippedWeaponInt.inRange(target);
	}
	public bool isStunned()
	{
		return !behaviorList[0].enabled;
	}

	public bool isTargetHostile(Actor targetActor)
	{
		if (targetActor == null)
		{
			return false;
		}

		if (this.tag.Equals(targetActor.tag)
			|| (tag.Contains(playerTag) && targetActor.tag.Contains(playerTag)))
		{
			return false;
		}

		return true;
	}

	public bool isUnarmed()
	{
		if (equippedWeaponInt == null)
		{
			return true;
		}
		return this.equippedWeaponInt.getType() == WeaponType.UNARMED;
	}

	/* Deals with actor death. Always called when health is reduced to zero.
	 * Needs to drop weapons/items, destroy each attached effect, play sound, and drop a corpse
	 */
	public void kill()
	{
		drop();

		for (int i = 0; i < effectHolder.transform.childCount; i ++)
		{
			Transform currentEffect = effectHolder.transform.GetChild(i);

			if (currentEffect != null)
			{
				Destroy(currentEffect.gameObject);
			}
		}

		dropItem();
		if (corpsePrefab != null)
		{
			if (!isLevelScene((SCENE)SCENE_BUILD_MASK[SceneManager.GetActiveScene().buildIndex]))
			{
				SceneManager.SetActiveScene(gameObject.scene);
			}
			GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);
			newCorpse.transform.Rotate(0, 0, Random.Range(-20, 20));

			Corpse script = newCorpse.GetComponent<Corpse>();
			if (script != null)
			{
				script.actorData = copyData(this.actorData);
				script.corpseSprite.sprite = this.corpseSprite;
			}
		}
		
		/* Player logic */
		if (gameObject.tag == ActorDefs.playerTag)
		{
			foreach (MonoBehaviour script in behaviorList)
			{
				script.enabled = false;
			}
			gameManager.gameOver(this);
		}
		
		if (enableOnDeath != null)
		{
			enableOnDeath.SetActive(true);
		}

		Destroy(transform.gameObject);
	}

	public void Move(Vector3 moveVector)
	{
		if (!isKnockedBack)
		{
			currMoveVector = new Vector3(moveVector.x, moveVector.y, moveVector.z);
			actorBody.velocity = moveVector;
		}
	}

	public bool pickup()
	{
		Collider2D[] hitTargets1 = Physics2D.OverlapCircleAll(this.transform.position + (transform.up * ActorDefs.GLOBAL_PICKUP_OFFSET), ActorDefs.GLOBAL_PICKUP_RANGE, this.pickupLayer);
		Collider2D[] hitTargets2 = Physics2D.OverlapCircleAll(this.transform.position, ActorDefs.GLOBAL_PICKUP_RANGE, this.pickupLayer);
		Collider2D[] hitTargets = new Collider2D[hitTargets1.Length + hitTargets2.Length];
		hitTargets1.CopyTo(hitTargets, 0);
		hitTargets2.CopyTo(hitTargets, hitTargets1.Length);

		return pickup(hitTargets);
	}

	public bool pickup(Collider2D[] hitTargets)
	{
		List<GameObject> pickupList = new List<GameObject>();

		foreach (Collider2D target in hitTargets)
		{
			if (target == null)
			{
				continue;
			}

			if (pickupItem(target.gameObject))
			{
				PickupEngine engine = target.gameObject.GetComponent<PickupEngine>();
				if (engine != null)
				{
					engine.disableHighlight();
				}
				return true;
			}
		}

		return false;
	}

	public bool pickupItem(GameObject target)
	{
		if (target == null)
		{
			return false;
		}

		/* Make sure to only pickup valid objects. This will be expanded on eventually */
		if (WeaponDefs.canWeaponBePickedUp(target))
		{
			Debug.Log("Picking up: " + target.transform.gameObject.name);
			if (this.equip(target.transform.gameObject))
			{
				//Make sure to only pick up one weapon, but pickups are ok
				//pickupItemPlayer(target, true);
				return true;
			}
		}

		PickupBox pickupBox = target.GetComponent<PickupBox>();
		if (pickupBox != null && pickupBox.hasPickup())
		{
			target = pickupBox.getPickup();
			if (target == null)
			{
				return false;
			}
		}

		/* TODO: Probably a better way to do this */
		if (this.tag == ActorDefs.playerTag)
		{
			return pickupItemPlayer(target, false);
		}

		return false;
	}

	public bool pickupItemPlayer(GameObject target, bool onlyPickup)
	{
		if (PickupDefs.canBePickedUp(target))
		{
			PickupInterface pickup = target.transform.gameObject.GetComponentInChildren<PickupInterface>();
			if (pickup == null)
			{
				pickup = target.transform.gameObject.GetComponentInParent<PickupInterface>();
			}
			if (pickup != null)
			{
				Debug.Log("Picking up: " + pickup);
				pickup.pickup(this);
				return true;
			}
		}
		else if (WeaponDefs.canWeaponBePickedUp(target) && !onlyPickup)
		{
			Debug.Log("Picking up: " + target.transform.gameObject.name);
			if (this.equip(target.transform.gameObject))
			{
				//Make sure to only pick up one weapon
				return true;
			}
		}

		/* TODO: trying to put a pickup on the ground if it's not valid */
		/*
		else if (pickupBox != null)
		{
			target.transform.SetParent(null, false);
			target.transform.SetPositionAndRotation(transform.position, target.transform.rotation);
			target.transform.RotateAround(target.transform.position, Vector3.forward, Random.Range(0, 360));
		}*/

		return false;
	}

	public void setAttackTarget(Actor targetActor)
	{
		attackTarget = targetActor;
	}

	public void setAttackOnly(string animStateSide, bool permanent)
	{
		BasicWeapon weaponScript = equippedWeapon.GetComponentInChildren<BasicWeapon>();
		if (weaponScript == null)
		{
			return;
		}

		if (permanent && System.String.IsNullOrEmpty(oneSidePermanent))
		{
			oneSidePermanent = animStateSide;
		}
		if (System.String.IsNullOrEmpty(animStateSide) && System.String.IsNullOrEmpty(oneSidePermanent))
		{
			setSide = null;
			weaponScript.anim.SetBool(WeaponDefs.ANIM_BOOL_ONLY_RIGHT, false);
			weaponScript.anim.SetBool(WeaponDefs.ANIM_BOOL_ONLY_LEFT, false);
			return;
		}

		// set to permanent if set
		string setString = System.String.IsNullOrEmpty(oneSidePermanent) ? animStateSide : oneSidePermanent;

		bool toSet = setString == WeaponDefs.ANIM_BOOL_ONLY_RIGHT;
		weaponScript.anim.SetBool(WeaponDefs.ANIM_BOOL_ONLY_RIGHT, toSet);
		weaponScript.anim.SetBool(WeaponDefs.ANIM_BOOL_ONLY_LEFT, !toSet);
		weaponScript.anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_SWAP_SIDE);
		equippedWeaponInt.setStartingPosition(toSet);

		setSide = setString;
	}

	public void setColor(Color newColor)
	{
		if (sprite != null)
		{
			this.sprite.color = newColor;
		}
	}

	public void setConstant(bool toggle, constantType type)
	{
		switch (type)
		{
			case constantType.STUN:
				foreach (MonoBehaviour script in behaviorList)
				{
					script.enabled = !toggle;
					if (this.tag == playerTag)
					{
						gameManager.signalMovementUnlocked();
						gameManager.signalRotationUnlocked();
					}
				}
				EnemyMove enemyMove = GetComponent<EnemyMove>();
				if (enemyMove != null)
				{
					enemyMove.disableStun();
				}
				break;
			case constantType.IFRAME:
				if (this.tag == playerTag)
				{
					this.invincible = toggle;
				}
				break;
			default:
				break;
		}
	}

	public void setActorCollision(bool toSet, string[] excludingLayers)
	{
		if (toSet)
		{
			actorBody.excludeLayers = 0;
		}
		else
		{
			actorBody.excludeLayers = LayerMask.GetMask(excludingLayers);
		}
	}

	public void setMovementLocked(bool locked)
	{
		movementLocked = locked;
	}

	/* Changes the actor's max speed to the given value.
	 * If changedSpeed is negative, will reset to the actor's base speed.
	 */
	public void setSpeed(float changedSpeed)
	{
		if (changedSpeed < 0)
		{
			/* Reset max speed */
			actorData.maxSpeed = _actorScriptable.maxSpeed;
		}
		else
		{
			Debug.Log("newSpeed: " + changedSpeed);
			actorData.maxSpeed = changedSpeed;
		}
	}

	/* Logic to handle taking damage from any source
	 * only applies effects that originate from this actor
	 * returns actual damage taken (health cannot be negative)
	 */
	public float takeDamage(float damage)
	{
		return takeDamage(damage, null);
	}

	public void addStun(float sec)
	{
		EConstant[] activeEffects = effectHolder.GetComponentsInChildren<EConstant>();
		foreach (EConstant effect in activeEffects)
		{
			if (effect != null && effect.effectScriptable.constantEffectType == EffectDefs.constantType.STUN)
			{
				effect.timer += sec;
			}
		}
	}

	public float takeDamage(float damage, Actor sourceActor)
	{
		if (this.invincible && sourceActor != null)
		{
			return 0F;
		}

		float startingHealth = actorData.health;
		/* make sure we dont go negative armor like a dumb */
		float damageTaken = damage;
		if (actorData.armor > 0)
		{
			damageTaken = actorData.armor > damage ? 0 : (damage - actorData.armor);
			if (damage > 0 && damageTaken == 0)
			{
				damageTaken = 0.5F;
			}
		}

		if (damageTaken <= 0)
		{
			return 0;
		}

		/* take damage from shield first */
		if (actorData.shield > 0)
		{
			float shieldDamage;
			if (damageTaken > actorData.shield)
			{
				shieldDamage = actorData.shield;
			}
			else
			{
				shieldDamage = damageTaken;
			}
			damageTaken -= shieldDamage;
			actorData.shield -= shieldDamage;
			if (this.tag.Equals(ActorDefs.playerTag))
			{
				/* shields will be lost on the HUD when damage is taken */
				gameManager.signalUpdateShieldEvent(actorData.shield);
			}
		}
		actorData.health -= damageTaken;
		actorData.health = actorData.health < 0 ? 0 : actorData.health;

		if (actorData.health <= 0.0F)
		{
			this.kill();
		}

		if (damageTaken > 0)
		{
			if (this.tag.Equals(ActorDefs.playerTag))
			{
				EffectDefs.effectApply(this, gameManager.effectManager.iFrame1);
				gameManager.signalUpdateHealthEvent(actorData.health);
			}
			else
			{
				EffectDefs.effectApply(this, gameManager.effectManager.iFrame0);
				EnemyMove enemyMove = GetComponent<EnemyMove>();
				if (enemyMove != null)
				{
					enemyMove.setStunResponse(sourceActor);
					if (damageTaken >= 1F && (enemyMove._detection == detectMode.idle || enemyMove._detection == detectMode.suspicious || enemyMove._detection == detectMode.wandering) && !isStunned() && !enemyMove.summoned)
					{
						EffectDefs.effectApply(this, gameManager.effectManager.stun1);
					}
				}
			}
		}

		if (sourceActor != null)
		{
			if (sourceActor.tag == ActorDefs.playerTag || this.tag == ActorDefs.playerTag)
			{
				/* TODO:
				 * hardcoded bad, but w/e
				 * hitstop for different amounts of damage
				 */
				float hitstopLength;
				float hitstopSpeed;
				if (damage < 1)
				{
					hitstopLength = 0;
					hitstopSpeed = 1F;
				}
				else if (damage < 1.5)
				{
					hitstopLength = HITSTOP_LENGTH_SMALL;
					hitstopSpeed = HITSTOP_SPD_NORMAL;
				}
				else if (damage < 2.5)
				{
					hitstopLength = HITSTOP_LENGTH_MID;
					hitstopSpeed = HITSTOP_SPD_NORMAL;
				}
				else if (damage < 4)
				{
					hitstopLength = HITSTOP_LENGTH_HIGH;
					hitstopSpeed = HITSTOP_SPD_HIGH;
				}
				else
				{
					hitstopLength = HITSTOP_LENGTH_MASSIVE;
					hitstopSpeed = HITSTOP_SPD_HIGH;
				}
				gameManager.hitstop(hitstopLength, hitstopSpeed);
			}
		}

		return startingHealth - actorData.health;
	}

	public float takeHeal(float heal)
	{
		float startingHealth = actorData.health;
		actorData.health += heal;

		if (actorData.health > _actorScriptable.health)
		{
			actorData.health = _actorScriptable.health;
		}

		if (this.tag.Equals(ActorDefs.playerTag))
		{
			gameManager.signalUpdateHealthEvent(actorData.health);
		}

		return startingHealth - actorData.health;
	}

	public void throwWeapon()
	{
		Vector3 pointerPos = actorBody.transform.TransformDirection(transform.up);
		throwWeapon(pointerPos);
	}

	public void throwWeapon(Vector3 throwTargetPos)
	{
		if (!equippedWeaponInt.canBeDropped() || equippedWeaponInt.isActive())
		{
			return;
		}

		Vector3 aimDir = new Vector3(throwTargetPos.x, throwTargetPos.y, 0) - this.transform.position;
		this.equippedWeaponInt.throwWeapon(Vector3.ClampMagnitude(aimDir, 1));

		resetEquip();
	}

	public void triggerDamageEffects(Actor target)
	{
		var mutationList = mutationHolder.GetComponentsInChildren<MutationInterface>();

		foreach (MutationInterface mutation in mutationList)
		{
			if (mutation.getMutationType() == mutationTrigger.DAMAGE_GIVEN)
			{
				mutation.trigger(target);
			}
		}
	}

	private void resetEquip()
	{
		equippedWeapon.transform.SetParent(null, true);
		WeaponDefs.setWeaponTag(equippedWeapon, WeaponDefs.OBJECT_WEAPON_TAG);

		equippedWeapon = null;
		equippedWeaponInt = null;

		equipEmpty();
	}

	public void equipEmpty()
	{
		if (unarmedPrefab == null)
		{
			unarmedPrefab = gameManager.prefabManager.weapPFist;
		}
		GameObject unarmedWeapon = instantiateWeapon(unarmedPrefab);

		equip(unarmedWeapon.transform.gameObject);
	}
}