using System.Collections;
using UnityEngine;

public class MSpider : MonoBehaviour, MutationInterface
{
	public Animator anim;

	public Sprite icon;

	private void OnDestroy()
	{
		GameManager gm = GameManager.Instance;
		gm.playerAbilityEvent -= abilityInputPressed;
	}

	private void abilityInputPressed()
	{

	}

	public string getDisplayName()
	{
		return MutationDefs.NAME_SPIDER;
	}
	public Sprite getIcon()
	{
		return icon;
	}
	public string getId()
	{
		return "MSpider";
	}

	public mutationTrigger getMutationType()
	{
		return mutationTrigger.ACTIVE_SLOT;
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
}