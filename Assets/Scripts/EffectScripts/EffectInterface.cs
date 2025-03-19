using System.Collections;
using UnityEngine;
public interface EffectInterface
{
	public float getCompareValue();

	public EffectScriptable getScriptable();

	public float getTimer();

	public void init(EffectScriptable scriptable);

	public bool isPermanent();

	public void tick(float seconds);

	public void startEffect(Actor target);
}