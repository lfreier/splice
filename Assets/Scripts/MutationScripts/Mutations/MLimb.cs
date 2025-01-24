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
	private Rigidbody2D heldRigidbody;
	private Obstacle heldObstacle;

	private bool collisionHit;

	[SerializeField] public Animator anim;

	[SerializeField] public Collider2D hitbox;
	[SerializeField] public Collider2D hitboxHand;

	[SerializeField] public MutationScriptable mutationScriptable;

	private void OnDestroy()
	{
		actorWielder.gameManager.playerInteractEvent -= interactInputPressed;
		actorWielder.gameManager.playerInteractReleaseEvent -= interactInputReleased;
		actorWielder.gameManager.playerAbilityEvent -= abilityInputPressed;
		actorWielder.gameManager.playerAbilityReleaseEvent -= abilityInputReleased;
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
		bool objectCostsMut = false;

		if (limbState == retracted.ACTIVE)
		{
			Transform handTransform = transform.GetChild(0);
			Vector3 handGrabPos = handTransform.position + (handTransform.up * grabOffset);

			Collider2D[] hitTargets = Physics2D.OverlapCircleAll(handGrabPos, grabPickupRange, grabLayers);
			Transform currChild = null;
			foreach (Collider2D target in hitTargets)
			{
				if (target.GetComponentInChildren<WeaponInterface>() != null || target.GetComponentInParent<WeaponInterface>() != null)
				{
					currChild = target.gameObject.transform;
					break;
				}
				if (target.GetComponentInChildren<PickupInterface>() != null || target.GetComponentInParent<PickupInterface>() != null)
				{
					currChild = target.gameObject.transform;
					break;
				}
				PickupBox box = target.GetComponentInChildren<PickupBox>();
				Obstacle obstacle = target.GetComponentInChildren<Obstacle>();
				if (box == null)
				{
					box = target.GetComponentInParent<PickupBox>();
				}
				/* if trying to grab a pickup box, pick up the whole thing */
				if (box != null && obstacle == null)
				{
					GameObject pickup = box.getPickup();
					if (pickup != null)
					{
						currChild = pickup.transform;
						break;
					}
				}

				//TODO: grabbing actors

				if (obstacle == null)
				{
					obstacle = target.GetComponentInParent<Obstacle>();
				}
				if (obstacle != null && obstacle._obstacleScriptable.weight < weightClass.MID)
				{
					Rigidbody2D objBody = target.GetComponentInChildren<Rigidbody2D>();
					if (objBody == null)
					{
						objBody = target.GetComponentInParent<Rigidbody2D>();
					}
					if (objBody != null)
					{
						/* Now hold the object and wait to release */
						currChild = target.gameObject.transform;
						heldRigidbody = objBody;
						heldRigidbody.simulated = false;
						heldObstacle = obstacle;
						objectCostsMut = true;
						break;
					}
				}
			}

			while (currChild != null)
			{
				if (currChild.parent == null)
				{
					/* only spend mut energy for certain objects */
					float currMutEnergy = actorWielder.gameManager.playerStats.getMutationBar();
					if (!objectCostsMut || currMutEnergy >= mutationScriptable.mutCost)
					{
						grabbedObject = currChild.gameObject;
						grabbedObject.transform.SetParent(handTransform);
						grabbedObject.transform.SetPositionAndRotation(handGrabPos, grabbedObject.transform.rotation);
						actorWielder.gameManager.playerStats.changeMutationBar(-mutationScriptable.mutCost);
						anim.SetTrigger("Retract");
						break;
					}
				}
				currChild = currChild.parent;
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

		/* subscribe to the necessary events for button presses */
		wielder.gameManager.playerInteractEvent += interactInputPressed;
		wielder.gameManager.playerInteractReleaseEvent += interactInputReleased;
		wielder.gameManager.playerAbilityEvent += abilityInputPressed;
		wielder.gameManager.playerAbilityReleaseEvent += abilityInputReleased;

		/* when limb is equipped, player can only attach from the right side */
		wielder.setAttackOnly(WeaponDefs.ANIM_BOOL_ONLY_RIGHT);

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
		}

		heldRigidbody.AddForce(Vector2.ClampMagnitude(releaseTarget - heldRigidbody.transform.position, 1) * releaseForce);
		heldRigidbody = null;
		heldObstacle = null;
	}

	public void setStartingPosition()
	{
		this.transform.parent.SetLocalPositionAndRotation(mutationScriptable.startingPosition, Quaternion.Euler(0, 0, mutationScriptable.startingRotation));
	}

	public void setWielder(Actor wielder)
	{
		actorWielder = wielder;
	}

	public void temp()
	{
		Debug.Log("Limb state is now active");
		anim.SetTrigger("Retract");
	}

	public bool toggleCollider()
	{
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
			actorWielder.gameManager.signalRotationLocked();
		}

		Actor actorHit = collision.gameObject.GetComponent<Actor>();
		if (actorHit != null)
		{
			actorHit.actorBody.AddForce(1000 * (this.gameObject.transform.parent.position - actorHit.transform.position));
			actorWielder.triggerDamageEffects(actorHit);
			actorHit.takeDamage(mutationScriptable.damage);
			Debug.Log("Hit: " + collision.name + " for " + mutationScriptable.damage + " damage");
		}
		else
		{
			Debug.Log("Hit: " + collision.name);
		}
	}
}