using System.Collections;
using UnityEngine;

public class MSporeMine : MonoBehaviour
{
	public Animator anim;

	public Collider2D triggerCollider;
	public Collider2D explosionCollider;

	public SpriteRenderer mineSprite;

	private float armTimer = 0;
	private float triggerTimer = 0;

	private bool triggered = false;

	public float armingLength = 1.2F;
	public float triggerLength = 0.45F;

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
				armTimer = 0;
			}
		}

		if (triggerTimer != 0)
		{
			triggerTimer -= Time.deltaTime;
			if (triggerTimer <= 0)
			{
				trigger();
				triggerTimer = 0;
			}
		}
	}

	public void arm()
	{
		triggerCollider.enabled = true;
		mineSprite.color = Color.white;
	}

	public void destroyThis()
	{
		Destroy(gameObject);
	}

	public void trigger()
	{
		mineSprite.enabled = false;
		anim.SetTrigger(MutationDefs.TRIGGER_SPORE_MINE);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actorHit = collision.GetComponentInChildren<Actor>();
		if (actorHit != null && !actorHit.tag.Contains(ActorDefs.playerTag) && !triggered)
		{
			triggerTimer = triggerLength;
			triggerCollider.enabled = false;
			mineSprite.color = GameManager.COLOR_RED;
			triggered = true;
		}
	}
}