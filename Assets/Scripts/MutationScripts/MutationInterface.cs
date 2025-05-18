using System.Collections;
using UnityEngine;

public interface MutationInterface
{
	public string getDisplayName();

	public Sprite getIcon();

	public string getId();
	public mutationTrigger getMutationType();

	public Sprite[] getTutorialSprites();

	public void trigger(Actor actorTarget);

	/* Create a new instance of the mutation and equip it to the actor player */
	public MutationInterface mEquip(Actor actor);

	public void setStartingPosition();

	public void setWielder(Actor wielder);

}