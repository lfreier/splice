using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class Obstacle : MonoBehaviour
{
	public ObstacleScriptable _obstacleScriptable;
	public Rigidbody2D obstacleBody;
	public SpriteRenderer spriteRenderer;
	public Sprite secondSprite;

	public AudioSource obstacleAudioPlayer;

	private RigidbodyType2D obstacleType;
	private bool physicsEnabled = false;
	private bool thresholdPassed = false;
	private float velocityThreshold = 0.05F;
	public float knockVelocityThreshold = 5F;
	public float angVelocityThreshold = 20F;

	public float durability;

	public bool beingHeld;

	private float thresholdTimer;

	public int startingLayer;

	public float knockOverDrag = 3F;

	public float destroyOnTimer = 0F;

	public int basicPrefabIndex;

	private GameManager gm;

	// Use this for initialization
	void Start()
	{
		startingLayer = gameObject.layer;
		if (obstacleBody != null)
		{
			obstacleType = obstacleBody.bodyType;
		}
		durability = _obstacleScriptable.durability;
		beingHeld = false;
		physicsEnabled = false;
		gm = GameManager.Instance;
	}

	private void FixedUpdate()
	{
		if (physicsEnabled && obstacleBody != null && _obstacleScriptable.type != ObstacleDefs.type.KNOCK_OVER)
		{
			if (obstacleBody.velocity.magnitude > velocityThreshold)
			{
				thresholdPassed = true;
			}
			if (thresholdPassed && obstacleBody.velocity.magnitude <= velocityThreshold)
			{
				obstacleBody.bodyType = obstacleType;
				physicsEnabled = false;
				thresholdPassed = false;
				if (_obstacleScriptable.type == ObstacleDefs.type.KNOCK_OVER)
				{
					obstacleBody.drag *= knockOverDrag;
				}
			}
		}
		else if (physicsEnabled && obstacleBody != null)
		{
			if (thresholdPassed)
			{
				thresholdTimer -= Time.deltaTime;
				if (thresholdTimer <= 0)
				{
					obstacleBody.bodyType = obstacleType;
					physicsEnabled = false;
					thresholdPassed = false;
					obstacleBody.drag *= knockOverDrag;
					thresholdTimer = 0;
				}
			}
			//TODO: angular velocity
			if (obstacleBody.velocity.magnitude > knockVelocityThreshold || Mathf.Abs(obstacleBody.angularVelocity) > angVelocityThreshold)
			{
				thresholdPassed = true;
				thresholdTimer = 0.15F;
			}

			Debug.Log("Velocities: " + obstacleBody.velocity.magnitude + " " + obstacleBody.angularVelocity);
		}

		if (destroyOnTimer != 0)
		{
			destroyOnTimer -= Time.deltaTime;
			if (destroyOnTimer <= 0)
			{
				destroyOnTimer = 0;
				Destroy(this.gameObject);
			}
		}
	}

	private void pushByDamage(WeaponInterface weapon)
	{
	}

	public void reduceDurability(float amount)
	{
		durability -= amount;
		if (durability <= 0)
		{
			beingHeld = false;
			Destroy(this.gameObject);
		}
	}

	public void enablePhysics()
	{
		if (obstacleType == RigidbodyType2D.Static)
		{
			physicsEnabled = true;
			thresholdPassed = false;
			obstacleBody.bodyType = RigidbodyType2D.Dynamic;
		}
		else if (_obstacleScriptable.type == ObstacleDefs.type.KNOCK_OVER)
		{
			physicsEnabled = true;
			thresholdPassed = false;
			obstacleBody.drag /= knockOverDrag;
		}
	}

	public void knockOver()
	{
		if (_obstacleScriptable.type == ObstacleDefs.type.KNOCK_OVER)
		{
			if (secondSprite != null && spriteRenderer != null)
			{
				spriteRenderer.sprite = secondSprite;
				spriteRenderer.size = new Vector2(secondSprite.rect.width / 16, secondSprite.rect.height / 16);
				spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.7F);
				Collider2D[] collArr = new Collider2D[obstacleBody.attachedColliderCount];
				obstacleBody.GetAttachedColliders(collArr);
				foreach (Collider2D coll in collArr)
				{
					Destroy(coll);
				}
				Destroy(obstacleBody);
			}
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Actor actorSource = null;
		if (gameObject.name.Contains("actor"))
		{
			actorSource = transform.parent.GetComponentInChildren<Actor>();
		}
		
		Actor actorHit = collision.gameObject.GetComponent<Actor>();
		float collisionThreshold = _obstacleScriptable.collisionDamageThreshold;

		if (actorSource != null)
		{
			/* hard code this (cringe) */
			collisionThreshold = 1.5F;
		}

		if (actorHit != null)
		{	
			Vector2 velocityDiff = obstacleBody.velocity - (Vector2)actorHit.currMoveVector;

			//Debug.Log("Obstacle/Actor diff: " + velocityDiff.magnitude);
			if (velocityDiff.magnitude >= collisionThreshold)
			{
				knockOver();
				if (obstacleBody.velocity.magnitude > collisionThreshold / 2)
				{
					actorHit.actorBody.AddForce(velocityDiff * _obstacleScriptable.actorPushForce);
					EffectDefs.effectApply(actorHit, gm.effectManager.stunHalf);
					/* hitstop if player is hit */
					actorHit.takeDamage(_obstacleScriptable.collisionDamage, actorHit);

					/* if two actors colliding, damage/stun first actor */
					if (actorSource != null)
					{
						gm.playSound(actorHit.actorAudioSource, gm.audioManager.obstacleActorHit.name, 1F);
						EffectDefs.effectApply(actorSource, gm.effectManager.stunHalf);
						actorSource.takeDamage(_obstacleScriptable.collisionDamage, actorHit);
					}
					else
					{
						gm.playSound(obstacleAudioPlayer, gm.audioManager.obstacleActorHit.name, 1F);
					}
				}
			}
		}
		else if (gm != null)
		{
			if (obstacleBody.velocity.magnitude > collisionThreshold / 4)
			{
				/* if two actors colliding, use correct audio player */
				if (actorSource != null)
				{
					Obstacle hitObs = collision.gameObject.GetComponentInChildren<Obstacle>();
					if (hitObs != null)
					{
						gm.playSound(hitObs.obstacleAudioPlayer, gm.audioManager.obstacleHit.name, 1F);
					}
				}
				else
				{
					gm.playSound(obstacleAudioPlayer, gm.audioManager.obstacleHit.name, 1F);
				}
			}
		}

		if (destroyOnTimer != 0)
		{
			Destroy(this.gameObject);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actorHit = collision.gameObject.GetComponent<Actor>();
		if (actorHit != null)
		{
			/* obstacle is being held by player */
			if (beingHeld)
			{
				MBeast playerBeast = GetComponentInParent<MBeast>();
				if (playerBeast != null)
				{
					playerBeast.triggerCollision(collision);
				}
			}
		}

		if (destroyOnTimer != 0)
		{
			Destroy(this.gameObject);
		}
	}
}