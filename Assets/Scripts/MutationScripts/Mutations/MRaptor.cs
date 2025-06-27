using System.Collections;
using UnityEngine;
using static ActorDefs;

public class MRaptor : MonoBehaviour, MutationInterface
{
	public Animator anim;
	public Sprite icon;

	public Actor actorWielder;
	public AudioClip pounceSound;

	public Collider2D pounceCollider;

	public bool pounceActive;
	private float bufferTimer = 0F;
	private float transformTimer = 0F;
	private bool noTransform = false;

	private Vector2 pounceTarget;
	public float pounceDistance = 400F;
	private float pounceDistanceMoved = 0F;

	public float pounceSpeed = 1200F;

	public MutationScriptable mutationScriptable;
	public GameObject raptorClawPrefab;

	public ClawWeapon equippedClaw = null;

	public Sprite abilityIcon1;
	public Sprite abilityIcon2;

	private static int POUNCE_COST_INDEX = 0;
	private static int MUT_XFORM_TIMER_INDEX = 1;

	private GameManager gameManager;

	private void OnDestroy()
	{
		gameManager = GameManager.Instance;
		if (gameManager != null)
		{
			gameManager.playerAbilityEvent -= abilityInputPressed;
			gameManager.playerAbilitySecondaryEvent -= abilityInputSecondaryPressed;
			gameManager.updateCellCount -= updateCells;
			gameManager.signalMovementUnlocked();
			gameManager.signalRotationUnlocked();
		}
	}

	private void FixedUpdate()
	{
		if (pounceActive)
		{
			if (pounceDistanceMoved >= pounceDistance)
			{
				stopPounce(false);
			}

			//every frame, move player in pounce target direction
			pounceDistanceMoved += pounceSpeed * Time.deltaTime;
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

		if (transformTimer != 0)
		{
			transformTimer -= Time.deltaTime;
			if (transformTimer <= 0)
			{
				if (equippedClaw == null)
				{
					transformTimer = 0;
					actorWielder.equipEmpty();
					bufferTimer = MutationDefs.RAPTOR_XFORM_BUFF_TIMER;
				}
				else if (equippedClaw != null && !equippedClaw.pounceAttackActive)
				{
					transformTimer = 0;
					Destroy(equippedClaw.gameObject);
					actorWielder.equipEmpty();
					equippedClaw = null;
					bufferTimer = MutationDefs.RAPTOR_XFORM_BUFF_TIMER;
				}
			}
		}
	}

	public void updateCells(int amount)
	{
		gameManager.playerStats.playerHUD.setMutAbilityFill(mutationScriptable.mutCost, mutationScriptable.values[POUNCE_COST_INDEX]);
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
			else if (transformTimer <= 0)
			{
				return;
			}

			anim.SetTrigger(MutationDefs.TRIGGER_RAPTOR_PSTART);
			bufferTimer = MutationDefs.ABILITY_BUFF_TIMER;
			noTransform = false;

			gameManager.playSound(actorWielder.actorAudioSource, pounceSound.name, 1F);
		}
	}

	private void abilityInputSecondaryPressed()
	{
		if (bufferTimer <= 0 && !pounceActive)
		{
			float currMutEnergy = actorWielder.gameManager.playerStats.getMutationBar();
			/* pouncing while transformed is free */
			if (transformTimer <= 0 && currMutEnergy >= mutationScriptable.values[POUNCE_COST_INDEX])
			{
				actorWielder.gameManager.playerStats.changeMutationBar(Mathf.RoundToInt(-mutationScriptable.values[POUNCE_COST_INDEX]));
			}
			else if (transformTimer <= 0)
			{
				return;
			}

			anim.SetTrigger(MutationDefs.TRIGGER_RAPTOR_PSTART);
			bufferTimer = MutationDefs.ABILITY_BUFF_TIMER;
			if (transformTimer <= 0)
			{
				transformTimer = 0.5F;
				noTransform = true;
			}

			gameManager.playSound(actorWielder.actorAudioSource, pounceSound.name, 1F);
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

	public void trigger(Actor actorTarget)
	{

	}

	public MutationInterface mEquip(Actor actor)
	{
		gameManager = GameManager.Instance;
		gameManager.playerAbilityEvent += abilityInputPressed;
		gameManager.playerAbilitySecondaryEvent += abilityInputSecondaryPressed;
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

		setWielder(actor);
		bufferTimer = 0;

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
		return mutationScriptable.tutorialSprites;
	}

	public void startPounce()
	{
		PlayerInputs playerIn = actorWielder.GetComponentInChildren<PlayerInputs>();
		if (playerIn != null)
		{
			pounceTarget = Vector2.ClampMagnitude(playerIn.pointerPos() - (Vector2)actorWielder.transform.position, 1);
		}

		gameManager.signalMovementLocked();
		gameManager.signalRotationLocked();
		actorWielder.setActorCollision(false, new string[] { GameManager.OBJECT_MID_LAYER });

		pounceActive = true;
		pounceDistanceMoved = 0F;

		if (transformTimer <= 0 && noTransform == false)
		{
			transformTimer = mutationScriptable.values[MUT_XFORM_TIMER_INDEX];
		}

		if (equippedClaw == null)
		{
			raptorClawPrefab.SetActive(false);
			GameObject clawObject = Instantiate(raptorClawPrefab);
			if (clawObject != null)
			{
				clawObject.SetActive(true);
				raptorClawPrefab.SetActive(true);
				actorWielder.equip(clawObject);
				equippedClaw = clawObject.GetComponentInChildren<ClawWeapon>();
			}
		}

		pounceCollider.enabled = true;
	}

	public void stopPounce(bool targetHit)
	{
		anim.SetTrigger(MutationDefs.TRIGGER_RAPTOR_PEND);
		anim.ResetTrigger(MutationDefs.TRIGGER_RAPTOR_PSTART);

		pounceActive = false;
		pounceDistanceMoved = 0F;

		pounceTarget = Vector3.zero;
		pounceCollider.enabled = false;


		if (!targetHit)
		{
			gameManager.signalMovementUnlocked();
			gameManager.signalRotationUnlocked();
		}
		actorWielder.setActorCollision(true, null);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision != null)
		{
			Actor actorHit = collision.gameObject.GetComponentInChildren<Actor>();
			if (actorHit != null)
			{
				EnemyMove enemyMove = actorHit.GetComponentInChildren<EnemyMove>();
				if (enemyMove != null)
				{
					enemyMove.setStunResponse(actorWielder);
					EffectDefs.effectApply(actorHit, gameManager.effectManager.stun1);
				}
				if (equippedClaw != null)
				{
					stopPounce(true);
					actorWielder.actorBody.velocity = Vector3.zero;
					equippedClaw.triggerPounceAttack(actorHit, mutationScriptable.damage, actorWielder);
					actorWielder.actorBody.rotation = actorWielder.aimAngle(actorHit.transform.position);
					return;
				}
			}
			stopPounce(false);
		}
	}
}