using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MBeast : BasicWeapon, MutationInterface
{
	private enum atkState
	{
		IDLE = 0,
		RIGHT = 1,
		LEFT = 2,
		OVERHEAD  = 3
	}

	public MutationScriptable mutScriptable;
	private int INDEX_SHIELD = 0;

	private atkState currState;
	private bool animationRunning;

	public Sprite icon;

	public float transformLength;
	private float transformTimer;

	public Vector2 beastColliderSize;

	public Collider2D rightArmColl;
	public Collider2D leftArmColl;
	public Collider2D overheadColl;

	public SpriteRenderer[] beastSprites;

	public float dashSpeed = 800F;

	private bool dashActive = false;
	private bool transformActive = false;

	private float startingShield;

	private Vector3 dashTarget;

	public float attackCancelFramesTime = 1F;
	private float attackTimer;

	private float buffTimer;

	private BoxCollider2D playerCollider;
	private Vector2 playerColliderSize;

	public MGrabber grabber;
	public float objectReleaseForce = 20000F;
	public float grabOffset = 0.3125F;
	public float grabPickupRange = 0.5F;

	private bool attackWithObstacle = false;

	public LayerMask grabLayers;

	private void OnDestroy()
	{
		GameManager.Instance.playerAbilityEvent -= abilityInputPressed;
	}

	private void FixedUpdate()
	{
		if (attackTimer != 0)
		{
			attackTimer -= Time.deltaTime;
			if (attackTimer <= 0)
			{
				Debug.Log("attack timed out");
				anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_TIMEOUT);
				anim.ResetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
				anim.ResetTrigger(MutationDefs.TRIGGER_BEAST_ATK2);
				anim.ResetTrigger(MutationDefs.TRIGGER_BEAST_ATK3);
				currState = atkState.IDLE;
				animationRunning = false;
				attackTimer = 0;
			}
		}

		if (transformTimer != 0)
		{
			transformTimer -= Time.deltaTime;
			if (transformTimer <= 0)
			{
				currState = atkState.IDLE;
				animationRunning = false;
				transformTimer = 0;
				stopBeastForm();
			}
		}

		if (buffTimer != 0)
		{
			buffTimer -= Time.deltaTime;
			if (buffTimer <= 0)
			{
				buffTimer = 0;
			}
		}

		if (dashActive)
		{
			//every frame, move player towards
			actorWielder.Move(dashTarget * dashSpeed * Time.deltaTime);
		}
	}

	private void abilityInputPressed()
	{
		float currMutEnergy = actorWielder.gameManager.playerStats.getMutationBar();
		if ((buffTimer > 0  || currMutEnergy < mutScriptable.mutCost) && !transformActive)
		{
			return;
		}

		if (transformActive && grabber.heldRigidbody != null)
		{
			/* let go */
			grabber.releaseHeldObject(objectReleaseForce);
			return;
		}
		else if (transformActive && grabber.heldRigidbody == null)
		{
			/* try to grab object */
			if (grabber.grabObjects(grabLayers, grabPickupRange, weightClass.LIGHT, false))
			{
				if (grabber.heldObstacle != null)
				{
					grabber.heldObstacle.beingHeld = true;
				}
			}
			return;
		}

		actorWielder.gameManager.playerStats.changeMutationBar(-mutScriptable.mutCost);
		buffTimer = MutationDefs.ABILITY_BUFF_TIMER;
		playerCollider.size = beastColliderSize;

		/* set new shield */
		actorWielder.actorData.shield = actorWielder.actorData.shield + mutScriptable.values[INDEX_SHIELD];
		startingShield = actorWielder.actorData.shield;

		actorWielder.gameManager.signalUpdateShieldEvent(actorWielder.actorData.shield);

		/* equip weapon */
		actorWielder.equip(this.gameObject);

		foreach (SpriteRenderer currSprite in beastSprites)
		{
			currSprite.enabled = true;
		}

		/* enter transform animation */
		animationRunning = false;
		transformTimer = transformLength;
		currState = atkState.IDLE;
		transformActive = true;
		anim.SetTrigger(MutationDefs.TRIGGER_BEAST);
	}

	override public bool attack(LayerMask targetLayer)
	{
		/* need to wait for animation to finish */
		if (animationRunning)
		{
			return false;
		}

		actorWielder.invincible = false;

		Debug.Log(currState);

		switch (currState)
		{
			case atkState.OVERHEAD:
				currState = atkState.IDLE;
				animationRunning = false;
				return true;
			case atkState.LEFT:
				currState = atkState.OVERHEAD;
				anim.SetTrigger(MutationDefs.TRIGGER_BEAST_ATK3);
				break;
			case atkState.RIGHT:
				currState = atkState.LEFT;
				anim.SetTrigger(MutationDefs.TRIGGER_BEAST_ATK2);
				break;
			case atkState.IDLE:
			default:
				anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
				currState = atkState.RIGHT;
				break;
		}
		//lastTargetLayer = targetLayer;

		//actorWielder.actorAudioSource.PlayOneShot(weaponSwingSound);

		animationRunning = true;

		return true;
	}

	public void attackOver()
	{
		attackTimer = attackCancelFramesTime;
		animationRunning = false;
	}

	public string getDisplayName()
	{
		return MutationDefs.NAME_BEAST;
	}

	public Sprite getIcon()
	{
		return icon;
	}
	public string getId()
	{
		return "MBeast";
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.ACTIVE_SLOT;
	}

	public void init(Actor wielder)
	{
		setWielder(wielder);

		GameManager.Instance.playerAbilityEvent += abilityInputPressed;

		/* save collider size */
		playerCollider = wielder.GetComponentInChildren<BoxCollider2D>();
		if (playerCollider != null)
		{
			playerColliderSize = playerCollider.size;
		}

		if (beastSprites == null || beastSprites.Length <= 0)
		{
			beastSprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
		}

		grabber.grabOffset = grabOffset;
		grabber.mutCost1 = 0;
		grabber.mutCost2 = 0;

		attackTimer = 0;
		currState = atkState.IDLE;
		animationRunning = false;
	}

	override public bool isActive()
	{
		return animationRunning;
	}

	/* for an active slot mutation, this does nothing */
	public void trigger(Actor actorTarget)
	{
		return;
	}

	public MutationInterface mEquip(Actor actor)
	{
		actorWielder = actor;

		init(actor);

		return this;
	}

	public void setStartingPosition()
	{

	}

	public void setWielder(Actor wielder)
	{
		actorWielder = wielder;
		grabber.wielder = wielder;
	}

	public void startDash()
	{
		if (!transformActive)
		{
			return;
		}

		dashActive = true;

		actorWielder.gameManager.signalMovementLocked();
		actorWielder.gameManager.signalRotationLocked();

		PlayerInputs playerIn = actorWielder.GetComponentInChildren<PlayerInputs>();
		if (playerIn != null)
		{
			dashTarget = Vector2.ClampMagnitude(playerIn.pointerPos() - (Vector2)actorWielder.transform.position, 1);
		}
	}

	public void stopDash()
	{
		dashActive = false;
		actorWielder.gameManager.signalMovementUnlocked();
		actorWielder.gameManager.signalRotationUnlocked();
		dashTarget = Vector3.zero;
	}

	public void stopBeastForm()
	{
		transformActive = false;
		anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_TIMEOUT);
		anim.StopPlayback();
		stopDash();
		/* reset collider */
		if (playerCollider != null)
		{
			playerCollider.size = playerColliderSize;
		}

		foreach (SpriteRenderer currSprite in beastSprites)
		{
			currSprite.enabled = false;
		}

		if (actorWielder.actorData.shield == startingShield)
		{
			actorWielder.gameManager.signalUpdateShieldEvent(0);
		}
		startingShield = 0;

		grabber.releaseHeldObject(objectReleaseForce);

		actorWielder.equipEmpty();

		anim.SetTrigger(MutationDefs.TRIGGER_STOP_TRANSFORM);

		/* reset health */
		actorWielder.actorBody.velocity = Vector3.zero;
		EffectDefs.effectApply(actorWielder, actorWielder.gameManager.effectManager.stunParry);
	}

	/* 3 - left 
	 * 1 - right
	 * 2 - overhead
	 */
	public void toggleBeastHitbox(int toggle)
	{
		bool enable = toggle > 0;
		int sideToToggle = Mathf.Abs(toggle);

		if (sideToToggle == 3 && leftArmColl != null)
		{
			leftArmColl.enabled = enable;
		}
		else if (sideToToggle == 1 && rightArmColl != null)
		{
			if (grabber.heldObstacle != null && grabber.heldObstacle.beingHeld)
			{
				/* don't enable right fist hitbox when holding something */
				//grabber.heldObstacle.obstacleBody.simulated = enable;
				grabber.heldObstacleCollider.isTrigger = true;
				grabber.heldObstacle.gameObject.layer = enable ? LayerMask.NameToLayer(GameManager.DAMAGE_LAYER) : grabber.heldObstacle.startingLayer;
				attackWithObstacle = enable;
				rightArmColl.enabled = false;
				return;
			}
			rightArmColl.enabled = enable;
		}
		else if (sideToToggle == 2 && overheadColl != null)
		{
			overheadColl.enabled = enable;
		}
	}

	public void triggerCollision(Collider2D collision)
	{
		if (currState == atkState.OVERHEAD)
		{
			Actor actorHit = weaponHit(collision, mutScriptable.damage, 0);
			if (actorHit != null)
			{
				EffectDefs.effectApply(actorHit, actorHit.gameManager.effectManager.stun1);
				currState = atkState.IDLE;
			}
		}
		else if (currState != atkState.IDLE)
		{
			Obstacle bashingObstacle = grabber.heldObstacle;
			if (bashingObstacle != null && bashingObstacle.beingHeld && attackWithObstacle)
			{
				weaponHit(collision, _weaponScriptable.damage + bashingObstacle._obstacleScriptable.heldDamageBonus, 0);
				bashingObstacle.reduceDurability(1);
			}
			else
			{
				weaponHit(collision, _weaponScriptable.damage, 0);
			}
		}
	}

	Sprite[] MutationInterface.getTutorialSprites()
	{
		return mutScriptable.tutorialSprites;
	}
}