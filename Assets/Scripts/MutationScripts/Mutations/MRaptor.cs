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

	public AudioSource raptorAudioPlayer;

	public Sprite abilityIcon1;
	public Sprite abilityIcon2;

	public float aiPounceCooldown = 3F;
	private float aiPounceTimer;
	public float enemyAiPounceRange = 3F;
	public bool isEnemyAi = false;

	private static int POUNCE_COST_INDEX = 0;
	private static int MUT_XFORM_TIMER_INDEX = 1;

	private GameManager gameManager;

	void Start()
	{
		if (isEnemyAi)
		{
			gameManager = GameManager.Instance;
		}
	}

	private void OnDestroy()
	{
		gameManager = GameManager.Instance;
		if (gameManager != null)
		{
			if (!isEnemyAi)
			{
				gameManager.playerAbilityEvent -= abilityInputPressed;
				gameManager.playerAbilitySecondaryEvent -= abilityInputSecondaryPressed;
				gameManager.updateCellCount -= updateCells;
			}

			if (actorWielder.tag == playerTag)
			{
				gameManager.signalMovementUnlocked();
				gameManager.signalRotationUnlocked();
			}
			else
			{
				actorWielder.setMovementLocked(false);
				actorWielder.setRotationLocked(false);
			}
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

		if (aiPounceTimer != 0)
		{
			aiPounceTimer -= Time.deltaTime;
			if (aiPounceTimer <= 0)
			{
				aiPounceTimer = 0;
			}
		}

		if (transformTimer != 0 && !isEnemyAi)
		{
			transformTimer -= Time.deltaTime;
			if (transformTimer <= 0)
			{
				GameObject weap = actorWielder.getEquippedWeapon();
				if (weap != null)
				{
					equippedClaw = weap.GetComponentInChildren<ClawWeapon>();
				}
				if (equippedClaw == null)
				{
					transformTimer = 0;
					bufferTimer = MutationDefs.RAPTOR_XFORM_BUFF_TIMER;
				}
				else if (equippedClaw != null && !equippedClaw.pounceAttackActive)
				{
					transformTimer = 0;
					Destroy(equippedClaw.transform.parent.gameObject);
					actorWielder.unarmedPrefab = gameManager.prefabManager.weapPFist;
					actorWielder.equipEmpty();
					equippedClaw = null;
					bufferTimer = MutationDefs.RAPTOR_XFORM_BUFF_TIMER;
				}
			}
		}

		if (isEnemyAi)
		{
			Actor target = actorWielder.getAttackTarget();
			if (target != null && (target.transform.position - actorWielder.transform.position).magnitude < enemyAiPounceRange)
			{
				abilityInputSecondaryPressed();
				if (equippedClaw == null && actorWielder != null)
				{
					equippedClaw = actorWielder.GetComponentInChildren<ClawWeapon>();
					if (equippedClaw != null)
					{
						equippedClaw.raptorParent = this;
					}
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
			if (transformTimer <= 0 && currMutEnergy >= mutationScriptable.mutCost && !isEnemyAi)
			{
				actorWielder.gameManager.playerStats.changeMutationBar(-mutationScriptable.mutCost);
			}
			else if ((transformTimer <= 0 && !isEnemyAi) || (isEnemyAi && aiPounceTimer != 0))
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
			if (transformTimer <= 0 && currMutEnergy >= mutationScriptable.values[POUNCE_COST_INDEX] && !isEnemyAi)
			{
				actorWielder.gameManager.playerStats.changeMutationBar(Mathf.RoundToInt(-mutationScriptable.values[POUNCE_COST_INDEX]));
			}
			else if ((transformTimer <= 0 && !isEnemyAi) || (isEnemyAi && aiPounceTimer != 0))
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
		if (!isEnemyAi)
		{
			gameManager.playerAbilityEvent += abilityInputPressed;
			gameManager.playerAbilitySecondaryEvent += abilityInputSecondaryPressed;
			gameManager.updateCellCount += updateCells;
		}

		if (abilityIcon1 != null && abilityIcon2 != null && !isEnemyAi)
		{
			gameManager.playerStats.playerHUD.abilityIconImage1.sprite = abilityIcon1;
			gameManager.playerStats.playerHUD.abilityIconImage2.sprite = abilityIcon2;
		}
		else if (!isEnemyAi)
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
		else if (isEnemyAi)
		{
			pounceTarget = Vector2.ClampMagnitude(actorWielder.getAttackTarget().transform.position - actorWielder.transform.position, 1);
		}

		if (actorWielder.tag == playerTag)
		{
			gameManager.signalMovementLocked();
			gameManager.signalRotationLocked();
		}
		else
		{
			actorWielder.setMovementLocked(true);
			actorWielder.setRotationLocked(true);
		}
		actorWielder.setActorCollision(false, new string[] { GameManager.OBJECT_MID_LAYER });

		pounceActive = true;
		pounceDistanceMoved = 0F;

		if (transformTimer <= 0 && noTransform == false && !isEnemyAi)
		{
			transformTimer = mutationScriptable.values[MUT_XFORM_TIMER_INDEX];
		}

		if (equippedClaw == null && !isEnemyAi)
		{
			raptorClawPrefab.SetActive(false);
			GameObject clawObject = Instantiate(raptorClawPrefab);
			if (clawObject != null)
			{
				actorWielder.unarmedPrefab = raptorClawPrefab;
				clawObject.SetActive(true);
				raptorClawPrefab.SetActive(true);
				actorWielder.equip(clawObject);
				equippedClaw = clawObject.GetComponentInChildren<ClawWeapon>();
				equippedClaw.raptorParent = this;
			}
		}

		if (equippedClaw != null)
		{
			actorWielder.speedCheck = equippedClaw.getSpeed();
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
			if (actorWielder.tag == playerTag)
			{
				gameManager.signalMovementUnlocked();
				gameManager.signalRotationUnlocked();
			}
			else
			{
				actorWielder.setMovementLocked(false);
				actorWielder.setRotationLocked(false);
			}
		}

		if (isEnemyAi)
		{
			aiPounceTimer = aiPounceCooldown;
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
				else if (actorHit.tag == playerTag)
				{
					EffectDefs.effectApply(actorHit, gameManager.effectManager.stun1);
					actorHit.actorBody.velocity = Vector2.zero;
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