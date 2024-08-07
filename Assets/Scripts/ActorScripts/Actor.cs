using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

/*
 * Actor class:
 * Contains data and data interface for basic actors. 
 * Will be used as an interface for interacting with the Actor's 'body'
 */

public class Actor : MonoBehaviour
{
	public struct ActorData
	{
		public float health;

		public float maxSpeed;
		public float moveSpeed;

		public float acceleration;
		public float deceleration;

		public float hearingRange;
		public float sightRange;

		public float frightenedDistance;
	};

	public enum detectMode
	{
		idle = 0,
		suspicious = 1,
		hostile = 2,
		frightened = 3,
		cautious = 4,
		getWeapon = 5
	};

	public detectMode detection;
	public detectMode oldDetection;

	public ActorScriptable _actorScriptable;
	public ActorData actorData;
	public Rigidbody2D actorBody;

	private Actor attackTarget;
	private Vector3 moveTarget;
	public Vector3 currMoveVector;

	public LayerMask pickupLayer;

	public List<MonoBehaviour> behaviorList = new List<MonoBehaviour>();

	public GameObject mutationHolder;
	public GameObject effectHolder;

	public MutationInterface[] activeSlots;

	public GameManager gameManager;

	private GameObject equippedWeapon;
	private WeaponInterface equippedWeaponInt;

	private SpriteRenderer sprite;

	private float speedCheck;

	public void Start()
	{
		speedCheck = 0;
		attackTarget = null;
		gameManager = GameManager.Instance;
		activeSlots = new MutationInterface[MutationDefs.MAX_SLOTS];
		initActorData();
	}

	public void Update()
	{
		/* TODO: if game manager is null, just don't bother equipping fists.
		   But it shouldn't work like that, gameManager should be singleton*/
		if (gameManager != null && (equippedWeapon == null || equippedWeaponInt == null))
		{
			equipEmpty();
		}
	}

	public void FixedUpdate()
	{
		speedCheck -= Time.deltaTime;
	}

	private void initActorData()
	{
		actorData.health = _actorScriptable.health;

		actorData.maxSpeed = _actorScriptable.maxSpeed;
		actorData.moveSpeed = _actorScriptable.moveSpeed;
		actorData.acceleration = _actorScriptable.acceleration;
		actorData.deceleration = _actorScriptable.deceleration;

		actorData.hearingRange = _actorScriptable.hearingRange;
		actorData.sightRange = _actorScriptable.sightRange;

		actorData.frightenedDistance = _actorScriptable.frightenedDistance;

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

		Vector3 pointerPos = actorBody.transform.TransformDirection(transform.up);

		/* Literally just getting positive/negatives */
		Vector3 aimDir = new Vector3(pointerPos.x, pointerPos.y, 0) - this.transform.position;
		Vector3 translate = new Vector3(Random.Range(0.2F * Mathf.Sign(aimDir.x), 0.6F * Mathf.Sign(aimDir.x)), Random.Range(0.2F * Mathf.Sign(aimDir.y), 0.6F * Mathf.Sign(aimDir.y)), 0);

		equippedWeapon.transform.Translate(translate, Space.World);
		equippedWeapon.transform.Rotate(new Vector3(0, 0, Random.Range(-45, 45)), Space.Self);
		sprite.sortingLayerName = WeaponDefs.SORT_LAYER_GROUND;

		resetEquip();
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
		GameObject tempWeaponToEquip;
		WeaponInterface tempWeapInt = weaponToEquip.GetComponent<WeaponInterface>();

		/* This should make it so it doesn't matter if you equip the parent or child object */
		if (tempWeapInt == null)
		{
			if (null != (tempWeapInt = weaponToEquip.transform.GetChild(0).GetComponent<WeaponInterface>()))
			{
				tempWeaponToEquip = weaponToEquip;
			}
			else
			{
				return false;
			}
		}
		else
		{
			tempWeaponToEquip = weaponToEquip.transform.parent.gameObject;
		}

		if (equippedWeaponInt != null)
		{
			if (equippedWeaponInt.canBeDropped())
			{
				drop();
			}
			else if (equippedWeaponInt.getType() == WeaponType.UNARMED)
			{
				Destroy(equippedWeapon);
			}
			else
			{
				return false;
			}
		}

		equippedWeapon = tempWeaponToEquip;
		equippedWeaponInt = tempWeapInt;

		equippedWeapon.transform.SetParent(this.transform, true);
		sprite = equippedWeapon.GetComponentInChildren<SpriteRenderer>();
		sprite.sortingLayerName = WeaponDefs.SORT_LAYER_CHARS;
		equippedWeaponInt.setStartingPosition();

		setWeaponTag(equippedWeapon, WeaponDefs.EQUIPPED_WEAPON_TAG);

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

		activePrefab.transform.SetParent(this.gameObject.transform, false);
		mutScript.setStartingPosition();
		setWeaponTag(activePrefab, WeaponDefs.EQUIPPED_ACTIVE_TAG);

		return activePrefab;
	}

