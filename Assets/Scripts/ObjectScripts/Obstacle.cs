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
			Debug.Log("Diff magnitude: " + velocityDiff.magnitude); 
			Debug.Log("Obstacle magnitude: " + obstacleBody.velocity.magnitude);

			if (velocityDiff.magnitude >= _obstacleScriptable.collisionDamageThreshold && obstacleBody.velocity.magnitude > _obstacleScriptable.collisionDamageThreshold / 2)
			{
				actorHit.actorBody.AddForce(velocityDiff * (_obstacleScriptable.pushbackForce * 1000));
				actorHit.takeDamage(_obstacleScriptable.collisionDamage);
			}
		}

		WeaponInterface weaponHit = collision.gameObject.GetComponent<WeaponInterface>();
		if (weaponHit != null)
		{
			WeaponScriptable weapData = weaponHit.getScriptable();
			Vector3 weapPos = collision.transform.parent.transform.position;
			Vector3 force = Vector3.ClampMagnitude(obstacleBody.transform.position - weapPos, 1);
			force *= weapData.damage * 1000;
			obstacleBody.AddForce(force);
		}
	}
}