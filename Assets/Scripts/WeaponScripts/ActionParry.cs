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

	private void attackerStun(Actor attacker)
	{
		/* create new stun, attach it, and initialize it */
		EStun newStun = attacker.effectHolder.transform.AddComponent<EStun>();
		GameManager manager = GameManager.Instance;

		newStun.init(manager.getEffectScriptable(GameManager.EFCT_SCRIP_ID_STUN1));
		newStun.start(attacker);
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
				attackerStun(attacker);
			}
		}
	}
}