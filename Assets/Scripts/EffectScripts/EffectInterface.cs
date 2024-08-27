using System.Collections;
using UnityEngine;
public interface EffectInterface
{
	public float getCompareValue();

	public float getTimer();

	public void init(EffectScriptable scriptable);

	public bool isPermanent();

	public void tick(float seconds);

	public void start(Actor target);
}