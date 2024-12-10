using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Obstacle : MonoBehaviour
{
	public ObstacleScriptable _obstacleScriptable;
	public Rigidbody2D obstacleBody;

	private RigidbodyType2D obstacleType;
	private bool physicsEnabled = false;
	private bool thresholdPassed = false;
	private float velocityThreshold = 0.05F;

	// Use this for initialization
	void Start()
	{
		obstacleType = obstacleBody.bodyType;
		physicsEnabled = false;
	}

	private void Update()
	{
		if (physicsEnabled)
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
			}
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
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Actor actorHit = collision.gameObject.GetComponent<Actor>();
		if (actorHit != null)
		{	
			Vector2 velocityDiff = obstacleBody.velocity - (Vector2)actorHit.currMoveVector;

			Debug.Log("Obstacle/Actor diff: " + velocityDiff.magnitude);
			if (velocityDiff.magnitude >= _obstacleScriptable.collisionDamageThreshold && obstacleBody.velocity.magnitude > _obstacleScriptable.collisionDamageThreshold / 2)
			{
				actorHit.actorBody.AddForce(velocityDiff * _obstacleScriptable.actorPushForce);
				EffectDefs.effectApply(actorHit, actorHit.gameManager.effectManager.stunHalf);
				/* hitstop if player is hit */
				actorHit.takeDamage(_obstacleScriptable.collisionDamage, actorHit);
			}
		}
	}
}