	public GameObject instantiateWeapon(GameObject prefab)
	{
		GameObject weaponPrefab = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

		weaponPrefab.transform.SetParent(this.gameObject.transform, true);
		weaponPrefab.transform.position = new Vector3(0.25F, 0.25F, 0);
		setWeaponTag(weaponPrefab, WeaponDefs.EQUIPPED_WEAPON_TAG);

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

	public bool isTargetHostile(Actor targetActor)
	{
		if (targetActor == null)
		{
			return false;
		}

		if (this.tag.Equals(ActorDefs.npcTag) && targetActor.tag.Equals(ActorDefs.playerTag)
			|| this.tag.Equals(ActorDefs.playerTag) && targetActor.tag.Equals(ActorDefs.npcTag))
		{
			return true;
		}

		return false;
	}

	public bool isUnarmed()
	{
		if (equippedWeaponInt == null)
		{
			return true;
		}
		return this.equippedWeaponInt.getType() == WeaponType.UNARMED;
	}

	public void kill()
	{
		drop();
		//TODO: spawn replacement sprite
		Destroy(transform.gameObject);
	}

	public void Move(Vector3 moveVector)
	{
		currMoveVector = new Vector3(moveVector.x, moveVector.y, moveVector.z);
		actorBody.MovePosition(actorBody.transform.position + moveVector);
	}

	public void pickupItem()
	{
		Collider2D[] hitTargets1 = Physics2D.OverlapCircleAll(this.transform.position + (transform.up * ActorDefs.GLOBAL_PICKUP_OFFSET), ActorDefs.GLOBAL_PICKUP_RANGE, this.pickupLayer);
		Collider2D[] hitTargets2 = Physics2D.OverlapCircleAll(this.transform.position, ActorDefs.GLOBAL_PICKUP_RANGE, this.pickupLayer);
		Collider2D[] hitTargets = new Collider2D[hitTargets1.Length + hitTargets2.Length];
		hitTargets1.CopyTo(hitTargets, 0);
		hitTargets2.CopyTo(hitTargets, hitTargets1.Length);

		foreach (Collider2D target in hitTargets)
		{
			/* Make sure to only pickup valid objects. This will be expanded on eventually */
			if (WeaponDefs.canWeaponBePickedUp(target.gameObject))
			{
				Debug.Log("Picking up: " + target.gameObject.transform.parent.gameObject.name);
				if (this.equip(target.gameObject.transform.parent.gameObject))
				{
					//Make sure to only pick up one weapon
					break;
				}
			}

			/* TODO: Probably a better way to do this */
			if (this.tag == ActorDefs.playerTag)
			{
				if (MutationDefs.isMutationSelect(target.gameObject))
				{
					//TODO: implement mutation selection

					var mutationList = target.gameObject.GetComponents<MutationInterface>();

					foreach (MutationInterface mut in mutationList)
					{
						var existingMuts = mutationHolder.GetComponents(mut.GetType());
						if (existingMuts.Length > 0)
						{
							continue;
						}
						MutationInterface newMut = (MutationInterface)mutationHolder.AddComponent(mut.GetType());
						if (null != (newMut = newMut.mEquip(this)))
						{
							if (newMut.getMutationType() == mutationTrigger.ACTIVE_SLOT)
							{
								if (activeSlots[0] == null)
								{
									activeSlots[0] = newMut;
								}
							}
							break;
						}
					}
				}
				else if (PickupDefs.canBePickedUp(target.gameObject))
				{
					Debug.Log("Picking up: " + target.gameObject.transform.parent.gameObject.name);
					PickupInterface pickup = target.gameObject.transform.parent.gameObject.GetComponent<PickupInterface>();
					if (pickup != null)
					{
						pickup.pickup(this);
					}
				}
			}
		}
	}

	public void setAttackTarget(Actor targetActor)
	{
		attackTarget = targetActor;
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
			actorData.maxSpeed = changedSpeed;
		}
	}

	private static void setWeaponTag(GameObject weapon, string newTag)
	{
		weapon.tag = newTag;
		foreach (Transform child in weapon.transform)
		{
			child.tag = newTag;
			foreach (Transform secChild in child)
			{
				secChild.tag = newTag;
			}
		}
	}

	public float takeDamage(float damage)
	{
		actorData.health -= damage;
		actorData.health = actorData.health < 0 ? 0 : actorData.health;

		if (actorData.health <= 0.0F)
		{
			this.kill();
		}
		return actorData.health;
	}

	public void throwWeapon()
	{
		drop();
		return;
		Vector3 pointerPos = actorBody.transform.TransformDirection(transform.up);
		throwWeapon(pointerPos);
	}

	public void throwWeapon(Vector3 throwTargetPos)
	{
		if (!equippedWeaponInt.canBeDropped())
		{
			return;
		}

		drop();
		return;
		Vector3 aimDir = new Vector3(throwTargetPos.x, throwTargetPos.y, 0) - this.transform.position;
		
		this.equippedWeaponInt.throwWeapon(Vector3.ClampMagnitude(aimDir, 1));

		resetEquip();
	}

	public void triggerDamageEffects(Actor target)
	{
		var mutationList = mutationHolder.GetComponents<MonoBehaviour>();

		foreach (MutationInterface mutation in mutationList)
		{
			if (mutation.getMutationType() == mutationTrigger.DAMAGE_GIVEN)
			{
				mutation.trigger(target);
			}
		}
	}

	public void setStun(bool stun)
	{
		foreach (MonoBehaviour script in behaviorList)
		{
			script.enabled = !stun;
		}
	}

	public void useAction(short index)
	{
		if (activeSlots[index] != null)
		{
			activeSlots[index].trigger(this);
		}
	}

	private void resetEquip()
	{
		equippedWeapon.transform.SetParent(null, true);
		setWeaponTag(equippedWeapon, WeaponDefs.OBJECT_WEAPON_TAG);

		equippedWeapon = null;
		equippedWeaponInt = null;

		equipEmpty();
	}
	private void equipEmpty()
	{
		GameObject fistPrefab = instantiateWeapon(gameManager.weapPFist);

		equip(fistPrefab.transform.gameObject);
	}

	private void updatePointer()
	{
	}
}