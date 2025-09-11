using System.Collections;
using UnityEngine;

public class EBleed : MonoBehaviour, EffectInterface
{
	public float timer;
	public float tickTimer;

	public float bleedDamage;

	public Actor attachedActor;

	public AudioClip bleedSound;

	public EffectScriptable effectScriptable;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		timer -= Time.deltaTime;
		tickTimer -= Time.deltaTime;

		if (tickTimer <= 0)
		{
			tick(effectScriptable.tickLength);
		}
	}

	public float getCompareValue()
	{
		if (timer > 0)
		{
			return timer * effectScriptable.effectStrength;
		}

		return effectScriptable.effectLength * effectScriptable.effectStrength;
	}

	public EffectScriptable getScriptable()
	{
		return effectScriptable;
	}

	public float getTimer()
	{
		return timer;
	}

	public void init(EffectScriptable scriptable)
	{
		effectScriptable = scriptable;
	}

	public bool isPermanent()
	{
		return false;
	}

	public void tick(float seconds)
	{
		attachedActor.takeDamage(bleedDamage);
		tickTimer = effectScriptable.tickLength;

		if (bleedSound != null)
		{
			attachedActor.gameManager.playSound(attachedActor.actorAudioSource, bleedSound.name, 1F);
		}

		Debug.Log(attachedActor.name + "bleeds for " + bleedDamage + " damage");

		if (timer <= 0)
		{
			Destroy(this.gameObject);
		}
	}

	public void startEffect(Actor target)
	{
		attachedActor = target;

		timer = effectScriptable.effectLength;
		tickTimer = effectScriptable.tickLength;

		if (effectScriptable.effectSprite != null)
		{
			SpriteRenderer newSprite = gameObject.GetComponentInChildren<SpriteRenderer>();
			if (newSprite != null)
			{
				newSprite.sprite = effectScriptable.effectSprite;
				newSprite.size = new Vector2(1, 1);
				newSprite.enabled = true;
			}
		}

		bleedDamage = effectScriptable.effectStrength;
	}
}