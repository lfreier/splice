using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Obstacle : MonoBehaviour
{
	public ObstacleScriptable _obstacleScriptable;
	public Rigidbody2D obstacleBody;

	// Use this for initialization
	void Start()
	{

	}

	private void pushByDamage(WeaponInterface weapon)
	{
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
				EffectDefs.effectApply(actorHit, GameManager.EFCT_SCRIP_ID_STUNHALF);
				actorHit.takeDamage(_obstacleScriptable.collisionDamage);
			}
		}
	}
}