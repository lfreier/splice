using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Obstacle : MonoBehaviour
{
	public ObstacleScriptable _obstacleScriptable;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Actor actorHit = collision.gameObject.GetComponent<Actor>();
		if (actorHit != null)
		{
			Vector2 velocityDiff = collision.rigidbody.velocity - collision.otherRigidbody.velocity;
			if (velocityDiff.magnitude >= _obstacleScriptable.collisionDamageThreshold)
			{
				actorHit.takeDamage(_obstacleScriptable.collisionDamage);
			}
		}
	}
}