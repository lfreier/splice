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
			Actor attacker = weapon.getActorWielder();
			if (attacker != null)
			{
				attackerDisarm(attacker);
				EffectDefs.effectStun(attacker, GameManager.EFCT_SCRIP_ID_STUN3);
			}
		}
	}
}