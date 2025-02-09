using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static EffectDefs;

public class EConstant: MonoBehaviour, EffectInterface
{
	public float timer;
	public float tickTimer;

	public Actor attachedActor;

	public EffectScriptable effectScriptable;

	public bool permanent = false;

	private bool currFlash = false;

	private SpriteRenderer effectSprite;

	void Update()
	{
		timer -= Time.deltaTime;
		tickTimer -= Time.deltaTime;

		if (effectScriptable.constantEffectType == constantType.STUN)
		{
			effectSprite.transform.Rotate(this.transform.forward, Time.deltaTime * 75) ;
		}

		if (tickTimer <= 0)
		{
			tick(effectScriptable.tickLength);
		}
	}

	public float getCompareValue()
	{
		if (timer > 0)
		{
			return timer;
		}

		return effectScriptable.effectLength;
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
		return permanent;
	}

	public void tick(float seconds)
	{
		tickTimer = effectScriptable.tickLength;

		switch (effectScriptable.constantEffectType)
		{
			case constantType.IFRAME:
			if (currFlash)
			{
				attachedActor.setColor(Color.white);
				currFlash = false;
			}
			else
			{
				attachedActor.setColor(GameManager.COLOR_IFRAME);
				currFlash = true;
			}
				break;
			case constantType.STUN:
				break;
			default:
				break;
		}

		if (timer <= 0 && !permanent)
		{
			switch (effectScriptable.constantEffectType)
			{
				case constantType.IFRAME:
					attachedActor.setColor(Color.white);
					break;
				case constantType.STUN:
					break;
				default:
					break;
			}

			attachedActor.setConstant(false, effectScriptable.constantEffectType);
			//Debug.Log(attachedActor.name + " no longer suffering effect");
			Destroy(this.gameObject);
		}
	}

	public void start(Actor target)
	{
		attachedActor = target;
		currFlash = false;
		if (effectScriptable.effectStrength > 0)
		{
			attachedActor.setConstant(true, effectScriptable.constantEffectType);
		}

		if (effectScriptable.effectSprite != null)
		{
			SpriteRenderer stunSprite = gameObject.GetComponent<SpriteRenderer>();
			if (stunSprite != null)
			{
				stunSprite.enabled = true;
				this.effectSprite = stunSprite;
			}
		}

		timer = effectScriptable.effectLength;
		tickTimer = effectScriptable.tickLength;

		if (timer < 0)
		{
			permanent = true;
		}

		tick(tickTimer);
	}
}