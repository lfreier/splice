using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;

public class ActionParry : MonoBehaviour, ActionInterface
{
	public Animator anim;
	public Collider2D parryHitbox;

	private Actor actorWielder;

	public void action()
	{
		anim.SetTrigger(WeaponDefs.ANIM_TRIGGER_ATTACK_SEC);
	}

	private void attackerDisarm(Actor attacker)
	{
		attacker.drop();
	}

	public void setActorToHold(Actor actor)
	{
		actorWielder = actor;
	}

	public bool toggleHitbox()
	{
		return parryHitbox.enabled = !parryHitbox.enabled;
	}
	
	public void setDamagedSprite()
	{

	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		WeaponInterface weapon = collision.gameObject.GetComponent<WeaponInterface>();
		if (weapon != null)
		{
			Actor target = weapon.getActorWielder();
			if (target != null)
			{
				attackerDisarm(target);
				EffectDefs.effectApply(target, target.gameManager.effectManager.stunParry);
				WeaponScriptable weapData = actorWielder.equippedWeaponInt.getScriptable();
				
				Vector3 force = Vector3.ClampMagnitude(target.actorBody.transform.position - actorWielder.transform.position, 1);
				float forceMult = Mathf.Min(WeaponDefs.KNOCKBACK_MULT_PARRY * weapData.knockbackDamage * (1 - target._actorScriptable.knockbackResist), ActorDefs.MAX_PARRY_FORCE);
				Debug.Log("Parry force: " + forceMult);
				target.actorBody.AddForce(force * forceMult);

				//actorWielder.actorAudioSource.PlayOneShot(actorWielder.gameManager.audioManager.parry);
			}
		}
	}
}