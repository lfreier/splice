using JetBrains.Annotations;
using System.Collections;
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

	public LayerMask hitLayer;
	public LayerMask pickupLayer;

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
		equipEmpty();
		health = actorData.health;
		speedCheck = 0;
	}

	public void FixedUpdate()
	{
		speedCheck -= Time.deltaTime;
	}

	public float takeDamage(float damage)
	{
		health -= damage;
		health = health < 0 ? 0 : health;
		return health;
	}

	public void Move(Vector3 moveVector)
	{
		controller.Move(moveVector);
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

	/* 
	 * @param weaponToEquip needs to be a GameObject with BoxCollider2d and WeaponInterface script (swing/stab/shoot/etc.)
	 */
	public bool equip(GameObject weaponToEquip)
	{
		WeaponInterface tempWeapInt = weaponToEquip.GetComponent<WeaponInterface>();

		if (tempWeapInt == null)
		{
			return false;
		}
		/*  */
		if (equippedWeaponInt != null)
		{
			if (equippedWeaponInt.canBeDropped())
			{
				drop();
			}
			else if (equippedWeaponInt.getType() == WeaponType.UNARMED)
			{
				GameObject unarmedParent = equippedWeapon.transform.parent.gameObject;
				Destroy(equippedWeapon);
				Destroy(unarmedParent);
			}
		}

		equippedWeapon = weaponToEquip;
		equippedWeaponInt = tempWeapInt;
		weaponToEquip.transform.parent.SetParent(this.transform, true);
		equippedWeaponInt.setStartingPosition();
		equippedWeapon.tag = WeaponDefs.EQUIPPED_WEAPON_TAG;

		return true;
	}

	public void drop()
	{
		if (equippedWeaponInt == null || equippedWeaponInt.canBeDropped()) { return; }

		Vector3 pointerPos = actorBody.transform.TransformDirection(Vector3.forward);

		/* Literally just getting positive/negatives */
		Vector3 aimDir = new Vector3(pointerPos.x, pointerPos.y, 0) - this.transform.position;
		Vector3 translate = new Vector3(Random.Range(0.2F * Mathf.Sign(aimDir.x), 0.6F * Mathf.Sign(aimDir.x)), Random.Range(0.2F * Mathf.Sign(aimDir.y), 0.6F * Mathf.Sign(aimDir.y)), 0);

		equippedWeapon.transform.parent.Translate(translate, Space.World);
		equippedWeapon.transform.parent.Rotate(new Vector3(0, 0, Random.Range(-45, 45)), Space.Self);

		resetEquip();
	}


	public void throwWeapon()
	{
		Vector3 pointerPos = actorBody.transform.TransformDirection(Vector3.forward);

		if (equippedWeaponInt.canBeDropped())
		{
			throwWeapon(pointerPos);
		}
	}

	public void throwWeapon(Vector3 throwTargetPos)
	{
		Vector3 aimDir = new Vector3(throwTargetPos.x, throwTargetPos.y, 0) - this.transform.position;

		equippedWeaponInt.throwWeapon(Vector3.ClampMagnitude(aimDir, 1F));

		resetEquip();
	}

	private void resetEquip()
	{
		equippedWeapon.transform.parent.SetParent(null, true);
		equippedWeapon.tag = WeaponDefs.OBJECT_WEAPON_TAG;

		equippedWeapon = null;
		equippedWeaponInt = null;

		equipEmpty();
	}
	private void equipEmpty()
	{
		GameObject fistPrefab = (GameObject)Instantiate(Resources.Load("Weapons/Fists"), new Vector3(0, 0, 0), Quaternion.identity);
		fistPrefab.transform.parent = transform;
		fistPrefab.transform.position = new Vector3(0.25F, 0.25F, 0);
		fistPrefab.tag = WeaponDefs.EQUIPPED_WEAPON_TAG;

		equip(fistPrefab.transform.GetChild(0).gameObject);
	}

	private void updatePointer()
	{
	}
}