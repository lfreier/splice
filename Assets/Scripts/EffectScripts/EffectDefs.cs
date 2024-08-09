using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EffectDefs
{
	public static void effectStun(Actor attacker, string stunScriptable)
	{
		/* create new stun, attach it, and initialize it */
		EStun newStun = attacker.effectHolder.transform.AddComponent<EStun>();
		GameManager manager = GameManager.Instance;

		newStun.init(manager.getEffectScriptable(stunScriptable));
		newStun.start(attacker);
	}
}