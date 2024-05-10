using JetBrains.Annotations;
using System.Collections;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Actor class:
 * Contains data and data interface for basic actors. 
 * Will be used as an interface for interacting with the Actor's 'body'
 */

public class Actor : MonoBehaviour
{
	public enum detectMode
	{
		idle = 0,
		suspicious = 1,
		hostile = 2,
		frightened = 3,
		cautious = 4
	};

	public detectMode detection;
	public detectMode oldDetection;

	public ActorScriptable actorData;
	public Controller2D controller;
	public Rigidbody2D actorBody;

	private Actor attackTarget;
	private Vector3 moveTarget;

	public LayerMask hitLayer;
	public LayerMask pickupLayer;

	public GameObject mutationHolder;
	public GameObject effectHolder;

	public GameManager gameManager;

	private GameObject equippedWeapon;
	private WeaponInterface equippedWeaponInt;
	private float speedCheck;

	private float health;

	public Actor(ActorScriptable actorScriptable)
	{
		actorData = actorScriptable;
		health = actorData.health;
	}

	public void Start()
	{
		health = actorData.health;
		speedCheck = 0;
		attackTarget = null;
		GameManager manager = GameManager.Instance;
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

	public void attack()
	{
		if (equippedWeaponInt == null || !equippedWeaponInt.isActive())
		{
			if (speedCheck <= 0)
			{
				equippedWeaponInt.attack(hitLayer);
				speedCheck = equippedWeaponInt.getSpeed();
			}
		}
	}

	public void drop()
	{
		if (equippedWeaponInt == null || equippedWeaponInt.canBeDropped()) { return; }

		Vector3 pointerPos = actorBody.transform.TransformDirection(Vector3.forward);

		/* Literally just getting positive/negatives */
		Vector3 aimDir = new Vector3(pointerPos.x, pointerPos.y, 0) - this.transform.position;
		Vector3 translate = new Vector3(Random.Range(0.2F * Mathf.Sign(aimDir.x), 0.6F * Mathf.Sign(aimDir.x)), Random.Range(0.2F * Mathf.Sign(aimDir.y), 0.6F * Mathf.Sign(aimDir.y)), 0);

		equippedWeapon.transform.Translate(translate, Space.World);
		equippedWeapon.transform.Rotate(new Vector3(0, 0, Random.Range(-45, 45)), Space.Self);

		resetEquip();
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
		if (equippedWeaponInt == null)
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

		if (this.tag.Equals(ActorDefs.npcTag) && targetActor.tag.Equals(ActorDefs.playerTag))
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
		controller.Move(moveVector);
	}

	public void pickupItem()
	{
		Collider2D[] hitTargets = Physics2D.OverlapCircleAll(this.transform.position, WeaponDefs.GLOBAL_PICKUP_RANGE, this.pickupLayer);

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
			else if (MutationDefs.isMutationSelect(target.gameObject))
			{
				//TODO: implement mutation selection

				var mutationList = target.gameObject.GetComponents<MutationInterface>();

				foreach (MutationInterface mut in mutationList)
				{
					mutationHolder.AddComponent(mut.GetType());
					if (mut.mEquip(this))
					{
						break;
					}
				}

			}
		}
	}

	public void setAttackTarget(Actor targetActor)
	{
		attackTarget = targetActor;
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
		health -= damage;
		health = health < 0 ? 0 : health;

		if (health <= 0.0F)
		{
			this.kill();
		}
		return health;
	}

	public void throwWeapon()
	{
		Vector3 pointerPos = actorBody.transform.TransformDirection(Vector3.forward);
		throwWeapon(pointerPos);
	}

	public void throwWeapon(Vector3 throwTargetPos)
	{
		if (!equippedWeaponInt.canBeDropped())
		{
			return;
		}

		Vector3 aimDir = new Vector3(throwTargetPos.x, throwTargetPos.y, 0) - this.transform.position;

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