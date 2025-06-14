using System.Collections;
using UnityEngine;
using static ActorDefs;
using static UnityEngine.UI.Image;

public class MSpider : MonoBehaviour, MutationInterface
{
	public Animator anim;

	public Actor actorWielder;

	public Sprite icon;

	public MutationScriptable mutationScriptable;

	public GameObject projectilePrefab;

	public Vector3 shootOffset;

	public Collider2D fangCollider;

	private Actor actorToDamage;
	private float damagePerAttack;

	private bool animationActive;

	public bool pounceActive;
	private Vector2 pounceTarget;
	public float pounceSpeed = 1500F;

	public Sprite abilityIcon1;
	public Sprite abilityIcon2;

	private PlayerInputs playerIn;

	private static int MUT_SEC_COST_INDEX = 0;
	private static int MUT_FANG_DMG_INDEX = 1;
	private static int MUT_FANG_HEAL_INDEX = 2;

	private GameManager gameManager;

	private void OnDestroy()
	{
		gameManager.playerAbilityEvent -= abilityInputPressed;
		gameManager.playerAbilitySecondaryEvent -= abilitySecondaryInputPressed;
		gameManager.updateCellCount -= updateCells;
	}

	private void FixedUpdate()
	{
		if (pounceActive)
		{
			//every frame, move player in pounce target direction
			actorWielder.Move(pounceTarget * pounceSpeed * Time.deltaTime);
		}
	}

	private void updateCells(int amount)
	{
		gameManager.playerStats.playerHUD.setMutAbilityFill(mutationScriptable.mutCost, mutationScriptable.values[MUT_SEC_COST_INDEX]);
	}

	private void abilityInputPressed()
	{
		if (mutationScriptable.mutCost <= gameManager.playerStats.getMutationBar() && !animationActive)
		{
			anim.SetTrigger(MutationDefs.TRIGGER_SPIDER_STING);
			gameManager.playerStats.changeMutationBar(-mutationScriptable.mutCost);
			animationActive = true;
		}
	}

	private void abilitySecondaryInputPressed()
	{
		if (mutationScriptable.values[MUT_SEC_COST_INDEX] <= gameManager.playerStats.getMutationBar() && !animationActive)
		{
			anim.SetTrigger(MutationDefs.TRIGGER_SPIDER_SHOOT);
			gameManager.playerStats.changeMutationBar(Mathf.RoundToInt(-mutationScriptable.values[MUT_SEC_COST_INDEX]));
			animationActive = true;
		}
	}

	public void shootLeft()
	{
		Vector2 origin = new Vector2(-shootOffset.x, shootOffset.y);
		shoot(this.transform.TransformPoint(origin));
	}

	public void shootRight()
	{
		Vector2 origin = new Vector2(shootOffset.x, shootOffset.y);
		shoot(this.transform.TransformPoint(origin));
	}

	public void shoot(Vector2 origin)
	{
		Vector2 point = origin;
		if (playerIn != null)
		{
			point = playerIn.pointerPos();
		}
		GameObject projectileObj = Instantiate(projectilePrefab, origin, Quaternion.identity, null);
		Projectile projectile = projectileObj.GetComponentInChildren<Projectile>();
		if (projectile != null)
		{
			projectile.launch(origin, point, actorWielder);
		}
	}

	public void startPounce()
	{
		pounceTarget = Vector2.ClampMagnitude(playerIn.pointerPos() - (Vector2)actorWielder.transform.position, 1);
		pounceActive = true;
		gameManager.signalMovementLocked();
		gameManager.signalRotationLocked();
	}

	public void stopAbility()
	{
		gameManager.signalMovementUnlocked();
		gameManager.signalRotationUnlocked();

		animationActive = false;
		actorToDamage = null;
	}

	public void stopPounce()
	{
		pounceTarget = Vector2.zero;
		pounceActive = false;
		actorWielder.actorBody.velocity = Vector3.zero;
	}

	public void enableFangCollider(int enable)
	{
		fangCollider.enabled = enable > 0;
	}

	public void triggerFangAttack(Actor actorHit)
	{
		EnemyMove enemyMove = actorHit.GetComponentInChildren<EnemyMove>();
		if (enemyMove != null)
		{
			if ((enemyMove._detection == detectMode.idle || enemyMove._detection == detectMode.suspicious || enemyMove._detection == detectMode.wandering) && !enemyMove.summoned)
			{
				enemyMove.setStunResponse(actorWielder);
				EffectDefs.effectApply(actorHit, gameManager.effectManager.stun1);
			}
			else if (actorHit.isStunned())
			{
				enemyMove.setStunResponse(actorWielder);
				actorHit.addStun(1F);
			}
			else
			{
				/* only stunned or unaware targets can be stung */
				return;
			}
		}

		actorToDamage = actorHit;

		damagePerAttack = mutationScriptable.values[MUT_FANG_DMG_INDEX];

		pounceTarget = Vector2.zero;
		pounceActive = false;

		/* split damage up if it will kill*/
		if (actorHit.actorData.health < (damagePerAttack * 4))
		{
			damagePerAttack = actorHit.actorData.health / 4;
		}

		actorToDamage.actorBody.velocity = Vector3.zero;
		actorWielder.actorBody.velocity = Vector3.zero;
		actorWielder.actorBody.rotation = actorWielder.aimAngle(actorHit.transform.position);

		anim.SetTrigger(MutationDefs.TRIGGER_SPIDER_STING_HIT);
	}

	public void triggerFangDamage()
	{
		if (actorToDamage != null)
		{
			float startingHp = actorToDamage.actorData.health;
			actorToDamage.takeDamage(damagePerAttack);

			if (startingHp <= damagePerAttack)
			{
				actorWielder.takeHeal(mutationScriptable.values[MUT_FANG_HEAL_INDEX]);
			}
		}
	}

	public string getDisplayName()
	{
		return MutationDefs.NAME_SPIDER;
	}
	public Sprite getIcon()
	{
		return icon;
	}
	public string getId()
	{
		return "MSpider";
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
		gameManager.playerAbilitySecondaryEvent += abilitySecondaryInputPressed;
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

		animationActive = false;
		setWielder(actor);

		return this;
	}
	public void setStartingPosition()
	{
	}

	public void setWielder(Actor wielder)
	{
		actorWielder = wielder;
		playerIn = actorWielder.GetComponentInChildren<PlayerInputs>();
	}

	Sprite[] MutationInterface.getTutorialSprites()
	{
		return mutationScriptable.tutorialSprites;
	}
}