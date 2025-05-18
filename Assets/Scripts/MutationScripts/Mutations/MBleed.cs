using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MBleed : MonoBehaviour, MutationInterface
{
	MutationScriptable mutationScriptable;
	public Sprite icon;
	public void Start()
	{
		//get mutationscriptable
	}

	public string getDisplayName()
	{
		return "";
	}
	public Sprite getIcon()
	{
		return icon;
	}

	public string getId()
	{
		return "MBleed";
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.DAMAGE_GIVEN;
	}

	public void trigger(Actor actorTarget)
	{
		/* create new bleed, attach it, and initialize it */
		EffectDefs.effectApply(actorTarget, actorTarget.gameManager.effectManager.bleed1);
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

	Sprite[] MutationInterface.getTutorialSprites()
	{
		return mutationScriptable.tutorialSprites;
	}
}