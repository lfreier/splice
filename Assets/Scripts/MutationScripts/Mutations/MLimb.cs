using System.Collections;
using UnityEngine;

public class MLimb : MonoBehaviour, MutationInterface, MultiBoxCollider
{
	Actor actorWielder;

	[SerializeField] public Animator anim;

	[SerializeField] public Collider2D hitbox;
	[SerializeField] public Collider2D hitboxHand;

	[SerializeField] public MutationScriptable mutationScriptable;

	public void colliderEnter(Collider2D collision, MultiBoxCollider childScript)
	{
		this.OnTriggerEnter2D(collision);
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.ACTIVE_SLOT;
	}

	public void trigger(Actor actorTarget)
	{
		anim.SetTrigger("Attack");
	}

	public MutationInterface mEquip(Actor actor)
	{
		/* Need to equip a new prefab */
		actorWielder = actor;

		GameObject mLimbPrefab = actor.instantiateActive(actor.gameManager.mutPLimb);
		actor.equipActive(mLimbPrefab);

		MLimb newScript = mLimbPrefab.GetComponentInChildren<MLimb>();

		newScript.setWielder(actor);
		newScript.hitbox.isTrigger = false;

		return newScript;
	}
	public void setStartingPosition()
	{
		this.transform.parent.SetLocalPositionAndRotation(mutationScriptable.startingPosition, Quaternion.Euler(0, 0, mutationScriptable.startingRotation));
	}

	public void setWielder(Actor wielder)
	{
		actorWielder = wielder;
	}

	public bool toggleCollider()
	{
		hitboxHand.enabled = !hitboxHand.enabled;
		return hitbox.enabled = !hitbox.enabled;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		//TODO: deal with layermasks in a way that actually makes sense later
		Transform currParent = this.gameObject.transform;
		while (currParent != null)
		{
			if (collision.name == currParent.name)
			{
				Debug.Log("Stop hitting yourself");
				return;
			}
			currParent = currParent.transform.parent;
		}

		Actor actorHit = collision.GetComponent<Actor>();
		if (actorHit != null)
		{
			actorWielder.triggerDamageEffects(actorHit);
			actorHit.takeDamage(mutationScriptable.damage);
			Debug.Log("Hit: " + collision.name + " for " + mutationScriptable.damage + " damage");
		}
		else
		{
			Debug.Log("Hit: " + collision.name);
		}
	}
}