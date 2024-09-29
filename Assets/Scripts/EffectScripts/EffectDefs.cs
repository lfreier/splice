using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EffectDefs
{

	public enum constantType
	{
		NONE = 0,
		STUN = 1,
		IFRAME = 2
	}

	public static void effectApply(Actor target, string effectScriptable)
	{
		/* create new effect, attach it, and initialize it */
		GameManager manager = GameManager.Instance;
		EffectScriptable effectData = manager.getEffectScriptable(effectScriptable);

		GameObject prefab = GameObject.Instantiate(effectData.effectPrefab, Vector3.zero, Quaternion.identity);
		EffectInterface newEffect = prefab.GetComponent<EffectInterface>();

		foreach (var repeatEffect in target.effectHolder.GetComponentsInChildren(newEffect.GetType()))
		{
			if (repeatEffect != null)
			{
				EffectInterface currEffect = repeatEffect as EffectInterface;
				if ((effectData.constantEffectType != constantType.NONE && currEffect.getScriptable().constantEffectType != constantType.NONE)
					&& effectData.constantEffectType != currEffect.getScriptable().constantEffectType)
				{
					continue;
				}
				if (currEffect.getCompareValue() >= newEffect.getCompareValue())
				{
					GameObject.Destroy(prefab);
					return;
				}
				else
				{
					GameObject.Destroy(repeatEffect.gameObject);
				}
			}
		}
		
		prefab.transform.SetParent(target.effectHolder.transform);
		prefab.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		
		newEffect.init(effectData);
		newEffect.start(target);
	}
}