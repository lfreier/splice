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

	private Vector3 dashTarget;

	public float attackCancelFramesTime = 1F;
	private float attackTimer;

	private float buffTimer;

	private BoxCollider2D playerCollider;
	private Vector2 playerColliderSize;

	private void OnDestroy()
	{
		actorWielder.gameManager.playerAbilityEvent -= abilityInputPressed;
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
		if (buffTimer > 0  || currMutEnergy < mutScriptable.mutCost)
		{
			return;
		}

		buffTimer = MutationDefs.ABILITY_BUFF_TIMER;
		playerCollider.size = beastColliderSize;

		/* set new health */

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

		wielder.gameManager.playerAbilityEvent += abilityInputPressed;

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

		actorWielder.equipEmpty();

		anim.SetTrigger(MutationDefs.TRIGGER_STOP_TRANSFORM);

		/* reset health */
		actorWielder.actorBody.velocity = Vector3.zero;
		EffectDefs.effectApply(actorWielder, actorWielder.gameManager.effectManager.stunParry);
	}

	/* 0 - left 
	 * 1 - right
	 * 2 - overhead
	 */
	public void toggleBeastHitbox(int sideToToggle)
	{
		Collider2D checkInitial;
		if (sideToToggle == 0 && leftArmColl != null)
		{
			checkInitial = leftArmColl;
			leftArmColl.enabled = !leftArmColl.enabled;
		}
		else if (sideToToggle == 1 && rightArmColl != null)
		{
			checkInitial = rightArmColl;
			rightArmColl.enabled = !rightArmColl.enabled;
		}
		else if (sideToToggle == 2 && overheadColl != null)
		{
			checkInitial = overheadColl;
			overheadColl.enabled = !overheadColl.enabled;
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
			weaponHit(collision, _weaponScriptable.damage, 0);
		}
	}
}