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

	//private float susRotate;
	private bool tickBool;

	private bool currFlash = false;

	private SpriteRenderer effectSprite;

	void FixedUpdate()
	{
		timer -= Time.deltaTime;
		tickTimer -= Time.deltaTime;

		if (effectScriptable.constantEffectType == constantType.STUN)
		{
			effectSprite.transform.Rotate(this.transform.forward, Time.deltaTime * 75) ;
		}
		/*
		else if (effectScriptable.constantEffectType == constantType.SUS)
		{
			transform.Rotate(this.transform.forward, Time.deltaTime * susRotate);
			if (Mathf.Abs(transform.rotation.z) >= effectScriptable.effectStrength)
			{
				susRotate = -susRotate;
			}
		}
		*/
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
			case constantType.SUS:
			case constantType.SEEKING:
			case constantType.NOTICE:
				if (transform.rotation == Quaternion.identity)
				{
					if (effectScriptable.constantEffectType == constantType.NOTICE)
					{
						effectSprite.transform.localScale = new Vector3(1.3F, 1.3F);
					}
					else if (effectScriptable.constantEffectType == constantType.SEEKING)
					{
						effectSprite.transform.localScale = new Vector3(1.3F, 1.3F);
					}
					else
					{
						effectSprite.transform.localScale = new Vector3(1.1F, 1.1F);
					}

					if (tickBool)
					{
						tickBool = false;
						transform.Rotate(new Vector3(0, 0, effectScriptable.effectStrength));
					}
					else
					{
						tickBool = true;
						transform.Rotate(new Vector3(0, 0, -effectScriptable.effectStrength));
					}
				}
				else
				{
					effectSprite.transform.localScale = new Vector3(1, 1);
					tickBool = transform.rotation.z < 0;
					transform.rotation = Quaternion.identity;
				}
				break;
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
			if (attachedActor.displayedEffect != null && attachedActor.displayedEffect.effectScriptable.constantEffectType == effectScriptable.constantEffectType)
			{
				attachedActor.displayedEffect = null;
			}
			//Debug.Log(attachedActor.name + " no longer suffering effect");
			Destroy(this.gameObject);
		}
	}

	public void startEffect(Actor target)
	{
		attachedActor = target;
		currFlash = false;

		// remove other displayed effects - lower is higher priority
		if (attachedActor.displayedEffect != null &&  this.effectScriptable.constantEffectType < attachedActor.displayedEffect.effectScriptable.constantEffectType)
		{
			Destroy(attachedActor.displayedEffect.gameObject);
		}

		if (effectScriptable.effectStrength > 0)
		{
			attachedActor.setConstant(true, effectScriptable.constantEffectType);
		}

		if (effectScriptable.effectSprite != null)
		{
			SpriteRenderer newSprite = gameObject.GetComponentInChildren<SpriteRenderer>();
			if (newSprite != null)
			{
				newSprite.enabled = true;
				this.effectSprite = newSprite;
				attachedActor.displayedEffect = this;
			}
		}

		timer = effectScriptable.effectLength;
		tickTimer = effectScriptable.tickLength;

		// set permanent effects
		if (timer < 0 || effectScriptable.constantEffectType == constantType.SUS || effectScriptable.constantEffectType == constantType.SEEKING)
		{
			permanent = true;
		}

		tick(tickTimer);
	}
}