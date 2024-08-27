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
		anim.SetTrigger("AttackSecondary");
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
	
	private void OnTriggerEnter2D(Collider2D collision)
	{
		WeaponInterface weapon = collision.gameObject.GetComponent<WeaponInterface>();
		if (weapon != null)
		{
			Actor target = weapon.getActorWielder();
			if (target != null)
			{
				attackerDisarm(target);
				EffectDefs.effectApply(target, GameManager.EFCT_SCRIP_ID_STUN1);
				WeaponScriptable weapData = weapon.getScriptable();
				Vector3 force = Vector3.ClampMagnitude(target.actorBody.transform.position - actorWielder.transform.position, 1);
				target.actorBody.AddForce(force * 1500 * weapData.knockbackDamage * target._actorScriptable.knockbackResist);
			}
		}
	}
}