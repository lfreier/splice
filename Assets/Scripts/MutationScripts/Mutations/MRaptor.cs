using System.Collections;
using UnityEngine;

public class MRaptor : MonoBehaviour, MutationInterface
{
	public Animator anim;
	public Sprite icon;

	public Actor actorWielder;

	public Collider2D pounceCollider;

	public bool pounceActive;
	private float bufferTimer = 0F;
	private float transformTimer = 0F;

	private Vector2 pounceTarget;

	public float pounceSpeed;

	public MutationScriptable mutationScriptable;
	public GameObject raptorClawPrefab;

	public ClawWeapon equippedClaw;

	private GameManager gameManager;

	private void OnDestroy()
	{
		gameManager.playerAbilityEvent -= abilityInputPressed;
	}

	private void FixedUpdate()
	{
		if (pounceActive)
		{
			//every frame, move player towards
			actorWielder.Move(pounceTarget * pounceSpeed * Time.deltaTime);
			//SoundDefs.createSound(actorWielder.transform.position, soundScriptable);
		}

		if (bufferTimer != 0)
		{
			bufferTimer -= Time.deltaTime;
			if (bufferTimer <= 0)
			{
				bufferTimer = 0;
			}
		}
	}

	private void abilityInputPressed()
	{
		if (bufferTimer <= 0 && !pounceActive)
		{
			float currMutEnergy = actorWielder.gameManager.playerStats.getMutationBar();
			/* pouncing while transformed is free */
			if (transformTimer <= 0 && currMutEnergy >= mutationScriptable.mutCost)
			{
				actorWielder.gameManager.playerStats.changeMutationBar(-mutationScriptable.mutCost);
			}
			else
			{
				return;
			}

			anim.SetTrigger(MutationDefs.TRIGGER_RAPTOR_PSTART);
			bufferTimer = MutationDefs.ABILITY_BUFF_TIMER;
		}
	}

	public string getDisplayName()
	{
		return MutationDefs.NAME_RAPTOR;
	}

	public Sprite getIcon()
	{
		return icon;
	}

	public string getId()
	{
		return "MRaptor";
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.ACTIVE_SLOT;
	}

	public void init(Actor wielder)
	{
		gameManager = GameManager.Instance;
		gameManager.playerAbilityEvent += abilityInputPressed;
		setWielder(wielder);
		bufferTimer = 0;
	}

	public void trigger(Actor actorTarget)
	{

	}

	public MutationInterface mEquip(Actor actor)
	{
		init(actor);

		return this;
	}

	public void setStartingPosition()
	{
		this.transform.SetLocalPositionAndRotation(mutationScriptable.startingPosition, Quaternion.Euler(0, 0, mutationScriptable.startingRotation));
	}

	public void setWielder(Actor wielder)
	{
		actorWielder = wielder;
	}

	Sprite[] MutationInterface.getTutorialSprites()
	{
		return null;
	}

	public void startPounce()
	{
		PlayerInputs playerIn = actorWielder.GetComponentInChildren<PlayerInputs>();
		if (playerIn != null)
		{
			pounceTarget = Vector2.ClampMagnitude(playerIn.pointerPos() - (Vector2)actorWielder.transform.position, 1);
		}

		actorWielder.gameManager.signalMovementLocked();
		actorWielder.gameManager.signalRotationLocked();

		pounceActive = true;

		actorWielder.setActorCollision(false);

		raptorClawPrefab.SetActive(false);
		GameObject clawObject = Instantiate(raptorClawPrefab);
		if (clawObject != null)
		{
			clawObject.SetActive(true);
			actorWielder.equip(clawObject);
			equippedClaw = clawObject.GetComponentInChildren<ClawWeapon>();
		}
	}

	public void stopPounce()
	{
		anim.SetTrigger(MutationDefs.TRIGGER_RAPTOR_PEND);

		pounceActive = false;
		actorWielder.setActorCollision(true);

		pounceTarget = Vector3.zero;

		actorWielder.gameManager.signalMovementUnlocked();
		actorWielder.gameManager.signalRotationUnlocked();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision != null)
		{
			Actor actorHit = collision.gameObject.GetComponentInChildren<Actor>();
			if (actorHit != null)
			{
				EffectDefs.effectApply(actorHit, gameManager.effectManager.stun1);
				if (equippedClaw != null)
				{
					equippedClaw.triggerPounceAttack(actorHit, mutationScriptable.damage, actorWielder);
				}
			}
			stopPounce();
		}
	}
}