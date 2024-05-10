using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MBleed : MonoBehaviour, MutationInterface
{
	MutationScriptable mutationScriptable;

	public void Start()
	{
		//get mutationscriptable
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.DAMAGE_GIVEN;
	}

	public void trigger(Actor actorTarget)
	{
		/* create new bleed, attach it, and initialize it */
		EBleed newBleed = actorTarget.effectHolder.transform.AddComponent<EBleed>();
		GameManager manager = GameManager.Instance;

		newBleed.init(manager.getEffectScriptable(GameManager.EFCT_SCRIP_ID_BLEED1));
		newBleed.start(actorTarget);
	}

	public bool mEquip(Actor actor)
	{

		return true;
	}
}