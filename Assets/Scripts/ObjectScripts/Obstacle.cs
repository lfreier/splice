using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class Obstacle : MonoBehaviour
{
	public ObstacleScriptable _obstacleScriptable;
	public Rigidbody2D obstacleBody;
	public SpriteRenderer spriteRenderer;
	public Sprite secondSprite;

	private RigidbodyType2D obstacleType;
	private bool physicsEnabled = false;
	private bool thresholdPassed = false;
	private float velocityThreshold = 0.05F;
	public float knockVelocityThreshold = 5F;
	public float angVelocityThreshold = 20F;

	private float thresholdTimer;

	public float knockOverDrag = 3F;

	// Use this for initialization
	void Start()
	{
		obstacleType = obstacleBody.bodyType;
		physicsEnabled = false;
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
	}

	private void pushByDamage(WeaponInterface weapon)
	{
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
				GameManager gm = GameManager.Instance;
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
		Actor actorHit = collision.gameObject.GetComponent<Actor>();
		if (actorHit != null)
		{	
			Vector2 velocityDiff = obstacleBody.velocity - (Vector2)actorHit.currMoveVector;

			//Debug.Log("Obstacle/Actor diff: " + velocityDiff.magnitude);
			if (velocityDiff.magnitude >= _obstacleScriptable.collisionDamageThreshold)
			{
				knockOver();
				if (obstacleBody.velocity.magnitude > _obstacleScriptable.collisionDamageThreshold / 2)
				{
					actorHit.actorBody.AddForce(velocityDiff * _obstacleScriptable.actorPushForce);
					EffectDefs.effectApply(actorHit, actorHit.gameManager.effectManager.stunHalf);
					/* hitstop if player is hit */
					actorHit.takeDamage(_obstacleScriptable.collisionDamage, actorHit);
				}
			}
		}
	}
}