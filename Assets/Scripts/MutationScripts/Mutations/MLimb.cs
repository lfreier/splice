using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class MLimb : MonoBehaviour, MutationInterface
{
	public Actor actorWielder;

	public Sprite icon;
	public Sprite holdingIcon;
	public Sprite handIcon;

	public float grabOffset = 0.3125F;
	public float grabPickupRange = 0.75F;
	public float objectReleaseForce = 20000F;

	public LayerMask grabLayers;

	public MGrabber grabber;

	public enum retracted {
		IDLE = 0,
		ACTIVE = 1,
		RETRACTED = 2,
		BUSY = 3
	};

	public retracted limbState;

	private bool collisionHit;

	private GameManager gameManager;

	[SerializeField] public Animator anim;

	[SerializeField] public Collider2D hitbox;
	[SerializeField] public Collider2D hitboxHand;

	[SerializeField] public MutationScriptable mutationScriptable;

	public Sprite abilityIcon1;
	public Sprite abilityIcon2;

	public MLimbBlade blade;
	public Collider2D bladeCollider;

	public AudioClip limbRetract;
	public AudioClip limbStretch;

	public AudioClip bladeExtendSound;

	public AudioSource limbAudioPlayer;

	private int bladeOver = 0;

	private static int MUT_GRAB2_COST_INDEX = 0;
	private static int MUT_SEC_COST_INDEX = 1;

	private void OnDestroy()
	{
		gameManager = GameManager.Instance;
		if (gameManager != null)
		{
			gameManager.playerInteractEvent -= interactInputPressed;
			gameManager.playerInteractReleaseEvent -= interactInputReleased;
			gameManager.playerAbilityEvent -= abilityInputPressed;
			gameManager.playerAbilitySecondaryEvent -= abilityInputSecondaryPressed;
			gameManager.playerAbilityReleaseEvent -= abilityInputReleased;
			gameManager.updateCellCount -= updateCells;
		}
	}

	private void FixedUpdate()
	{
		if (collisionHit && !hitbox.IsTouchingLayers(Physics2D.GetLayerCollisionMask(hitbox.gameObject.layer)) && !hitboxHand.IsTouchingLayers(Physics2D.GetLayerCollisionMask(hitbox.gameObject.layer)))
		{
			actorWielder.gameManager.signalRotationUnlocked();
			collisionHit = false;
		}
	}

	public void updateCells(int amount)
	{
		gameManager.playerStats.playerHUD.setMutAbilityFill(mutationScriptable.mutCost, mutationScriptable.values[MUT_SEC_COST_INDEX]);
	}

	private void abilityInputPressed()
	{
		if (limbState == retracted.IDLE)
		{
			if (grabber.heldRigidbody != null)
			{
				grabber.releaseHeldObject(objectReleaseForce);
				return;
			}
			anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
			Debug.Log("Limb state is now active from pressing");
			limbState = retracted.ACTIVE;

			gameManager.playSound(limbAudioPlayer, limbStretch.name, 1F);
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
			Debug.Log("Limb state is now idle from releasing");
			limbState = retracted.IDLE;
			if (grabber.heldRigidbody != null)
			{
				grabber.releaseHeldObject(objectReleaseForce);
			}
			else
			{
				abilityInputPressed();
			}
		}
	}

	private void abilityInputSecondaryPressed()
	{
		if (limbState == retracted.IDLE && (gameManager.playerStats.getMutationBar() >= mutationScriptable.values[MUT_SEC_COST_INDEX]))
		{
			gameManager.playerStats.changeMutationBar(-Mathf.RoundToInt(mutationScriptable.values[MUT_SEC_COST_INDEX]));
			if (grabber.heldRigidbody != null)
			{
				grabber.releaseHeldObject(objectReleaseForce);
			}

			if (blade != null)
			{
				blade.collisions.Clear();
			}

			limbState = retracted.BUSY;
			anim.SetTrigger(MutationDefs.TRIGGER_LIMB_BLADE);

			gameManager.playSound(limbAudioPlayer, bladeExtendSound.name, 1F);
		}
	}

	public void bladeAnimOver()
	{
		bladeOver++;
		if (bladeOver >= 2)
		{
			limbState = retracted.IDLE;
			bladeOver = 0;
			Debug.Log("Blade anim over");
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
			if (grabber.grabObjects(grabLayers, grabPickupRange, weightClass.MID, true))
			{
				anim.SetTrigger("Retract");
				gameManager.playSound(limbAudioPlayer, limbRetract.name, 1F);
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

		gameManager = GameManager.Instance;
		/* subscribe to the necessary events for button presses */
		gameManager.playerInteractEvent += interactInputPressed;
		gameManager.playerInteractReleaseEvent += interactInputReleased;
		gameManager.playerAbilityEvent += abilityInputPressed;
		gameManager.playerAbilitySecondaryEvent += abilityInputSecondaryPressed;
		gameManager.playerAbilityReleaseEvent += abilityInputReleased;
		gameManager.updateCellCount += updateCells;

		if (abilityIcon1 != null && abilityIcon2 != null)
		{
			gameManager.playerStats.playerHUD.abilityIconImage1.sprite = abilityIcon1;
			gameManager.playerStats.playerHUD.abilityIconImage2.sprite = abilityIcon2;
		}
		else
		{
			gameManager.playerStats.playerHUD.abilityIconImage1.sprite = null;
			gameManager.playerStats.playerHUD.abilityIconImage2.sprite = null;
		}

		grabber.grabOffset = grabOffset;
		grabber.mutCost1 = mutationScriptable.mutCost;
		grabber.mutCost2 = Mathf.RoundToInt(mutationScriptable.values[MUT_GRAB2_COST_INDEX]);

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
	public void limbRetracted()
	{
		//if holding the button, get ready to release
		//otherwise, just pickup
		if (grabber.heldRigidbody != null)
		{
			Debug.Log("Limb state is now retracted from animation");
			limbState = retracted.RETRACTED;
		}
		else
		{
			Debug.Log("Limb state is now idle from animation");
			limbState = retracted.IDLE;
		}

		if (grabber.grabbedObject != null && grabber.heldRigidbody == null)
		{
			actorWielder.pickupItem(grabber.grabbedObject);
		}

		grabber.grabbedObject = null;
	}

	public void setStartingPosition()
	{
		this.transform.parent.SetLocalPositionAndRotation(mutationScriptable.startingPosition, Quaternion.Euler(0, 0, mutationScriptable.startingRotation));
	}

	public void setWielder(Actor wielder)
	{
		actorWielder = wielder;
		gameManager = GameManager.Instance;
		grabber.wielder = wielder;
	}

	public void playRetractSound()
	{
		gameManager.playSound(limbAudioPlayer, limbRetract.name, 1F);
	}

	/* called when the limb stretch animatino is finished - automatically retract */
	public void retractLimb()
	{
		Debug.Log("Limb state is now active");
		anim.SetTrigger("Retract");
	}

	public bool toggleCollider(int enable)
	{
		if (bladeOver <= 1 && bladeCollider != null)
		{
			bladeCollider.enabled = enable > 0;
		}
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

	Sprite[] MutationInterface.getTutorialSprites()
	{
		return mutationScriptable.tutorialSprites;
	}
}