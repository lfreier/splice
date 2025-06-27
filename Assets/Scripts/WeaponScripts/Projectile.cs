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

	public AudioClip projectileHitSound;

	private Actor actorOrigin = null;

	private static string TRIGGER_PROJECTILE_HIT = "TriggerProjectileHit";

	private Vector2 originVector;

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
		float originVelocity = actorOrigin.actorBody.velocity.magnitude;
		Debug.Log("extra vel: " + originVelocity);
		body.AddForce(Vector2.ClampMagnitude(target - origin, 1) * (projectileLaunchForce + originVelocity));
		expireTimer = expireLength;
		originVector = origin;
		this.actorOrigin = actorOrigin;
	}

	public void processHit()
	{
		if (actorHit != null)
		{
			actorHit.gameManager.playSound(actorHit.actorAudioSource, projectileHitSound.name, 1F);
			EnemyMove enemyMove = actorHit.GetComponentInChildren<EnemyMove>();
			if (enemyMove != null)
			{
				enemyMove.setStunResponse(originVector);
			}
			EffectDefs.effectApply(actorHit, actorHit.gameManager.effectManager.stunParry);
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
				processHit();
			}
		}
	}
}