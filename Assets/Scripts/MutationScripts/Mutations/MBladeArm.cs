using System.Collections;
using UnityEngine;

public class MBladeArm : MonoBehaviour, MutationInterface
{
	public Sprite icon;

	public string getDisplayName()
	{
		return "BLADE ARM";
	}
	public Sprite getIcon()
	{
		return icon;
	}
	public string getId()
	{
		return "MBladeArm";
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.IS_WEAPON;
	}

	public void trigger(Actor actorTarget)
	{

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
		return null;
	}

	public void updateCells(int amount)
	{

	}
}