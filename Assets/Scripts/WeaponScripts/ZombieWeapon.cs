using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using static UnityEngine.GraphicsBuffer;

public class ZombieWeapon : BasicWeapon
{
	public WeaponScriptable[] zombieAttacks;

	public float dashSpeed = 700;
	private bool dashActive = false;
	private Vector2 dashTarget;

	public void FixedUpdate()
	{
		if (dashActive)
		{
			//every frame, move towards dash target
			actorWielder.Move(dashSpeed * Time.deltaTime * dashTarget);
		}
	}

	//long wind up attack
	//small wind up, two swing attack
	override public bool attack(LayerMask targetLayer)
	{
		Actor target = getActorWielder().getAttackTarget();

		if (target == null)
		{
			return false;
		}

		//TODO: make it not hacky
		if (Vector3.Distance(transform.position, target.transform.position) <= zombieAttacks[0].npcAttackRange)
		{
			anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK_SEC);
		}
		else
		{
			anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK);
		}

		actorWielder.invincible = false;
		//actorWielder.actorAudioSource.PlayOneShot(weaponSwingSound);

		return true;
	}

	override public bool inRange(Vector3 target)
	{
		if (zombieAttacks.Length < 2)
		{
			return false;
		}
		return Vector3.Distance(transform.position, target) <= zombieAttacks[1].npcAttackRange;
	}

	public void startDash()
	{
		Actor actorTarget = getActorWielder().getAttackTarget();
		if (actorTarget != null)
		{
			dashTarget = Vector2.ClampMagnitude(actorTarget.transform.position - actorWielder.transform.position, 1);
		}
		
		actorWielder.setMovementLocked(true);
		dashActive = true;
	}

	public void stopDash()
	{
		actorWielder.setMovementLocked(false);
		dashTarget = actorWielder.transform.position;
		dashActive = false;
	}
}