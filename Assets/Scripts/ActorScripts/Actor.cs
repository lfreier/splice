using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using static EffectDefs;

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
		public float maxHealth;

		public float maxSpeed;
		public float moveSpeed;

		public float acceleration;
		public float deceleration;

		public float hearingRange;
		public float sightAngle;
		public float sightRange;

		public float frightenedDistance;
	};

	public enum detectMode
	{
		nul = -1,
		idle = 0,
		suspicious = 1,
		seeking = 2,
		lost = 3,
		hostile = 4,
		frightened = 5,
		getWeapon = 6
	};

	public PlayerHUD hud;

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

	public GameObject[] droppedItems;

	public GameManager gameManager;

	private GameObject equippedWeapon;
	public WeaponInterface equippedWeaponInt;

	public Sprite corpseSprite;
	public GameObject corpsePrefab;

	private SpriteRenderer sprite;
	private SpriteRenderer weaponSprite;

	private float speedCheck;

	private bool invincible = false;

	public void Start()
	{
		speedCheck = 0;
		attackTarget = null;
		gameManager = GameManager.Instance;
		activeSlots = new MutationInterface[MutationDefs.MAX_SLOTS];
		sprite = GetComponent<SpriteRenderer>();
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
		actorData.maxHealth = _actorScriptable.health;

		actorData.maxSpeed = _actorScriptable.maxSpeed;
		actorData.moveSpeed = _actorScriptable.moveSpeed;
		actorData.acceleration = _actorScriptable.acceleration;
		actorData.deceleration = _actorScriptable.deceleration;

		actorData.hearingRange = _actorScriptable.hearingRange;
		actorData.sightAngle = _actorScriptable.sightAngle;
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
		weaponSprite.sortingLayerName = WeaponDefs.SORT_LAYER_GROUND;

		resetEquip();
	}

	public void dropItem()
	{
		foreach (GameObject item in droppedItems)
		{
			GameObject newDrop = Instantiate(item, this.transform.position, this.transform.rotation, this.transform.parent);
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
				Destroy(equippedWeapon);
			}
			else
			{
				return false;
			}
		}
		equippedWeapon = tempWeap.gameObject;
		equippedWeaponInt = tempWeapInt;

		equippedWeapon.transform.SetParent(this.transform, true);
		weaponSprite = equippedWeapon.GetComponentInChildren<SpriteRenderer>();
		weaponSprite.sortingLayerName = WeaponDefs.SORT_LAYER_CHARS;
		equippedWeaponInt.setStartingPosition();

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

	public Camera getCamera()
	{
		return Camera.main;
	}
	public GameObject instantiateActive(GameObject prefab)
	{
		GameObject activePrefab = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
		MutationInterface mutScript = activePrefab.GetComponentInChildren<MutationInterface>();

		activePrefab.transform.SetParent(this.gameObject.transform, false);
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
			//SceneManager.LoadScene(SceneDefs.GAME_OVER_SCENE, LoadSceneMode.Additive);
		}
		Destroy(transform.gameObject);
	}

	public void Move(Vector3 moveVector)
	{
		currMoveVector = new Vector3(moveVector.x, moveVector.y, moveVector.z);
		actorBody.velocity = moveVector;
	}

	public bool pickupItem()
	{
		Collider2D[] hitTargets1 = Physics2D.OverlapCircleAll(this.transform.position + (transform.up * ActorDefs.GLOBAL_PICKUP_OFFSET), ActorDefs.GLOBAL_PICKUP_RANGE, this.pickupLayer);
		Collider2D[] hitTargets2 = Physics2D.OverlapCircleAll(this.transform.position, ActorDefs.GLOBAL_PICKUP_RANGE, this.pickupLayer);
		Collider2D[] hitTargets = new Collider2D[hitTargets1.Length + hitTargets2.Length];
		hitTargets1.CopyTo(hitTargets, 0);
		hitTargets2.CopyTo(hitTargets, hitTargets1.Length);

		foreach (Collider2D target in hitTargets)
		{
			GameObject targetObject = target.gameObject;

			PickupBox pickupBox = target.gameObject.GetComponent<PickupBox>();
			if (pickupBox != null && pickupBox.hasPickup())
			{
				targetObject = pickupBox.getPickup();
			}

			/* Make sure to only pickup valid objects. This will be expanded on eventually */
			if (WeaponDefs.canWeaponBePickedUp(targetObject))
			{
				Debug.Log("Picking up: " + targetObject.transform.gameObject.name);
				if (this.equip(targetObject.transform.gameObject))
				{
					//Make sure to only pick up one weapon
					return true;
				}
			}

			/* TODO: Probably a better way to do this */
			if (this.tag == ActorDefs.playerTag)
			{
				if (MutationDefs.isMutationSelect(targetObject))
				{
					//TODO: implement mutation selection

					var mutationList = targetObject.GetComponents<MutationInterface>();

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
									return true;
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
						return true;
					}
				}
			}
		}

		return false;
	}

	public void setAttackTarget(Actor targetActor)
	{
		attackTarget = targetActor;
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

	/* Logic to handle taking damage from any source
	 * only applies effects that originate from this actor
	 * returns actual damage taken (health cannot be negative)
	 */
	public float takeDamage(float damage)
	{
		if (this.invincible)
		{
			return 0F;
		}

		float startingHealth = actorData.health;
		actorData.health -= damage;
		actorData.health = actorData.health < 0 ? 0 : actorData.health;

		if (actorData.health <= 0.0F)
		{
			this.kill();
		}

		if (this.tag.Equals(ActorDefs.playerTag))
		{
			EffectDefs.effectApply(this, GameManager.EFCT_SCRIP_ID_IFRAME1);
			hud.updateHealth(actorData.health);
		}
		else
		{
			EffectDefs.effectApply(this, GameManager.EFCT_SCRIP_ID_IFRAME0);
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
		WeaponDefs.setWeaponTag(equippedWeapon, WeaponDefs.OBJECT_WEAPON_TAG);

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