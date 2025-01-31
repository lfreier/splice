using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using static ActorDefs;
using static EffectDefs;
using static WeaponDefs;

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

	public GameObject[] droppedItems;

	public GameManager gameManager;

	private GameObject equippedWeapon;
	public WeaponInterface equippedWeaponInt;

	public Sprite corpseSprite;
	public GameObject corpsePrefab;

	private SpriteRenderer sprite;

	private float speedCheck;

	private string setSide = null;

	public bool invincible = false;

	public void Start()
	{
		speedCheck = 0;
		attackTarget = null;
		activeSlots = new MutationInterface[MutationDefs.MAX_SLOTS];
		sprite = GetComponent<SpriteRenderer>();
		initActorData();
	}

	public void Update()
	{
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

	public void attackSecondary()
	{
		if (equippedWeaponInt == null || !equippedWeaponInt.isActive())
		{
			if (speedCheck <= 0)
			{
				equippedWeaponInt.attackSecondary();
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
		Vector3 translate = new Vector3(Random.Range(0.2F * Mathf.Sign(aimDir.x), 0.6F * Mathf.Sign(aimDir.x)), Random.Range(0.2F * Mathf.Sign(aimDir.y), 0.6F * Mathf.Sign(aimDir.y)), 0);

		equippedWeapon.transform.Translate(translate, Space.World);
		equippedWeapon.transform.Rotate(new Vector3(0, 0, Random.Range(-45, 45)), Space.Self);
		setWeaponLayer(WeaponDefs.SORT_LAYER_GROUND);

		resetEquip();
	}

	public void dropItem()
	{
		Vector3 droppedPosition = new Vector3(this.transform.position.x, this.transform.position.y);
		foreach (GameObject item in droppedItems)
		{
			droppedPosition.x += Random.Range(-0.5F, -.5F);
			droppedPosition.y += Random.Range(-0.5F, -.5F);
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
			else if (equippedWeaponInt.getType() == WeaponType.UNARMED)
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
			setWeaponLayer(WeaponDefs.SORT_LAYER_CHARS);
			equippedWeaponInt.setStartingPosition(true);
		}

		WeaponDefs.setWeaponTag(equippedWeapon, WeaponDefs.EQUIPPED_WEAPON_TAG);

		equippedWeaponInt.setActorToHold(this);

		if (setSide != null)
		{
			setAttackOnly(setSide);
		}

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
		return behaviorList[0].enabled;
	}

	public bool isTargetHostile(Actor targetActor)
	{
		if (targetActor == null)
		{
			return false;
		}

		if (this.tag.Equals(targetActor.tag))
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
			GameObject newCorpse = Instantiate(corpsePrefab, transform.position, transform.rotation);
			SpriteRenderer newSprite = newCorpse.GetComponent<SpriteRenderer>();
			newCorpse.transform.Rotate(0, 0, Random.Range(-20, 20));

			if (newSprite != null)
			{
				newSprite.sprite = corpseSprite;
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
		Destroy(transform.gameObject);
	}

	public void Move(Vector3 moveVector)
	{
		currMoveVector = new Vector3(moveVector.x, moveVector.y, moveVector.z);
		actorBody.velocity = moveVector;
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
			if (pickupItem(target.gameObject))
			{
				return true;
			}
		}

		return false;
	}

	public bool pickupItem(GameObject target)
	{
		/* Make sure to only pickup valid objects. This will be expanded on eventually */
		if (WeaponDefs.canWeaponBePickedUp(target))
		{
			Debug.Log("Picking up: " + target.transform.gameObject.name);
			if (this.equip(target.transform.gameObject))
			{
				//Make sure to only pick up one weapon
				return true;
			}
		}

		PickupBox pickupBox = target.GetComponent<PickupBox>();
		if (pickupBox != null && pickupBox.hasPickup())
		{
			target = pickupBox.getPickup();
		}

		/* TODO: Probably a better way to do this */
		if (this.tag == ActorDefs.playerTag)
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
			else if (WeaponDefs.canWeaponBePickedUp(target))
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
		}

		return false;
	}

	public void setAttackTarget(Actor targetActor)
	{
		attackTarget = targetActor;
	}

	public void setAttackOnly(string animStateSide)
	{
		BasicWeapon weaponScript = equippedWeapon.GetComponentInChildren<BasicWeapon>();
		if (weaponScript == null)
		{
			return;
		}

		bool toSet = animStateSide == WeaponDefs.ANIM_BOOL_ONLY_RIGHT;
		weaponScript.anim.SetBool(WeaponDefs.ANIM_BOOL_ONLY_RIGHT, toSet);
		weaponScript.anim.SetBool(WeaponDefs.ANIM_BOOL_ONLY_LEFT, !toSet);
		weaponScript.anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_SWAP_SIDE);
		equippedWeaponInt.setStartingPosition(toSet);

		setSide = animStateSide;
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
				}
				break;
			case constantType.IFRAME:
				this.invincible = toggle;
				break;
			default:
				break;
		}
	}

	public void setActorCollision(bool toSet)
	{
		if (toSet)
		{
			actorBody.excludeLayers = 0;
		}
		else
		{
			actorBody.excludeLayers = LayerMask.GetMask(new string[] { GameManager.OBJECT_MID_LAYER, GameManager.ACTOR_LAYER });
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

	private void setWeaponLayer(string layerName)
	{
		SpriteRenderer[] allSprites = equippedWeapon.GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer temp in allSprites)
		{
			temp.sortingLayerName = layerName;
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

	public float takeDamage(float damage, Actor sourceActor)
	{
		if (this.invincible)
		{
			return 0F;
		}

		float startingHealth = actorData.health;
		/* make sure we dont go negative armor like a dumb */
		float damageTaken = actorData.armor > damage ? 0 : (damage - actorData.armor);

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
		if (!equippedWeaponInt.canBeDropped())
		{
			return;
		}

		Vector3 aimDir = new Vector3(throwTargetPos.x, throwTargetPos.y, 0) - this.transform.position;
		setWeaponLayer(WeaponDefs.SORT_LAYER_GROUND);
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
		GameObject fistPrefab = instantiateWeapon(gameManager.weapPFist);

		equip(fistPrefab.transform.gameObject);
	}
}