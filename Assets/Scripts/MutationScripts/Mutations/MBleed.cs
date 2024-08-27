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
		EffectDefs.effectApply(actorTarget, GameManager.EFCT_SCRIP_ID_BLEED1);
	}

	public MutationInterface mEquip(Actor actor)
	{

		return this;
	}
	public void setStartingPosition()
	{

	}

	public void setWielder(Actor wielder)
	{

	}
}