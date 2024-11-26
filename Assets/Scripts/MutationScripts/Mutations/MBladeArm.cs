using System.Collections;
using UnityEngine;

public class MBladeArm : MonoBehaviour, MutationInterface
{
	public Sprite icon;

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
}