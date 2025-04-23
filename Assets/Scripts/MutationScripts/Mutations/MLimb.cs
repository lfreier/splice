using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class MLimb : MonoBehaviour, MutationInterface
{
	Actor actorWielder;

	public Sprite icon;
	public Sprite holdingIcon;
	public Sprite handIcon;

	public float grabOffset = 0.3125F;
	public float grabPickupRange = 0.75F;
	public float objectReleaseForce = 20000F;
	public float releaseTargetOffset = 2F;

	public LayerMask grabLayers;

	public enum retracted {
		IDLE = 0,
		ACTIVE = 1,
		RETRACTED = 2
	};

	public retracted limbState;

	public GameObject grabbedObject;
	public Rigidbody2D heldRigidbody;
	private Obstacle heldObstacle;

	private bool collisionHit;

	private GameManager gameManager;

	[SerializeField] public Animator anim;

	[SerializeField] public Collider2D hitbox;
	[SerializeField] public Collider2D hitboxHand;

	[SerializeField] public MutationScriptable mutationScriptable;

	private void OnDestroy()
	{
		GameManager gm = GameManager.Instance;
		gm.playerInteractEvent -= interactInputPressed;
		gm.playerInteractReleaseEvent -= interactInputReleased;
		gm.playerAbilityEvent -= abilityInputPressed;
		gm.playerAbilityReleaseEvent -= abilityInputReleased;
	}

	private void FixedUpdate()
	{
		if (collisionHit && !hitbox.IsTouchingLayers(Physics2D.GetLayerCollisionMask(hitbox.gameObject.layer)) && !hitboxHand.IsTouchingLayers(Physics2D.GetLayerCollisionMask(hitbox.gameObject.layer)))
		{
			actorWielder.gameManager.signalRotationUnlocked();
			collisionHit = false;
		}
	}

	private void abilityInputPressed()
	{
		if (limbState == retracted.IDLE)
		{
			if (heldRigidbody != null)
			{
				releaseHeldObject();
				return;
			}
			anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
			Debug.Log("Limb state is now active from pressing");
			limbState = retracted.ACTIVE;
		}
		/*
		else if (limbState == retracted.ACTIVE)
		{
			Debug.Log("Limb state is now retracted from active");
			anim.SetTrigger("Retract");
		}
		*/
		else if (limbState == retracted.RETRACTED)
		{
			if (heldRigidbody != null)
			{
				releaseHeldObject();
			}
			Debug.Log("Limb state is now idle from releasing");
			limbState = retracted.IDLE;
		}
	}
	
	private void abilityInputReleased()
	{
		/*
		if (limbState == retracted.ACTIVE)
		{
			anim.SetTrigger("Retract");
		}
		*/
	}

	private void interactInputPressed()
	{
		/* Grab an object from the limb hand
		 * Automatically picks up weapons and pickups - big objects will always be held
		 */

		if (limbState == retracted.ACTIVE)
		{
			Transform handTransform = transform.GetChild(0);
			Vector3 handGrabPos = handTransform.position + (handTransform.up * grabOffset);

			Collider2D[] hitTargets = Physics2D.OverlapCircleAll(handGrabPos, grabPickupRange, grabLayers);
			foreach (Collider2D target in hitTargets)
			{
				if (grabTargetCollider(target))
				{
					/* grabbed object found */
					break;
				}
				else
				{
					continue;
				}
			}
		}
		else if (limbState == retracted.RETRACTED)
		{
			//release item
			Debug.Log("Limb state is now idle (this might be pointless)");
			limbState = retracted.IDLE;
		}
		else
		{
			//do nothing
		}
	}

	/* returns true if the target collider is grabbed
	 * a.k.a true to break, false to continue
	 */
	public bool grabTargetCollider(Collider2D target)
	{
		/* weapon has to only check for parent, because of equipped weapons */
		WeaponInterface weapon = target.GetComponentInParent<WeaponInterface>();

		PickupBox box = target.GetComponentInChildren<PickupBox>();
		Obstacle obstacle = target.GetComponentInChildren<Obstacle>();
		Actor actor = target.GetComponentInChildren<Actor>();
		PickupInterface pickup = target.GetComponentInChildren<PickupInterface>();

		if (weapon != null)
		{
			if (WeaponDefs.canWeaponBePickedUp(target.gameObject))
			{
				if (checkGrabObject(target.gameObject.transform, null, 0))
				{
					return true;
				}
			}
			return false;
		}

		if (obstacle == null)
		{
			obstacle = target.GetComponentInParent<Obstacle>();
		}
		if (obstacle != null && obstacle._obstacleScriptable.weight <= weightClass.MID)
		{
			int cost = mutationScriptable.mutCost;
			if (obstacle._obstacleScriptable.weight == weightClass.MID)
			{
				cost = Mathf.RoundToInt(mutationScriptable.values[0]);
			}
			Rigidbody2D objBody = target.GetComponentInChildren<Rigidbody2D>();
			if (objBody == null)
			{
				objBody = target.GetComponentInParent<Rigidbody2D>();
			}
			if (objBody != null)
			{
				/* Now hold the object and wait to release */
				if (checkGrabObject(target.gameObject.transform, null, cost))
				{
					heldRigidbody = objBody;
					heldRigidbody.simulated = false;
					heldObstacle = obstacle;
					WeaponDefs.setObjectLayer(WeaponDefs.SORT_LAYER_CHARS, heldObstacle.gameObject);
					return true;
				}
				return false;
			}
		}

		if (actor == null)
		{
			actor = target.GetComponentInParent<Actor>();
		}
		if (actor != null)
		{
			Rigidbody2D objBody = target.GetComponentInChildren<Rigidbody2D>();
			if (objBody == null)
			{
				objBody = target.GetComponentInParent<Rigidbody2D>();
			}
			if (objBody != null && actor.name != actorWielder.name)
			{
				/* Now hold the object and wait to release */
				if (checkGrabObject(target.gameObject.transform, actor, mutationScriptable.mutCost))
				{
					heldRigidbody = objBody;
					heldRigidbody.simulated = false;
					return true;
				}
				return false;
			}
		}

		if (box == null)
		{
			box = target.GetComponentInParent<PickupBox>();
		}
		/* if trying to grab a pickup box, pick up the whole thing */
		if (box != null && obstacle == null)
		{
			GameObject boxPickup = box.getPickup();
			if (boxPickup != null)
			{
				if (checkGrabObject(boxPickup.transform, null, 0))
				{
					return true;
				}
				return false;
			}
		}

		if (pickup == null)
		{
			pickup = target.GetComponentInParent<PickupInterface>();
		}
		if (pickup != null)
		{
			if (checkGrabObject(target.gameObject.transform, null, 0))
			{
				return true;
			}
		}

		return false;
	}

	public void addObjectToLimb(Transform objectTransform)
	{
		grabbedObject = objectTransform.gameObject;

		Rigidbody2D objBody = grabbedObject.GetComponentInChildren<Rigidbody2D>();
		if (objBody == null)
		{
			objBody = grabbedObject.GetComponentInParent<Rigidbody2D>();
		}
		if (objBody != null)
		{
			heldRigidbody = objBody;
			heldRigidbody.simulated = false;

			Obstacle obstacle = grabbedObject.GetComponentInChildren<Obstacle>();
			if (obstacle == null)
			{
				obstacle = grabbedObject.GetComponentInParent<Obstacle>();
			}
			if (obstacle != null)
			{
				heldObstacle = obstacle;
				WeaponDefs.setObjectLayer(WeaponDefs.SORT_LAYER_CHARS, heldObstacle.gameObject);

				Transform handTransform = transform.GetChild(0);
				Vector3 handGrabPos = handTransform.position + (handTransform.up * grabOffset);
				grabbedObject.transform.SetParent(handTransform);
				grabbedObject.transform.SetPositionAndRotation(handGrabPos, grabbedObject.transform.rotation);
			}
		}
	}

	private bool checkGrabObject(Transform currChild, Actor grabbedActor, int cost)
	{
		while (currChild != null)
		{
			if (currChild.parent == null)
			{
				/* only spend mut energy for certain objects */
				float currMutEnergy = gameManager.playerStats.getMutationBar();
				if (currMutEnergy >= cost)
				{
					grabbedObject = currChild.gameObject;

					Transform handTransform = transform.GetChild(0);
					Vector3 handGrabPos = handTransform.position + (handTransform.up * grabOffset);
					grabbedObject.transform.SetParent(handTransform);
					grabbedObject.transform.SetPositionAndRotation(handGrabPos, grabbedObject.transform.rotation);

					if (cost > 0)
					{
						gameManager.playerStats.changeMutationBar(-mutationScriptable.mutCost);
					}
					if (grabbedActor != null)
					{
						EffectDefs.effectApply(grabbedActor, grabbedActor.gameManager.effectManager.stunPermanent);
					}
					anim.SetTrigger("Retract");
					return true;
				}
				else
				{
					heldRigidbody = null;
				}
			}
			currChild = currChild.parent;
		}

		return false;
	}

	private void interactInputReleased()
	{

	}

	public string getDisplayName()
	{
		return MutationDefs.NAME_LIMB;
	}

	public Sprite getIcon()
	{
		return icon;
	}

	public string getId()
	{
		return "MLimb";
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.ACTIVE_SLOT;
	}


	private void init(Actor wielder)
	{
		setWielder(wielder);

		gameManager = GameManager.Instance;
		/* subscribe to the necessary events for button presses */
		gameManager.playerInteractEvent += interactInputPressed;
		gameManager.playerInteractReleaseEvent += interactInputReleased;
		gameManager.playerAbilityEvent += abilityInputPressed;
		gameManager.playerAbilityReleaseEvent += abilityInputReleased;

		/* when limb is equipped, player can only attach from the right side */
		wielder.setAttackOnly(WeaponDefs.ANIM_BOOL_ONLY_RIGHT, true);

		Debug.Log("Limb state is now idle");
		limbState = retracted.IDLE;
	}

	public MutationInterface mEquip(Actor actor)
	{
		actorWielder = actor;

		init(actor);

		return this;
	}

	/* Called when the "Retract" animation is finished
	 */
	private void limbRetracted()
	{
		//if holding the button, get ready to release
		//otherwise, just pickup
		if (heldRigidbody != null)
		{
			Debug.Log("Limb state is now retracted from animation");
			limbState = retracted.RETRACTED;
		}
		else
		{
			Debug.Log("Limb state is now idle from animation");
			limbState = retracted.IDLE;
		}

		if (grabbedObject != null && heldRigidbody == null)
		{
			actorWielder.pickupItem(grabbedObject);
		}

		grabbedObject = null;
	}

	private void releaseHeldObject()
	{
		float releaseForce = objectReleaseForce;
		heldRigidbody.simulated = true;
		heldRigidbody.transform.SetParent(null, true);

		Vector3 releaseTarget = actorWielder.transform.position + (actorWielder.transform.up * releaseTargetOffset);
		CollisionExclusion exclusion = heldRigidbody.AddComponent<CollisionExclusion>();

		/* Disable collisions for short amount of time to avoid player collision */
		Collider2D[] colliders1 = new Collider2D[actorWielder.actorBody.attachedColliderCount];
		actorWielder.actorBody.GetAttachedColliders(colliders1);
		Collider2D[] colliders2 = new Collider2D[heldRigidbody.attachedColliderCount];
		heldRigidbody.GetAttachedColliders(colliders2);
		exclusion.init(0.1F, colliders1, colliders2);
		if (heldObstacle != null)
		{
			heldObstacle.enablePhysics();
			releaseForce = Mathf.Min(objectReleaseForce, heldObstacle._obstacleScriptable.maxObstacleForce);
			WeaponDefs.setObjectLayer(WeaponDefs.SORT_LAYER_GROUND, heldObstacle.gameObject);
		}

		Actor grabbedActor = heldRigidbody.GetComponentInChildren<Actor>();
		if (grabbedActor != null)
		{
			EffectDefs.effectApply(grabbedActor, grabbedActor.gameManager.effectManager.stun1);
			releaseForce = objectReleaseForce / 4.5F;
		}

		heldRigidbody.AddForce(Vector2.ClampMagnitude(releaseTarget - heldRigidbody.transform.position, 1) * releaseForce);
		heldRigidbody = null;
		heldObstacle = null;
		grabbedActor = null;
	}

	public void setStartingPosition()
	{
		this.transform.parent.SetLocalPositionAndRotation(mutationScriptable.startingPosition, Quaternion.Euler(0, 0, mutationScriptable.startingRotation));
	}

	public void setWielder(Actor wielder)
	{
		actorWielder = wielder;
		gameManager = GameManager.Instance;
	}

	public void temp()
	{
		Debug.Log("Limb state is now active");
		anim.SetTrigger("Retract");
	}

	public bool toggleCollider()
	{
		/*
		if (!hitbox.enabled)
		{
			hitbox.gameObject.layer = LayerMask.NameToLayer(GameManager.OBJECT_MID_LAYER);
			hitboxHand.gameObject.layer = LayerMask.NameToLayer(GameManager.OBJECT_MID_LAYER);
		}
		else
		{
			hitbox.gameObject.layer = LayerMask.NameToLayer(GameManager.OBJECT_LAYER);
			hitboxHand.gameObject.layer = LayerMask.NameToLayer(GameManager.OBJECT_LAYER);
		}

		hitboxHand.enabled = !hitboxHand.enabled;
		return hitbox.enabled = !hitbox.enabled;
		*/
		return false;
	}

	/* for an active slot mutation, this does nothing */
	public void trigger(Actor actorTarget)
	{
		return;
	}

	public void triggerCollision(Collider2D collision)
	{
		OnTriggerEnter2D(collision);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		//TODO: deal with layermasks in a way that actually makes sense later
		Transform currParent = this.gameObject.transform;
		while (currParent != null)
		{
			if (collision.name == currParent.name)
			{
				Debug.Log("Stop hitting yourself");
				return;
			}
			currParent = currParent.transform.parent;
		}

		if (!collisionHit)
		{
			collisionHit = true;
			gameManager.signalRotationLocked();
		}

		Actor actorHit = collision.gameObject.GetComponent<Actor>();
		if (actorHit != null)
		{
			actorHit.actorBody.AddForce(1000 * (this.gameObject.transform.parent.position - actorHit.transform.position));
			actorWielder.triggerDamageEffects(actorHit);
			actorHit.takeDamage(mutationScriptable.damage, actorWielder);
			Debug.Log("Hit: " + collision.name + " for " + mutationScriptable.damage + " damage");
		}
		else
		{
			Debug.Log("Hit: " + collision.name);
		}
	}
}