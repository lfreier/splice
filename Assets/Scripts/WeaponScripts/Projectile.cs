using System.Collections;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public enum projectileType
	{
		bullet = 0,
		spider = 1
	}

	public Animator anim;

	public Rigidbody2D body;
	public BoxCollider2D projectileCollider;

	public projectileType type = projectileType.bullet;
	public float projectileLaunchForce = 1000F;
	public float projectileDamage = 0;

	public Actor actorHit = null;

	public WeaponScriptable attachedWeaponScriptable;

	public AudioClip projectileHitSound;
	public SoundScriptable wallHitSound;

	private Actor actorOrigin = null;

	private static string TRIGGER_PROJECTILE_HIT = "TriggerProjectileHit";

	private Vector2 originVector;

	private float expireTimer = 0F;
	private static float expireLength = 3F;

	private bool soundMade = false;

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
		
		body.AddForce(Vector2.ClampMagnitude(target - origin, 1) * (projectileLaunchForce + originVelocity));
		expireTimer = expireLength;
		originVector = origin;
		this.actorOrigin = actorOrigin;
	}

	public void processHit(Collider2D collision)
	{
		if (actorHit != null)
		{
			actorHit.gameManager.playSound(actorHit.actorAudioSource, projectileHitSound.name, 1F);
			switch (type)
			{
				case projectileType.spider:
					processStringHit();
					break;
				case projectileType.bullet:
				default:
					processBulletHit(collision);
					break;
			}
		}
	}

	void processBulletHit(Collider2D collision)
	{
		float knockbackMult = 1;
		float maxForce = 10000;

		if (actorHit != null &&!actorHit.invincible)
		{
			actorHit.takeDamage(projectileDamage);
			knockbackMult = (1 - actorHit._actorScriptable.knockbackResist) * WeaponDefs.KNOCKBACK_MULT_ACTOR;

			if (attachedWeaponScriptable.soundActorHit != null)
			{
				GameManager.Instance.playSound(actorHit.actorAudioSource, attachedWeaponScriptable.soundActorHit.name, attachedWeaponScriptable.soundActorHitVolume);
			}

			maxForce = ActorDefs.MAX_HIT_FORCE;
			if (actorHit.isStunned())
			{
				maxForce = ActorDefs.MAX_PARRY_FORCE;
			}

			KnockbackTimer knockbackTimer = actorHit.AddComponent<KnockbackTimer>();
			knockbackTimer.init(actorHit._actorScriptable.knockbackResist);
		}
		else
		{
			if (collision.tag == SoundDefs.TAG_WALL_METAL)
			{
				SoundDefs.createSound(collision.transform.position, wallHitSound);
				if (attachedWeaponScriptable.soundWallHit != null)
				{
					GameManager.Instance.playSound(actorHit.actorAudioSource, attachedWeaponScriptable.soundWallHit.name, attachedWeaponScriptable.soundWallHitVolume);
				}
			}

			Obstacle obstacle = collision.GetComponent<Obstacle>();
			if (obstacle != null)
			{
				knockbackMult = obstacle._obstacleScriptable.weaponHitMult * WeaponDefs.KNOCKBACK_MULT_OBSTACLE;
				maxForce = obstacle._obstacleScriptable.maxObstacleForce;
				obstacle.knockOver();

				if (attachedWeaponScriptable.soundObstacleHit != null && !soundMade && knockbackMult != 0)
				{
					soundMade = true;
					GameManager.Instance.playSound(actorHit.actorAudioSource, attachedWeaponScriptable.soundObstacleHit.name, attachedWeaponScriptable.soundObstacleHitVolume);
				}
			}
		}

		/* knockback */
		Rigidbody2D hitBody = collision.attachedRigidbody;
		if (hitBody != null)
		{
			Vector3 force = Vector3.ClampMagnitude(hitBody.transform.position - this.transform.position, 1);
			float forceMult = Mathf.Min(attachedWeaponScriptable.knockbackDamage * knockbackMult, maxForce);
			hitBody.AddForce(force * forceMult);
		}
	}

	void processStringHit()
	{
		if (actorHit == null)
		{
			return;
		}
		EnemyMove enemyMove = actorHit.GetComponentInChildren<EnemyMove>();
		if (enemyMove != null)
		{
			enemyMove.setStunResponse(originVector);
		}
		EffectDefs.effectApply(actorHit, actorHit.gameManager.effectManager.stunParry);
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
				processHit(collision);
			}
		}
	}
}