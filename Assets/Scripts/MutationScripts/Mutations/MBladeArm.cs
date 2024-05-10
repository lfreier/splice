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

	public bool mEquip(Actor actor)
	{
		/* Instantiate the weapon prefab, then equip it. */
		GameObject bladeArmPrefab = actor.instantiateWeapon(actor.gameManager.weapPBladeArm);
		actor.equip(bladeArmPrefab);

		return true;
	}
}