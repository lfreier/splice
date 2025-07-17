using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static WeaponDefs;

public class ClawWeapon : BasicWeapon
{
	public SecondWeapon secondClaw;
	public atkState currState;
	private float clawAttackTimer;
	public float attackCancelFramesTime = 0.8F;

	private Actor pounceSourceActor;
	private Actor pounceActorToHit;
	private float pounceDamage;
	public bool pounceAttackActive = false;

	public SpriteRenderer shredTrailSprite;
	public SpriteRenderer shredTrailSprite2;

	public MRaptor raptorParent;

	public AudioClip pounceShredSound;
	public float pounceShredSoundVolume = 0.5F;

	private void OnDestroy()
	{
		GameManager gm = GameManager.Instance;
		if (gm != null)
		{
			gm.signalMovementUnlocked();
			gm.signalRotationUnlocked();
		}
	}

	private void FixedUpdate()
	{
		if (clawAttackTimer != 0)
		{
			clawAttackTimer -= Time.deltaTime;
			if (clawAttackTimer <= 0)
			{
				anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_TIMEOUT);
				anim.ResetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
				anim.ResetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK_SEC);
				currState = atkState.IDLE;
				clawAttackTimer = 0;
			}
		}
	}

	override public void init()
	{
		clawAttackTimer = 0;
		currState = atkState.IDLE;
		base.init();
	}

	override public bool attack(LayerMask targetLayer)
	{
		if (pounceAttackActive == true)
		{
			return false;
		}
		actorWielder.invincible = false;
		soundMade = false;
		clawAttackTimer = 0;

		switch (currState)
		{
			case atkState.LEFT:
				currState = atkState.RIGHT;
				anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK_SEC);
				break;
			case atkState.RIGHT:
				currState = atkState.LEFT;
				anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
				break;
			case atkState.IDLE:
			default:
				anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
				currState = atkState.LEFT;
				break;
		}

		if (_weaponScriptable.soundSwing != null)
		{
			AudioClip toPlay;
			gameManager.audioManager.soundHash.TryGetValue(_weaponScriptable.soundSwing.name, out toPlay);
			if (toPlay != null)
			{
				weaponAudioPlayer.PlayOneShot(toPlay, gameManager.effectsVolume);
			}
		}

		return true;
	}

	override public void setStartingPosition(bool side)
	{
		transform.parent.SetLocalPositionAndRotation(new Vector3(_weaponScriptable.equipPosX, _weaponScriptable.equipPosY, 0), Quaternion.Euler(0, 0, _weaponScriptable.equipRotZ));
		anim.enabled = true;
	}

	override public bool toggleCollider(int enable)
	{
		if (enable > 0)
		{
			clawAttackTimer = attackCancelFramesTime;
		}
		return base.toggleCollider(enable);
	}

	public bool toggleSecondWeaponCollider(int enable)
	{
		if (secondClaw != null)
		{
			return secondClaw.toggleCollider(enable);
		}
		return false;
	}


	public void stopPounceAttack()
	{
		actorWielder.gameManager.signalMovementUnlocked();
		actorWielder.gameManager.signalRotationUnlocked();
		pounceAttackActive = false;


		if (shredTrailSprite != null)
		{
			shredTrailSprite.enabled = false;
		}

		if (shredTrailSprite2 != null)
		{
			shredTrailSprite2.enabled = false;
		}
	}

	public void toggleShredTrailSprite1(int toggle)
	{
		shredTrailSprite.enabled = toggle > 0;
	}

	public void toggleShredTrailSprite2(int toggle)
	{
		shredTrailSprite2.enabled = toggle > 0;
	}

	public void triggerPounceAttack(Actor actorToHit, float pounceDamagePerHit, Actor sourceActor)
	{
		pounceActorToHit = actorToHit;
		pounceSourceActor = sourceActor;
		pounceDamage = pounceDamagePerHit;
		pounceAttackActive = true;
		anim.SetTrigger(MutationDefs.TRIGGER_POUNCE_ATTACK);

		if (raptorParent != null)
		{
			playSound(raptorParent.raptorAudioPlayer, pounceShredSound.name, pounceShredSoundVolume);
		}
		sourceActor.actorBody.velocity = Vector2.zero;
	}

	public void triggerPounceDamage()
	{
		if (pounceActorToHit != null)
		{
			pounceActorToHit.takeDamage(pounceDamage, pounceSourceActor);
			pounceActorToHit.actorBody.velocity = Vector2.zero;
		}
		if (pounceSourceActor != null)
		{
			pounceSourceActor.actorBody.velocity = Vector2.zero;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		weaponHit(collision);
	}
}