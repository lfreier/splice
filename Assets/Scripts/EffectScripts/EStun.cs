using System.Collections;
using UnityEngine;

public class EStun: MonoBehaviour, EffectInterface
{
	public float timer;
	public float tickTimer;

	public Actor attachedActor;

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
		tickTimer = effectScriptable.tickLength;

		if (timer <= 0)
		{
			attachedActor.setStun(false);
			Debug.Log(attachedActor.name + " no longer stunned");
			Destroy(this);
		}
	}

	public void start(Actor target)
	{
		attachedActor = target;

		attachedActor.setStun(true);

		timer = effectScriptable.effectLength;
		tickTimer = effectScriptable.tickLength;
	}
}