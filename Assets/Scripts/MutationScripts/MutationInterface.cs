using System.Collections;
using UnityEngine;

public interface MutationInterface
{
	public mutationTrigger getMutationType();
	public void trigger(Actor actorTarget);

	/* Create a new instance of the mutation and equip it to the actor player */
	public bool mEquip(Actor actor);
}