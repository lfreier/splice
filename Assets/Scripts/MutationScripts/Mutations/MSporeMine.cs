using System.Collections;
using UnityEngine;

public class MSporeMine : MonoBehaviour
{
	public Collider2D triggerCollider;
	public Collider2D explosionCollider;

	private float armTimer = 0;
	private float triggerTimer = 0;

	private static float armingLength = 1.5F;
	private static float triggerLength = 0.5F;

	void Start()
	{
		armTimer = armingLength;
		triggerTimer = 0;
	}

	private void FixedUpdate()
	{
		if (armTimer != 0)
		{
			armTimer -= Time.deltaTime;
			if (armTimer <= 0)
			{
				arm();
			}
		}

		if (triggerTimer != 0)
		{
			triggerTimer -= Time.deltaTime;
			if (triggerTimer <= 0)
			{
				trigger();
			}
		}
	}

	public void arm()
	{
		triggerCollider.enabled = true;
	}

	public void trigger()
	{
		explosionCollider.enabled = true;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actorHit = collision.GetComponentInChildren<Actor>();
		if (actorHit != null && actorHit.tag != ActorDefs.playerTag)
		{
			triggerTimer = triggerLength;
		}
	}
}