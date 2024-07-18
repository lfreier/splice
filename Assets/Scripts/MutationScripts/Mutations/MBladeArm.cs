using System.Collections;
using UnityEngine;

public class MBladeArm : MonoBehaviour, MutationInterface
{
	public mutationTrigger getMutationType()
	{
		return mutationTrigger.IS_WEAPON;
	}

	public void trigger(Actor actorTarget)
	{

	}

	public MutationInterface mEquip(Actor actor)
	{
		/* Instantiate the weapon prefab, then equip it. */
		GameObject bladeArmPrefab = actor.instantiateWeapon(actor.gameManager.weapPBladeArm);
		actor.equip(bladeArmPrefab);

		return this;
	}
	public void setStartingPosition()
	{
	}

	public void setWielder(Actor wielder)
	{
	}
}