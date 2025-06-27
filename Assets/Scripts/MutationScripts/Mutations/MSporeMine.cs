using System.Collections;
using UnityEngine;

public class MSporeMine : MonoBehaviour
{
	public Animator anim;

	public Collider2D triggerCollider;
	public Collider2D explosionCollider;

	public AudioClip armSound;
	public AudioClip explosionSound;
	public AudioClip triggerSound;

	public AudioSource mineAudioPlayer;

	public SpriteRenderer mineSprite;

	private float armTimer = 0;
	private float triggerTimer = 0;
	private bool destroy = false;

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

		if (destroy)
		{
			if (!mineAudioPlayer.isPlaying)
			{
				Destroy(gameObject);
			}
		}
	}

	public void arm()
	{
		triggerCollider.enabled = true;
		mineSprite.color = Color.white;
		GameManager.Instance.playSound(mineAudioPlayer, armSound.name, 0.7F);
	}

	public void destroyThis()
	{
		destroy = true;
		explosionCollider.enabled = false;
		triggerCollider.enabled = false;
	}

	public void trigger()
	{
		mineSprite.enabled = false;
		anim.SetTrigger(MutationDefs.TRIGGER_SPORE_MINE);
		GameManager.Instance.playSound(mineAudioPlayer, explosionSound.name, 1F);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Actor actorHit = collision.GetComponentInChildren<Actor>();
		if (actorHit != null && !actorHit.tag.Contains(ActorDefs.playerTag) && !triggered)
		{
			triggerTimer = triggerLength;
			triggerCollider.enabled = false;
			mineSprite.color = GameManager.COLOR_RED;
			GameManager.Instance.playSound(mineAudioPlayer, triggerSound.name, 0.7F);
			triggered = true;
		}
	}
}