using System.Collections;
using System.Drawing;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public Animator anim;

	public Rigidbody2D body;
	public BoxCollider2D projectileCollider;

	public float projectileLaunchForce = 1000F;

	public Actor actorHit = null;
	private Actor actorOrigin = null;

	private static string TRIGGER_PROJECTILE_HIT = "TriggerProjectileHit";

	private float expireTimer = 0F;
	private static float expireLength = 3F;

	private void FixedUpdate()
	{
		if (expireTimer != 0)
		{
			expireTimer -= Time.deltaTime;
			if (expireTimer <= 0)
			{
				anim.SetTrigger(TRIGGER_PROJECTILE_HIT);
			}
		}
	}

	public void destroySelf()
	{
		Destroy(gameObject);
	}

	public void enableCollider()
	{
		projectileCollider.enabled = true;
	}

	public void launch(Vector2 origin, Vector2 target, Actor actorOrigin)
	{
		float rotation = (Mathf.Atan2((target - origin).y, (target - origin).x) * Mathf.Rad2Deg) - 90F;
		transform.SetPositionAndRotation(origin, Quaternion.Euler(0, 0, rotation));

		body.AddForce(Vector2.ClampMagnitude(target - origin, 1) * (projectileLaunchForce + (target - actorOrigin.actorBody.velocity).magnitude));
		expireTimer = expireLength;

		this.actorOrigin = actorOrigin;
	}

	public void processHit()
	{
		if (actorHit != null)
		{
			EffectDefs.effectApply(actorHit, actorHit.gameManager.effectManager.stun3);
			//TODO: add visual
		}
	}

	public void triggerCollision(Collider2D collision)
	{
		if (collision != null && actorHit == null)
		{
			actorHit = collision.gameObject.GetComponentInChildren<Actor>();
			if (actorHit == actorOrigin)
			{
				actorHit = null;
			}
			else
			{
				body.velocity = Vector2.zero;
				anim.SetTrigger(TRIGGER_PROJECTILE_HIT);
			}
		}
	}
}